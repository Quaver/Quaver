using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Transitions;
using Quaver.Shared.Input.Global;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Loading;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.Selection.Components;
using Quaver.Shared.Screens.Selection.UI;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Quaver.Shared.Screens.Tournament;
using Quaver.Shared.Screens.V2.SkinEditor;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Selection
{
    /// <summary>
    ///     Owns Song Select V2 state, input handling, map selection and gameplay navigation
    /// </summary>
    internal sealed class SelectionScreen : SkinV2Screen, IHasLeftPanel, ISelectionScreenState
    {
        public override QuaverScreenType Type { get; } = QuaverScreenType.Select;

        internal List<Mapset> AvailableMapsets { get; private set; }

        internal event Action LibraryChanged;

        public Bindable<SelectContainerPanel> ActiveLeftPanel { get; set; } = new Bindable<SelectContainerPanel>(SelectContainerPanel.Leaderboard);

        SelectScrollContainerType ISelectionScreenState.ActiveScrollContainer => SelectScrollContainerType.Mapsets;

        SelectContainerPanel ISelectionScreenState.ActiveLeftPanel => ActiveLeftPanel.Value;

        private bool IsMultiplayer { get; }
        private SelectJukebox Jukebox { get; }
        private GlobalInputScopeToken InputToken { get; }
        private bool LibraryRefreshScheduled { get; set; }
        private bool Destroyed { get; set; }

        protected override ISkinV2EditorHost SkinEditorHost => (SelectionScreenView)View;

        internal bool CanHandleInput => !Destroyed && !Exiting && !IsSkinEditorActive && DialogManager.Dialogs.Count == 0 && ReferenceEquals(((QuaverGame)GameBase.Game).CurrentScreen, this);

        // Keep the factory signature compatible while the remaining panels are rebuilt.
        internal SelectionScreen(SelectScrollContainerType? activeScrollContainer = null, SelectContainerPanel activeLeftPanel = SelectContainerPanel.Leaderboard)
        {
            ActiveLeftPanel.Value = activeLeftPanel;
            IsMultiplayer = OnlineManager.CurrentGame != null;
            if (ModManager.IsActivated(ModIdentifier.Paused))
                ModManager.RemoveMod(ModIdentifier.Paused);

            RefreshLibrary();
            Jukebox = new SelectJukebox(this);
            View = new SelectionScreenView(this);
            InputToken = new SelectionInputToken(this);

            MapManager.MapsetDeleted += OnLibraryChanged;
            MapManager.MapDeleted += OnLibraryChanged;
            MapManager.MapUpdated += OnLibraryChanged;
            MapManager.Selected.ValueChanged += OnSelectedMapChanged;

            if (IsMultiplayer)
                OnlineManager.Client?.SetGameCurrentlySelectingMap(true);
            else
                SetRichPresence();

            MapLoadingScreen.QueueStreamerFilesWrite();
        }

        public override void OnFirstUpdate()
        {
            GameBase.Game.GlobalUserInterface.Cursor.Show(1);
            GameBase.Game.GlobalUserInterface.Cursor.Alpha = 1;

            if (AudioEngine.Track?.IsPlaying == true)
                AudioEngine.Track.Fade(100, 300);

            SkinManager.StartWatching();
            ScreenExiting += OnScreenExiting;

            base.OnFirstUpdate();
        }

        public override void Update(GameTime gameTime)
        {
            if (CanHandleInput)
            {
                if (KeyboardManager.IsUniqueKeyPress(Keys.Down))
                {
                    if (!((SelectionScreenView)View).TryMoveDifficultySelection(1))
                        MoveSelection(1);
                }
                else if (KeyboardManager.IsUniqueKeyPress(Keys.Up))
                {
                    if (!((SelectionScreenView)View).TryMoveDifficultySelection(-1))
                        MoveSelection(-1);
                }
                else if (KeyboardManager.IsUniqueKeyPress(Keys.Right))
                    MoveSelection(1);
                else if (KeyboardManager.IsUniqueKeyPress(Keys.Left))
                    MoveSelection(-1);
            }

            Jukebox.Update(gameTime);
            base.Update(gameTime);
        }

        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.Selecting, -1, string.Empty, 0, string.Empty, 0);

        internal void SelectMapset(Mapset mapset)
        {
            if (CanHandleInput && AvailableMapsets.Contains(mapset) &&
                !mapset.Maps.Contains(MapManager.Selected.Value))
                MapManager.SelectMapFromMapset(mapset);
        }

        internal void SelectMap(Map map)
        {
            if (CanHandleInput && map != null && AvailableMapsets.Any(set => set.Maps.Contains(map)))
                MapManager.Selected.Value = map;
        }

        internal void HandleBackAction()
        {
            if (CanHandleInput)
                ExitToMenu();
        }

        private void MoveSelection(int direction)
        {
            var selected = MapManager.Selected.Value;
            var index = AvailableMapsets.FindIndex(set => set.Maps.Contains(selected));
            var next = index < 0 ? 0 : index + direction;
            if (next >= 0 && next < AvailableMapsets.Count)
                SelectMapset(AvailableMapsets[next]);
        }

        private void RefreshLibrary()
        {
            AvailableMapsets = MapManager.Mapsets
                .Where(set => set.Maps != null && set.Maps.Count > 0).ToList();

            var selected = MapManager.Selected.Value;
            var current = selected == null ? null : AvailableMapsets.SelectMany(set => set.Maps)
                .FirstOrDefault(map => map.Md5Checksum == selected.Md5Checksum);

            if (current != null)
            {
                if (!ReferenceEquals(current, selected))
                    MapManager.Selected.Value = current;
            }
            else if (AvailableMapsets.Count > 0)
                MapManager.SelectMapFromMapset(AvailableMapsets[0]);
            else if (selected != null)
                MapManager.Selected.Value = null;
        }

        private void OnLibraryChanged(object sender, EventArgs args)
        {
            if (LibraryRefreshScheduled || Destroyed || Exiting)
                return;

            LibraryRefreshScheduled = true;
            View.Container.AddScheduledUpdate(() =>
            {
                if (Destroyed || Exiting)
                    return;

                LibraryRefreshScheduled = false;
                RefreshLibrary();
                LibraryChanged?.Invoke();
            });
        }

        private void OnSelectedMapChanged(object sender, BindableValueChangedEventArgs<Map> args) => MapLoadingScreen.QueueStreamerFilesWrite();

        private void OnScreenExiting(object sender, ScreenExitingEventArgs args) => SkinManager.StopWatching();

        private static void SetRichPresence()
        {
            DiscordHelper.Presence.PartySize = 0;
            DiscordHelper.Presence.PartyMax = 0;
            DiscordHelper.Presence.StartTimestamp = 0;
            DiscordHelper.Presence.EndTimestamp = 0;
            DiscordHelper.Presence.LargeImageText = OnlineManager.GetRichPresenceLargeKeyText(ConfigManager.SelectedGameMode.Value);
            DiscordHelper.Presence.SmallImageKey = ModeHelper.ToShortHand(ConfigManager.SelectedGameMode.Value).ToLower();
            DiscordHelper.Presence.SmallImageText = ModeHelper.ToLongHand(ConfigManager.SelectedGameMode.Value);
            Helpers.RichPresenceHelper.UpdateRichPresence("In the menus", "Selecting a song");
        }

        internal void ExitToGameplay()
        {
            var map = MapManager.Selected.Value;
            if (map == null)
                return;

            if (OnlineManager.CurrentGame != null)
            {
                SelectMultiplayerMap(map);
                return;
            }

            if (SkinManager.Skin?.SoundSelect != null)
            {
                AudioEngine.Track?.Pause();
                SkinManager.Skin.SoundSelect.CreateChannel().Play();
            }

            if (OnlineManager.IsSpectatingSomeone)
                OnlineManager.Client?.StopSpectating();

            if (ModManager.IsActivated(ModIdentifier.Coop))
            {
                Exit(() => new TournamentScreen(2));
                return;
            }

            Exit(() => QuaverScreenFactory.CreateMapLoading(new List<Score>()));
        }

        private void ExitToMenu()
        {
            Exit(() =>
            {
                if (IsMultiplayer)
                {
                    OnlineManager.Client?.SetGameCurrentlySelectingMap(false);
                    return QuaverScreenFactory.CreateMultiplayerGame();
                }

                return QuaverScreenFactory.CreateMainMenu();
            });
        }

        private void SelectMultiplayerMap(Map map)
        {
            var game = OnlineManager.CurrentGame;
            if (game == null)
                return;

            var difficulty = map.DifficultyFromMods(ModManager.Mods);
            if (difficulty < game.MinimumDifficultyRating || difficulty > game.MaximumDifficultyRating)
            {
                ShowMultiplayerConstraint("Screen_Multiplayer_DifficultyRange", $"{game.MinimumDifficultyRating:0.00} - {game.MaximumDifficultyRating:0.00}");
                return;
            }

            var length = map.SongLength * ModHelper.GetRateFromMods(ModManager.Mods) / 1000f;
            if (length > game.MaximumSongLength)
            {
                ShowMultiplayerConstraint("Screen_Multiplayer_MaximumSongLength", game.MaximumSongLength.ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (!game.AllowedGameModes.Contains((byte) map.Mode))
            {
                ShowMultiplayerConstraint("Screen_Multiplayer_AllowedGameModes", string.Empty);
                return;
            }

            if (map.LNPercentage < game.MinimumLongNotePercentage || map.LNPercentage > game.MaximumLongNotePercentage)
            {
                ShowMultiplayerConstraint("Screen_Multiplayer_LongNotePercentageRange", $"{game.MinimumLongNotePercentage:0} - {game.MaximumLongNotePercentage:0}");
                return;
            }

            Transitioner.FadeIn();
            ThreadScheduler.Run(() =>
            {
                OnlineManager.Client?.ChangeMultiplayerGameMap(map.Md5Checksum, map.MapId, map.MapSetId, map.ToString(), (byte) map.Mode, difficulty, map.GetDifficultyRatings(), map.GetJudgementCount(), map.GetAlternativeMd5());
                OnlineManager.Client?.SetGameCurrentlySelectingMap(false);
                View.Container.AddScheduledUpdate(() => Exit(() => QuaverScreenFactory.CreateMultiplayerGame()));
            });
        }

        private static void ShowMultiplayerConstraint(string localizationKey, string value) =>
            NotificationManager.Show(NotificationLevel.Error,  $"{LocalizationManager.Get(localizationKey)}{(string.IsNullOrEmpty(value) ? string.Empty : $": {value}")}");
        
        /// <summary>
        ///     Handle global selection actions while on Song Select screen
        /// </summary>
        private sealed class SelectionInputToken(SelectionScreen screen) : GlobalInputScopeToken
        {
            public override GlobalInputScope Scope => GlobalInputScope.Selection;

            public override GlobalInputHandleResult Handle(GlobalKeybindActions action,
                bool isKeyPress = true, bool isRelease = false)
            {
                if (!screen.CanHandleInput || !isKeyPress || isRelease)
                    return GlobalInputHandleResult.Pass;

                switch (action.BaseWithLayer())
                {
                    case GlobalKeybindActions.Back:
                        screen.HandleBackAction();
                        return GlobalInputHandleResult.Consumed;
                    case GlobalKeybindActions.NavigateSelect:
                        screen.ExitToGameplay();
                        return GlobalInputHandleResult.Consumed;
                    default:
                        return GlobalInputHandleResult.Pass;
                }
            }
        }

        public override void Destroy()
        {
            Destroyed = true;
            MapManager.MapsetDeleted -= OnLibraryChanged;
            MapManager.MapDeleted -= OnLibraryChanged;
            MapManager.MapUpdated -= OnLibraryChanged;
            MapManager.Selected.ValueChanged -= OnSelectedMapChanged;

            ScreenExiting -= OnScreenExiting;

            LibraryChanged = null;
            InputToken.Dispose();
            Jukebox.Destroy();
            ActiveLeftPanel.Dispose();
            SkinManager.StopWatching();

            base.Destroy();
        }
    }
}
