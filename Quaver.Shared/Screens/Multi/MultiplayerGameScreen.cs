using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Loading;
using Quaver.Shared.Screens.MultiplayerLobby;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.Selection.UI;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Multi
{
    public sealed class MultiplayerGameScreen : QuaverScreen, IHasLeftPanel
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Multiplayer;

        /// <summary>
        ///     The current multiplayer game
        /// </summary>
        public Bindable<MultiplayerGame> Game { get; private set; }

        /// <summary>
        ///     The currently active panel on the left side of the screen
        /// </summary>
        public Bindable<SelectContainerPanel> ActiveLeftPanel { get; set; }

        /// <summary>
        ///     Keeps track of if the user is play testing in the map preview
        /// </summary>
        public Bindable<bool> IsPlayTestingInPreview { get; private set; }

        /// <summary>
        ///     If true, when the screen exits, it will not exit the user out of the game
        /// </summary>
        public bool DontLeaveGameUponScreenSwitch { get; set; }

        /// <summary>
        /// </summary>
        public MultiplayerGameScreen()
        {
            if (OnlineManager.CurrentGame == null)
            {
                Exit(() => new MultiplayerLobbyScreen());
                return;
            }

            CreateGameBindable();
            InitializeActiveLeftPanelBindable();
            InitializeTestPlayingBindable();

            ScreenExiting += (sender, args) =>
            {
                if (DontLeaveGameUponScreenSwitch)
                    return;

                SteamManager.SetRichPresence("steam_player_group", null);
                SteamManager.SetRichPresence("steam_player_group_size", null);

                OnlineManager.LeaveGame();
            };

            if (OnlineManager.Client != null)
            {
                OnlineManager.Client.OnGameMapChanged += OnMapChanged;
                OnlineManager.Client.OnGameStarted += OnGameStarted;
                OnlineManager.Client.OnGameNameChanged += OnGameNameChanged;
                OnlineManager.Client.OnUserJoinedGame += OnUserJoinedGame;
                OnlineManager.Client.OnUserLeftGame += OnUserLeftGame;
                OnlineManager.Client.OnGameMaxPlayersChanged += OnMaxPlayersChanged;
            }

            SetRichPresence();
            SelectMultiplayerMap();
            View = new MultiplayerGameScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void OnFirstUpdate()
        {
            if (OnlineManager.IsSpectatingSomeone)
                OnlineManager.Client?.StopSpectating();

            MapLoadingScreen.AddModsFromIdentifiers(OnlineManager.GetSelfActivatedMods());
            OnlineManager.SendGameDifficultyRatings(OnlineManager.CurrentGame.MapMd5, OnlineManager.CurrentGame.AlternativeMd5);
            
            base.OnFirstUpdate();
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (!Exiting)
                GameBase.Game.GlobalUserInterface.Cursor.Alpha = 1;

            HandleInput();
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Game?.Dispose();
            ActiveLeftPanel?.Dispose();
            IsPlayTestingInPreview?.Dispose();

            if (OnlineManager.Client != null)
            {
                OnlineManager.Client.OnGameMapChanged -= OnMapChanged;
                OnlineManager.Client.OnGameStarted -= OnGameStarted;
                OnlineManager.Client.OnGameNameChanged -= OnGameNameChanged;
                OnlineManager.Client.OnUserJoinedGame -= OnUserJoinedGame;
                OnlineManager.Client.OnUserLeftGame -= OnUserLeftGame;
                OnlineManager.Client.OnGameMaxPlayersChanged -= OnMaxPlayersChanged;
            }

            base.Destroy();
        }

        /// <summary>
        ///     Creates <see cref="Game"/>
        /// </summary>
        private void CreateGameBindable()
        {
            var game = OnlineManager.CurrentGame ?? GetTestGame();
            Game = new Bindable<MultiplayerGame>(game) {Value = game};
        }

        /// <summary>
        ///     Initializes the bindable which keeps track of which panel on the left side of the screen is active
        /// </summary>
        private void InitializeActiveLeftPanelBindable()
        {
            ActiveLeftPanel = new Bindable<SelectContainerPanel>(SelectContainerPanel.MatchSettings)
            {
                Value = SelectContainerPanel.MatchSettings
            };
        }

        /// <summary>
        ///     Initializes the bindable that determines if we're currently play testing
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void InitializeTestPlayingBindable()
            => IsPlayTestingInPreview = new Bindable<bool>(false) {Value = false};

        /// <summary>
        ///     Finds and selects the multiplayer map
        /// </summary>
        public static void SelectMultiplayerMap()
        {
            var map = MapManager.FindMapFromMd5(OnlineManager.CurrentGame.MapMd5);

            if (map == null)
                map = MapManager.FindMapFromMd5(OnlineManager.CurrentGame.AlternativeMd5);

            if (map == null)
            {
                if (AudioEngine.Track.IsPlaying)
                    AudioEngine.Track.Pause();

                MapManager.Selected.Value = null;
                OnlineManager.Client?.DontHaveMultiplayerGameMap();
                return;
            }

            MapManager.Selected.Value = map;
            OnlineManager.Client?.HasMultiplayerGameMap();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, GameMapChangedEventArgs e) => SelectMultiplayerMap();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameStarted(object sender, GameStartedEventArgs e)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.Multiplayer, -1, "-1", 1, "", 0);

        /// <summary>
        /// </summary>
        private void HandleInput()
        {
            if (Exiting)
                return;

            if (DialogManager.Dialogs.Count != 0)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.F1))
                HandleKeyPressF1();

            if (KeyboardManager.IsUniqueKeyPress(Keys.F2))
                HandleKeyPressF2();

            if (KeyboardManager.IsUniqueKeyPress(Keys.F3))
                HandleKeyPressF3();

            if (KeyboardManager.IsUniqueKeyPress(Keys.F4))
                HandleKeyPressF4();

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
            {
                if (ActiveLeftPanel.Value != SelectContainerPanel.MatchSettings)
                    ActiveLeftPanel.Value = SelectContainerPanel.MatchSettings;
                else
                {
                    Exit(() => new MultiplayerLobbyScreen());
                    return;
                }
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Tab) && ActiveLeftPanel.Value == SelectContainerPanel.Leaderboard)
                SelectionScreen.HandleKeyPressTab();

            HandleKeyPressControlInput();
            HandleKeyPressAltInput();
        }

        /// <summary>
        ///     Handles when the user holds control down and performs input actions
        /// </summary>
        private void HandleKeyPressControlInput()
        {
            if (!KeyboardManager.IsCtrlDown() || KeyboardManager.IsAltDown())
                return;

            // Increase rate.
            if (Game.Value.HostId == OnlineManager.Self?.OnlineUser?.Id || Game.Value.FreeModType.HasFlag(MultiplayerFreeModType.Rate))
            {
                // Increase rate.
                if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyIncreaseGameplayAudioRate.Value))
                    ModManager.AddSpeedMods(SelectionScreen.GetNextRate(true, KeyboardManager.IsShiftDown()));

                // Decrease Rate
                if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyDecreaseGameplayAudioRate.Value))
                    ModManager.AddSpeedMods(SelectionScreen.GetNextRate(false, KeyboardManager.IsShiftDown()));
            }
        }

        /// <summary>
        ///     Handles when the user holds ALT down and performs input actions
        /// </summary>
        private void HandleKeyPressAltInput()
        {
            if (!KeyboardManager.IsAltDown())
                return;

            if (MapManager.Selected.Value != null)
            {
                var offsetChange = KeyboardManager.IsCtrlDown() ? 1 : 5;

                if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyIncreaseMapOffset.Value))
                {
                    MapManager.Selected.Value.LocalOffset += offsetChange;
                    HandleOffsetChange();
                }
                else if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyDecreaseMapOffset.Value))
                {
                    MapManager.Selected.Value.LocalOffset -= offsetChange;
                    HandleOffsetChange();
                }
            }
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressF1()
        {
            if (ActiveLeftPanel.Value != SelectContainerPanel.Modifiers)
                ActiveLeftPanel.Value = SelectContainerPanel.Modifiers;
            else
                ActiveLeftPanel.Value = SelectContainerPanel.MatchSettings;
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressF2()
        {
            if (ActiveLeftPanel.Value != SelectContainerPanel.Leaderboard)
                ActiveLeftPanel.Value = SelectContainerPanel.Leaderboard;
            else
                ActiveLeftPanel.Value = SelectContainerPanel.MatchSettings;
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressF3()
        {
            if (ActiveLeftPanel.Value != SelectContainerPanel.MapPreview)
                ActiveLeftPanel.Value = SelectContainerPanel.MapPreview;
            else
                ActiveLeftPanel.Value = SelectContainerPanel.MatchSettings;
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressF4()
        {
            if (ActiveLeftPanel.Value != SelectContainerPanel.UserProfile)
                ActiveLeftPanel.Value = SelectContainerPanel.UserProfile;
            else
                ActiveLeftPanel.Value = SelectContainerPanel.MatchSettings;
        }

        /// <summary>
        /// </summary>
        private void HandleOffsetChange()
        {
            var map = MapManager.Selected.Value;

            if (map == null)
                return;

            NotificationManager.Show(NotificationLevel.Info, $"Local map offset changed to: {map.LocalOffset} ms");
            MapDatabaseCache.UpdateMap(map);
        }

        /// <summary>
        /// </summary>
        private void SetRichPresence()
        {
            // Friend Grouping of Steam's Enhanced Rich Presence
            SteamManager.SetRichPresence("steam_player_group", Game.Value.GameId.ToString());
            SteamManager.SetRichPresence("steam_player_group_size", Game.Value.PlayerIds.Count.ToString());

            DiscordHelper.Presence.LargeImageText = OnlineManager.GetRichPresenceLargeKeyText(ConfigManager.SelectedGameMode.Value);
            DiscordHelper.Presence.SmallImageKey = ModeHelper.ToShortHand(ConfigManager.SelectedGameMode.Value).ToLower();
            DiscordHelper.Presence.SmallImageText = ModeHelper.ToLongHand(ConfigManager.SelectedGameMode.Value);

            RichPresenceHelper.UpdateRichPresence($"{Game.Value.Name} ({Game.Value.PlayerIds.Count} of {Game.Value.MaxPlayers})", "Waiting to Start");
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private MultiplayerGame GetTestGame()
        {
            return new MultiplayerGame
            {
                Id = "abc123",
                GameId = 999,
                Name = "Example Game",
                Type = MultiplayerGameType.Friendly,
                Ruleset = MultiplayerGameRuleset.Battle_Royale,
                GameMode = (byte) GameMode.Keys4,
                PlayerIds = new List<int>(),
                MaxPlayers = 16,
                MapMd5 = "None",
                AlternativeMd5 = "None",
                MapId = -1,
                MapsetId = -1,
                Map = "Artist - Title [Example]",
                JudgementCount = 1000,
                HostId = 1,
                PlayersReady = new List<int>() { 3, 6, 13, 7 },
                RedTeamPlayers = new List<int> { 0, 1, 2, 3, 4, 5, 6 },
                BlueTeamPlayers = new List<int> { 7, 8, 9, 10, 11, 12, 13, 14},
                PlayersWithoutMap = new List<int>()
                {
                    8, 9, 7, 2
                },
                PlayerMods = new List<MultiplayerPlayerMods>()
                {
                    new MultiplayerPlayerMods()
                    {
                        UserId = 2,
                        Modifiers = ((long) (ModIdentifier.NoFail)).ToString()
                    },
                    new MultiplayerPlayerMods()
                    {
                        UserId = 3,
                        Modifiers = ((long) (ModIdentifier.Mirror | ModIdentifier.Speed07X)).ToString()
                    }
                },
                PlayerWins = new List<MultiplayerPlayerWins>()
                {
                    new MultiplayerPlayerWins()
                    {
                        UserId = 0,
                        Wins = 6
                    },
                    new MultiplayerPlayerWins()
                    {
                        UserId = 5,
                        Wins = 47
                    }
                },
                RefereeUserId = 11,
                HostRotation = true,
                FreeModType = MultiplayerFreeModType.Regular,
            };
        }

        private void OnGameNameChanged(object sender, GameNameChangedEventArgs e) => SetRichPresence();

        private void OnUserJoinedGame(object sender, UserJoinedGameEventArgs e) => SetRichPresence();

        private void OnUserLeftGame(object sender, UserLeftGameEventArgs e) => SetRichPresence();

        private void OnMaxPlayersChanged(object sender, MaxPlayersChangedEventArgs e) => SetRichPresence();
    }
}