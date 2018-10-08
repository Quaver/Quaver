using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using osu_database_reader;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Audio;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Graphics.Notifications;
using Quaver.Modifiers;
using Quaver.Screens.Gameplay;
using Quaver.Screens.Loading;
using Quaver.Screens.Select.UI.Mods;
using Wobble.Discord;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Screens;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Quaver.Screens.Select
{
    public class SelectScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Select;

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///     The list of available mapsets in the screen.
        ///     This value can change as the user searches for maps.
        /// </summary>
        public List<Mapset> AvailableMapsets { get; set; }

        /// <summary>
        ///     The previous search term the user searched for.
        ///     Used to persist through screen changes.
        /// </summary>
        public static string PreviousSearchTerm { get; set; } = "";

        /// <summary>
        /// </summary>
        public SelectScreen()
        {
            // Grab the mapsets available to the user according to their previous search term.
            AvailableMapsets = MapsetHelper.SearchMapsets(MapManager.Mapsets, PreviousSearchTerm);

            // If no mapsets were found, just default to all of them.
            if (AvailableMapsets.Count == 0)
                AvailableMapsets = MapManager.Mapsets;

            AvailableMapsets = MapsetHelper.OrderMapsetByConfigValue(AvailableMapsets);

            DiscordManager.Client.CurrentPresence.Details = "Selecting a song";
            DiscordManager.Client.CurrentPresence.State = "In the menus";
            DiscordManager.Client.SetPresence(DiscordManager.Client.CurrentPresence);

            View = new SelectScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleInput();
            KeepPlayingAudioTrackAtPreview();

            base.Update(gameTime);
        }

        /// <summary>
        ///     Handles input for the select screen.
        /// </summary>
        private void HandleInput()
        {
            var screenView = (SelectScreenView) View;

            if (DialogManager.Dialogs.Count > 0)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Enter))
                QuaverScreenManager.ChangeScreen(new MapLoadingScreen(new List<LocalScore>()));

            if (KeyboardManager.IsUniqueKeyPress(Keys.F1))
                DialogManager.Show(new ModsDialog());

            if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) ||
                KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl))
            {
                // Increase rate.
                if (KeyboardManager.IsUniqueKeyPress(Keys.OemPlus))
                    ModManager.AddSpeedMods((float) Math.Round(AudioEngine.Track.Rate + 0.1f, 1));

                // Decrease Rate
                if (KeyboardManager.IsUniqueKeyPress(Keys.OemMinus))
                    ModManager.AddSpeedMods((float) Math.Round(AudioEngine.Track.Rate - 0.1f, 1));
            }
        }

        /// <summary>
        ///     Plays the audio track at the preview time if it has stopped
        /// </summary>
        private static void KeepPlayingAudioTrackAtPreview()
        {
            if (AudioEngine.Track == null)
                return;

            if (AudioEngine.Track.HasPlayed && AudioEngine.Track.IsStopped)
                AudioEngine.PlaySelectedTrackAtPreview();
        }
    }
}