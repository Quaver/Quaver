/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Database.Settings;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Editor;
using Quaver.Shared.Screens.Importing;
using Quaver.Shared.Screens.Loading;
using Quaver.Shared.Screens.Menu;
using Quaver.Shared.Screens.Result;
using Quaver.Shared.Screens.Select.UI.Leaderboard;
using Quaver.Shared.Screens.Select.UI.Mapsets;
using Quaver.Shared.Screens.Select.UI.Modifiers;
using Quaver.Shared.Screens.Settings;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Select
{
    public class SelectScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Select;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///     The previous search term the user searched for.
        ///     Used to persist through screen changes.
        /// </summary>
        public static string PreviousSearchTerm { get; set; } = "";

        /// <summary>
        ///     The mapsets that are currently available to be displayed.
        /// </summary>
        public List<Mapset> AvailableMapsets { get; set; }

        /// <summary>
        ///     Is true if we're going from song select to gameplay.
        ///     True when exiting
        /// </summary>
        public bool IsExitingToGameplay { get; private set; }

        /// <summary>
        /// </summary>
        public SelectScreen()
        {
            // Go to the import screen if we've imported a map not on the select screen
            if (MapsetImporter.Queue.Count > 0 || QuaverSettingsDatabaseCache.OutdatedMaps.Count != 0 || MapDatabaseCache.MapsToUpdate.Count != 0)
            {
                Exit(() => new ImportingScreen());
                return;
            }

            // Grab the mapsets available to the user according to their previous search term.
            AvailableMapsets = MapsetHelper.SearchMapsets(MapManager.Mapsets, PreviousSearchTerm);

            // If no mapsets were found, just default to all of them.
            if (AvailableMapsets.Count == 0)
                AvailableMapsets = MapManager.Mapsets;

            AvailableMapsets = MapsetHelper.OrderMapsetsByConfigValue(AvailableMapsets);

            Logger.Debug($"There are currently: {AvailableMapsets.Count} available mapsets to play in select.",
                LogType.Runtime);

            DiscordHelper.Presence.Details = "Selecting a song";
            DiscordHelper.Presence.State = "In the menus";
            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);

            ConfigManager.AutoLoadOsuBeatmaps.ValueChanged += OnAutoLoadOsuBeatmapsChanged;
            ConfigManager.DisplayFailedLocalScores.ValueChanged += OnDisplayFailedScoresChanged;

            var game = GameBase.Game as QuaverGame;
            var cursor = game?.GlobalUserInterface.Cursor;
            cursor.Alpha = 1;

            View = new SelectScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            KeepPlayingAudioTrackAtPreview();
            HandleInput();

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable twice DelegateSubtraction
            ConfigManager.AutoLoadOsuBeatmaps.ValueChanged -= OnAutoLoadOsuBeatmapsChanged;
            ConfigManager.DisplayFailedLocalScores.ValueChanged -= OnDisplayFailedScoresChanged;

            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void OnFirstUpdate()
        {
            LoadOrFadeAudioTrack();
            base.OnFirstUpdate();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.Selecting,
            -1, "", (byte) ConfigManager.SelectedGameMode.Value, "", (long) ModManager.Mods);

        /// <summary>
        ///     Handles all input for the screen.
        /// </summary>
        private void HandleInput()
        {
            if (DialogManager.Dialogs.Count != 0 || Exiting)
                return;

            HandleKeyPressEscape();
            HandleKeyPressEnter();
            HandleKeyPressRight();
            HandleKeyPressLeft();
            HandleKeyPressControlRateChange();
            HandleKeyPressTab();
            HandleKeyPressF1();
            // HandleKeyPressF2(); // disabled for now, till the container issue is resolved.
            HandleMousePressRight();
        }

        /// <summary>
        ///     Plays the audio track at the preview time if it has stopped
        /// </summary>
        private void KeepPlayingAudioTrackAtPreview()
        {
            if (Exiting)
                return;

            if (AudioEngine.Track == null)
            {
                AudioEngine.PlaySelectedTrackAtPreview();
                return;
            }

            lock (AudioEngine.Track)
            {
                if (AudioEngine.Track.HasPlayed && AudioEngine.Track.IsStopped)
                    AudioEngine.PlaySelectedTrackAtPreview();
            }
        }

        /// <summary>
        ///     Handles when the user presses escape.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void HandleKeyPressEscape()
        {
            var view = View as SelectScreenView;

            if (!KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyNavigateBack.Value))
                return;

            switch (view.ActiveContainer)
            {
                case SelectContainerStatus.Mapsets:
                    ExitToMenu();
                    break;
                case SelectContainerStatus.Difficulty:
                    view.SwitchToContainer(SelectContainerStatus.Mapsets);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Handles when the user presses the enter key.
        /// </summary>
        private void HandleKeyPressEnter()
        {
            var view = View as SelectScreenView;

            if (!KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyNavigateSelect.Value) || AvailableMapsets.Count == 0)
                return;

            switch (view.ActiveContainer)
            {
                case SelectContainerStatus.Mapsets:
                    view.SwitchToContainer(SelectContainerStatus.Difficulty);
                    break;
                case SelectContainerStatus.Difficulty:
                    ExitToGameplay();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Handles when the user presses the right key
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void HandleKeyPressRight()
        {
            var view = View as SelectScreenView;

            if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt) ||
                KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt))
                return;

            if (!KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyNavigateRight.Value))
                return;

            switch (view.ActiveContainer)
            {
                case SelectContainerStatus.Mapsets:
                    view?.MapsetScrollContainer.SelectNextMapset(Direction.Forward);
                    break;
                case SelectContainerStatus.Difficulty:
                    view.DifficultyScrollContainer.SelectNextDifficulty(Direction.Forward);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Handles when the user presses the left key.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void HandleKeyPressLeft()
        {
            var view = View as SelectScreenView;

            if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt) ||
                KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt))
                return;

            if (!KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyNavigateLeft.Value))
                return;

            switch (view.ActiveContainer)
            {
                case SelectContainerStatus.Mapsets:
                    view?.MapsetScrollContainer.SelectNextMapset(Direction.Backward);
                    break;
                case SelectContainerStatus.Difficulty:
                    view.DifficultyScrollContainer.SelectNextDifficulty(Direction.Backward);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Gets the adjacent rate value.
        ///
        ///     For example, if the current rate is 1.0x, the adjacent value would be either 0.95x or 1.1x,
        ///     depending on the argument.
        /// </summary>
        /// <param name="faster">If true, returns the higher rate, otherwise the lower rate.</param>
        /// <returns></returns>
        private static float GetNextRate(bool faster)
        {
            var current = AudioEngine.Track.Rate;
            var adjustment = 0.1f;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (current < 1.0f || (current == 1.0f && !faster))
                adjustment = 0.05f;

            var next = current + adjustment * (faster ? 1f : -1f);
            return (float) Math.Round(next, 2);
        }

        /// <summary>
        ///     Handles when the user wants to increase/decrease the rate of the song.
        /// </summary>
        private static void HandleKeyPressControlRateChange()
        {
            if (!KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) &&
                !KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl))
                return;

            // Increase rate.
            if (KeyboardManager.IsUniqueKeyPress(Keys.OemPlus))
                ModManager.AddSpeedMods(GetNextRate(true));

            // Decrease Rate
            if (KeyboardManager.IsUniqueKeyPress(Keys.OemMinus))
                ModManager.AddSpeedMods(GetNextRate(false));

            // Change from pitched to non-pitched
            if (KeyboardManager.IsUniqueKeyPress(Keys.D0))
            {
                ConfigManager.Pitched.Value = !ConfigManager.Pitched.Value;
                Logger.Debug($"Audio Rate Pitching is {(ConfigManager.Pitched.Value ? "Enabled" : "Disabled")}",
                    LogType.Runtime);
            }
        }

        /// <summary>
        ///     Handles when the user presses the tab key
        /// </summary>
        private static void HandleKeyPressTab()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Tab))
                return;

            var index = (int) ConfigManager.LeaderboardSection.Value;

            if (index + 1 < Enum.GetNames(typeof(LeaderboardType)).Length)
                ConfigManager.LeaderboardSection.Value = (LeaderboardType) index + 1;
            else
                ConfigManager.LeaderboardSection.Value = LeaderboardType.Local;
        }

        /// <summary>
        ///     Handles when the user presses the F1 key
        /// </summary>
        private void HandleKeyPressF1()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.F1))
                return;

            DialogManager.Show(new ModifiersDialog());
        }

        /// <summary>
        ///     Handles when the user presses the F2 key
        /// </summary>
        private void HandleKeyPressF2()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.F2))
                return;

            SelectRandomMap();
        }

        /// <summary>
        ///     Handles when the user presses the right mouse button
        /// </summary>
        private void HandleMousePressRight()
        {
            var view = View as SelectScreenView;

            switch (view.ActiveContainer)
            {
                case SelectContainerStatus.Difficulty:
                    if (MouseManager.IsUniqueClick(MouseButton.Right))
                    {
                        view.SwitchToContainer(SelectContainerStatus.Mapsets);
                    }

                    break;
            }
        }

        /// <summary>
        ///    If we've already got a working AudioTrack for the selected map, then fade it in.
        ///     Otherwise load it up at its preview.
        /// </summary>
        private static void LoadOrFadeAudioTrack()
        {
            if (AudioEngine.Track != null)
            {
                if (AudioEngine.Track.IsStopped || AudioEngine.Track.IsDisposed || AudioEngine.Track.IsPaused ||
                    MapManager.GetAudioPath(AudioEngine.Map) != MapManager.GetAudioPath(MapManager.Selected.Value))
                {
                    MapsetScrollContainer.LoadNewAudioTrackIfNecessary();
                }
                else
                    AudioEngine.Track.Fade(ConfigManager.VolumeMusic.Value, 500);

                return;
            }

            MapsetScrollContainer.LoadNewAudioTrackIfNecessary();
        }

        /// <summary>
        ///     Exits the screen to schedule loading the map and ultimately the gameplay screen
        /// </summary>
        public void ExitToGameplay()
        {
            IsExitingToGameplay = true;

            Exit(() =>
            {
                var game = GameBase.Game as QuaverGame;
                var cursor = game.GlobalUserInterface.Cursor;
                cursor.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, cursor.Alpha, 0, 200));

                if (AudioEngine.Track != null)
                {
                    lock (AudioEngine.Track)
                        AudioEngine.Track?.Fade(10, 500);
                }

                return new MapLoadingScreen(new List<Score>());
            }, 100);
        }

        /// <summary>
        ///     Exits the screen back to the main menu
        /// </summary>
        public void ExitToMenu() => Exit(() =>
        {
            if (AudioEngine.Track != null)
            {
                lock (AudioEngine.Track)
                    AudioEngine.Track?.Fade(10, 300);
            }

            return new MenuScreen();
        });

        /// <summary>
        ///     Exits the screen to the editor
        /// </summary>
        public void ExitToEditor() => Exit(() =>
        {
            AudioEngine.Track?.Pause();

            try
            {
                return new EditorScreen(MapManager.Selected.Value.LoadQua(false));
            }
            catch (Exception)
            {
                NotificationManager.Show(NotificationLevel.Error, "Unable to read map file!");
                return new SelectScreen();
            }
        });

        /// <summary>
        ///     Exits the screen to results with a given score.
        /// </summary>
        /// <param name="score"></param>
        public void ExitToResults(Score score) => Exit(() => new ResultScreen(score));

        /// <summary>
        ///     Called when the user changes whether they want to load osu maps or not
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAutoLoadOsuBeatmapsChanged(object sender, BindableValueChangedEventArgs<bool> e) => Exit(() => new ImportingScreen());

        /// <summary>
        ///     Called when the user changes the option for displaying failed scores.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisplayFailedScoresChanged(object sender, BindableValueChangedEventArgs<bool> e)
        {
            if (ConfigManager.LeaderboardSection.Value == LeaderboardType.Local)
                ConfigManager.LeaderboardSection.Value = LeaderboardType.Local;
        }

        /// <summary>
        ///     Used to select a random mapset (or map if inside a mapset).
        /// </summary>
        public void SelectRandomMap()
        {
            var view = View as SelectScreenView;
            var rnd = new Random(DateTime.Now.Millisecond);
            var selectedMapsetIndex = view.MapsetScrollContainer.SelectedMapsetIndex;
            var selectedDifficultyIndex = view.DifficultyScrollContainer.SelectedMapIndex;

            switch (view.ActiveContainer)
            {
                case SelectContainerStatus.Mapsets:
                    var randomMapsetIndex = selectedMapsetIndex;

                    if (AvailableMapsets.Count <= 1)
                        return;

                    // To avoid selecting the mapset already selected.
                    do
                    {
                        randomMapsetIndex = rnd.Next(AvailableMapsets.Count);
                    } while (randomMapsetIndex == selectedMapsetIndex);

                    view.MapsetScrollContainer.SelectMapset(randomMapsetIndex);
                    break;

                case SelectContainerStatus.Difficulty:
                    var mapset = AvailableMapsets[selectedMapsetIndex];
                    var randomMapIndex = selectedDifficultyIndex;

                    if (mapset.Maps.Count <= 1)
                        return;

                    // To avoid selecting the mapset already selected.
                    do
                        randomMapIndex = new Random(DateTime.Now.Millisecond).Next(mapset.Maps.Count);
                    while (randomMapIndex == selectedDifficultyIndex);

                    view.MapsetScrollContainer.SelectMap(selectedMapsetIndex, mapset.Maps[randomMapIndex], true);
                    break;
            }
        }
    }
}
