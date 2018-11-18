using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using osu.Shared;
using Quaver.Assets;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Graphics;
using Quaver.Graphics.Notifications;
using Quaver.Scheduling;
using Quaver.Screens.Loading;
using Quaver.Screens.Menu.UI.Buttons;
using Quaver.Screens.Menu.UI.Navigation.User;
using Quaver.Screens.Options;
using Quaver.Screens.Select.UI.Mods;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Window;

namespace Quaver.Screens.SongSelect.UI.Toolbox
{
    public class ToolboxContainer : ScrollContainer
    {
        /// <summary>
        ///     The button to activate the mods dialog
        /// </summary>
        private BorderedTextButton Mods { get; set; }

        /// <summary>
        ///     The button to edit the map.
        /// </summary>
        private BorderedTextButton Edit { get; set; }

        /// <summary>
        ///     The button to export the mapset.
        /// </summary>
        private BorderedTextButton ExportMapset { get; set; }

        /// <summary>
        ///     The button to play the map
        /// </summary>
        private BorderedTextButton Play { get; set; }

        /// <summary>
        ///     The button to go to the options menu
        /// </summary>
        private BorderedTextButton Options { get; set; }

        /// <summary>
        ///     The time the user last exported a map
        /// </summary>
        private long LastExportTime { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public ToolboxContainer()
            : base(new ScalableVector2(WindowManager.Width - 40, 56), new ScalableVector2(WindowManager.Width - 40, 56))
        {
            Alpha = 0f;

            CreateModsButton();
            CreateEditButton();
            CreateExportMapsetButton();
            CreatePlayButton();
            CreateOptionsButton();
        }

        /// <summary>
        ///
        /// </summary>
        private void CreateModsButton()
        {
            Mods = new BorderedTextButton("Mods", Color.DarkGray)
            {
                Parent = this,
                Text = { Font = BitmapFonts.Exo2Bold},
                Tint = Color.Black,
                Alpha = 0.75f,
                X = 30
            };

            Mods.Height -= 1;
            Mods.Y = -Mods.Height;
            Mods.Width -= 1;
            Mods.MoveToY((int) (Height / 2 - Mods.Height / 2f), Easing.OutQuint, 900);

            Mods.Clicked += (o, e) =>
            {
                DialogManager.Show(new ModsDialog());
            };

            AddContainedDrawable(Mods);
        }

        /// <summary>
        ///    Creates the button to edit the map
        /// </summary>
        private void CreateEditButton()
        {
            Edit = new BorderedTextButton("Edit Map", Color.DarkGray)
            {
                Parent = this,
                Text = {Font = BitmapFonts.Exo2Bold},
                Tint = Color.Black,
                Alpha = 0.75f,
                X = Mods.X + Mods.Width + 15
            };

            Edit.Height -= 1;
            Edit.Width -= 1;
            Edit.Y = -Edit.Height;
            Edit.MoveToY((int) (Height / 2 - Edit.Height / 2f), Easing.OutQuint, 900);

            Edit.Clicked += (o, e) =>
            {
                NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet. Check back soon!");
            };

            AddContainedDrawable(Edit);
        }

        /// <summary>
        ///     Creates the button to export the mapset
        /// </summary>
        private void CreateExportMapsetButton()
        {
            ExportMapset = new BorderedTextButton("Export Mapset", Color.DarkGray)
            {
                Parent = this,
                Text = {Font = BitmapFonts.Exo2Bold},
                Tint = Color.Black,
                Alpha = 0.75f,
                X = Edit.X + Edit.Width + 15
            };

            ExportMapset.Height -= 1;
            ExportMapset.Width -= 1;
            ExportMapset.Y = -ExportMapset.Height;
            ExportMapset.MoveToY((int) (Height / 2 - ExportMapset.Height / 2f), Easing.OutQuint, 900);

            ExportMapset.Clicked += (o, e) =>
            {
                if (Math.Abs(GameBase.Game.TimeRunning - LastExportTime) < 2000)
                {
                    NotificationManager.Show(NotificationLevel.Error, "Slow down! You can only export a set every 2 seconds.");
                    return;
                }

                LastExportTime = GameBase.Game.TimeRunning;

                ThreadScheduler.Run(() =>
                {
                    NotificationManager.Show(NotificationLevel.Info, "Exporting mapset to file...");
                    MapManager.Selected.Value.Mapset.ExportToZip();
                    NotificationManager.Show(NotificationLevel.Success, "Successfully exported mapset!");
                });
            };

            AddContainedDrawable(ExportMapset);
        }

        /// <summary>
        ///     Creates the button to play the map.
        /// </summary>
        private void CreatePlayButton()
        {
            Play = new BorderedTextButton("Play", Color.DarkGray)
            {
                Parent = this,
                Text = { Font = BitmapFonts.Exo2Bold},
                Tint = Color.Black,
                Alpha = 0.75f,
                Alignment = Alignment.TopRight,
                X = -30
            };

            Play.Height -= 1;
            Play.Y = -Play.Height;
            Play.Width -= 1;
            Play.MoveToY((int) (Height / 2 - Play.Height / 2f), Easing.OutQuint, 900);

            Play.Clicked += (o, e) =>
            {
                var game = GameBase.Game as QuaverGame;
                var screen = game?.CurrentScreen as SongSelectScreen;
                screen?.ExitToGameplay();
            };

            AddContainedDrawable(Play);
        }

        /// <summary>
        ///     Creates the button to go to the options menu
        /// </summary>
        private void CreateOptionsButton()
        {
            Options = new BorderedTextButton("Options", Color.DarkGray)
            {
                Parent = this,
                Text = { Font = BitmapFonts.Exo2Bold},
                Tint = Color.Black,
                Alpha = 0.75f,
                Alignment = Alignment.TopRight,
                X = -30
            };

            Options.Height -= 1;
            Options.Y = -Options.Height;
            Options.Width -= 1;
            Options.X = Play.X - Play.Width - 15;
            Options.MoveToY((int) (Height / 2 - Options.Height / 2f), Easing.OutQuint, 900);

            Options.Clicked += (o, e) =>
            {
                DialogManager.Show(new OptionsDialog(0.75f));
            };

            AddContainedDrawable(Options);
        }

        /// <summary>
        ///     Performs an exit animation for the buttons
        /// </summary>
        public void Exit()
        {
            Mods.MoveToY((int)Mods.Height * 2, Easing.OutQuint, 300);
            Edit.MoveToY((int)Edit.Height * 2, Easing.OutQuint, 300);
            ExportMapset.MoveToY((int)ExportMapset.Height * 2, Easing.OutQuint, 300);
            Play.MoveToY((int)Play.Height * 2, Easing.OutQuint, 300);
            Options.MoveToY((int)Options.Height * 2, Easing.OutQuint, 300);
        }
    }
}