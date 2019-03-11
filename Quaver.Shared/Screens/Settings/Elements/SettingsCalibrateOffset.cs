using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Menu.UI.Navigation.User;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Settings.Elements
{
    public class SettingsCalibrateOffset : SettingsItem
    {
        /// <summary>
        /// </summary>
        private BorderedTextButton CalibrateOffsetButton { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        public SettingsCalibrateOffset(SettingsDialog dialog, string name) : base(dialog, name) => CreateExportButton();

        /// <summary>
        /// </summary>
        private void CreateExportButton()
        {
            CalibrateOffsetButton = new BorderedTextButton("Calibrate", Colors.MainAccent)
            {
                Parent = this,
                X = -50,
                Alignment = Alignment.MidRight,
                Height = 30,
                Width = 225,
                Text =
                {
                    Font = Fonts.SourceSansProSemiBold,
                    FontSize = 12
                }
            };

            CalibrateOffsetButton.Clicked += (o, e) =>
            {
                var game = (QuaverGame) GameBase.Game;

                if (game.CurrentScreen.Type == QuaverScreenType.Editor)
                {
                    NotificationManager.Show(NotificationLevel.Warning, "Finish what you're doing before calibrating a new offset");
                    return;
                }

                var path = $"Quaver.Resources/Maps/Offset/offset.qua";

                var qua = Qua.Parse(GameBase.Game.Resources.Get(path));

                if (AudioEngine.Track != null && !AudioEngine.Track.IsDisposed && AudioEngine.Track.IsPlaying)
                    AudioEngine.Track.Pause();

                game.CurrentScreen?.Exit(() =>
                {
                    MapManager.Selected.Value = Map.FromQua(qua, path, true);
                    MapManager.Selected.Value.Qua = qua;

                    // Make the user not allow to fail.
                    ModManager.RemoveAllMods();
                    ModManager.AddMod(ModIdentifier.NoFail);

                    // Load the background (usually the default one)
                    BackgroundHelper.Load(MapManager.Selected.Value);
                    DialogManager.Dismiss(Dialog);

                    return new GameplayScreen(qua, "", new List<Score>(), null, false, 0, true);
                });
            };
        }
    }
}