using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Modifiers;
using Quaver.Screens.Loading;
using Quaver.Screens.Menu;
using Quaver.Screens.SongSelect.UI.Leaderboard;
using Quaver.Screens.SongSelect.UI.Mapsets;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Wobble.Discord;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Screens;

namespace Quaver.Screens.SongSelect
{
    public class SongSelectScreen : QuaverScreen
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
        /// </summary>
        public SongSelectScreen()
        {
            // Grab the mapsets available to the user according to their previous search term.
            AvailableMapsets = MapsetHelper.SearchMapsets(MapManager.Mapsets, PreviousSearchTerm);

            // If no mapsets were found, just default to all of them.
            if (AvailableMapsets.Count == 0)
                AvailableMapsets = MapManager.Mapsets;

            AvailableMapsets = MapsetHelper.OrderMapsetByConfigValue(AvailableMapsets);

            Logger.Debug($"There are currently: {AvailableMapsets.Count} available mapsets to play in select.", LogType.Runtime);

            DiscordManager.Client.CurrentPresence.Details = "Selecting a song";
            DiscordManager.Client.CurrentPresence.State = "In the Menus";
            DiscordManager.Client.SetPresence(DiscordManager.Client.CurrentPresence);

            View = new SongSelectScreenView(this);
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
            if (DialogManager.Dialogs.Count != 0)
                return;

            HandleKeyPressEscape();
            HandleKeyPressEnter();
            HandleKeyPressRight();
            HandleKeyPressLeft();
            HandleKeyPressControlRateChange();
            HandleKeyPressTab();
        }

        /// <summary>
        ///     Plays the audio track at the preview time if it has stopped
        /// </summary>
        private static void KeepPlayingAudioTrackAtPreview()
        {
            lock (AudioEngine.Track)
            {
                if (AudioEngine.Track == null)
                    return;

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
            var view = View as SongSelectScreenView;

            if (!KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                return;

            switch (view.ActiveContainer)
            {
                case SelectContainerStatus.Mapsets:
                    QuaverScreenManager.ScheduleScreenChange(() =>
                    {
                        AudioEngine.Track?.Fade(10, 500);
                        return new MenuScreen();
                    });
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
            var view = View as SongSelectScreenView;

            if (!KeyboardManager.IsUniqueKeyPress(Keys.Enter))
                return;

            switch (view.ActiveContainer)
            {
                case SelectContainerStatus.Mapsets:
                    view.SwitchToContainer(SelectContainerStatus.Difficulty);
                    break;
                case SelectContainerStatus.Difficulty:
                    QuaverScreenManager.ChangeScreen(new MapLoadingScreen(new List<LocalScore>()));
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
            var view = View as SongSelectScreenView;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Right))
            {
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
        }

        /// <summary>
        ///     Handles when the user presses the left key.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void HandleKeyPressLeft()
        {
            var view = View as SongSelectScreenView;

            if (!KeyboardManager.IsUniqueKeyPress(Keys.Left))
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
        ///     Handles when the user wants to increase/decrease the rate of the song.
        /// </summary>
        private static void HandleKeyPressControlRateChange()
        {
            if (!KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl))
                return;

            // Increase rate.
            if (KeyboardManager.IsUniqueKeyPress(Keys.OemPlus))
                ModManager.AddSpeedMods((float) Math.Round(AudioEngine.Track.Rate + 0.1f, 1));

            // Decrease Rate
            if (KeyboardManager.IsUniqueKeyPress(Keys.OemMinus))
                ModManager.AddSpeedMods((float) Math.Round(AudioEngine.Track.Rate - 0.1f, 1));

            // Change from pitched to non-pitched
            if (KeyboardManager.IsUniqueKeyPress(Keys.D0))
            {
                ConfigManager.Pitched.Value = !ConfigManager.Pitched.Value;
                Logger.Debug($"Audio Rate Pitching is {(ConfigManager.Pitched.Value ? "Enabled" : "Disabled")}", LogType.Runtime);
            }
        }

        /// <summary>
        ///     Handles when the user presses the tab key
        /// </summary>
        private static void HandleKeyPressTab()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Tab))
                return;

            if (ConfigManager.LeaderboardSection.Value == LeaderboardType.Local)
                ConfigManager.LeaderboardSection.Value = LeaderboardType.Global;
            else if (ConfigManager.LeaderboardSection.Value == LeaderboardType.Global)
                ConfigManager.LeaderboardSection.Value = LeaderboardType.Local;
        }

        /// <summary>
        ///    If we've already got a working AudioTrack for the selected map, then fade it in.
        ///     Otherwise load it up at its preview.
        /// </summary>
        private static void LoadOrFadeAudioTrack()
        {
            if (AudioEngine.Track != null)
            {
                if (AudioEngine.Track.IsStopped || AudioEngine.Track.IsDisposed || AudioEngine.Track.IsPaused)
                    MapsetScrollContainer.LoadNewAudioTrackIfNecessary();
                else
                    AudioEngine.Track.Fade(ConfigManager.VolumeMusic.Value, 500);

                return;
            }

            MapsetScrollContainer.LoadNewAudioTrackIfNecessary();
        }
    }
}