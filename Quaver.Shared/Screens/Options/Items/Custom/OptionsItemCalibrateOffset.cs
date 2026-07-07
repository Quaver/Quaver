using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Buttons;
using Quaver.Shared.Graphics.Notifications;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Gameplay;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemCalibrateOffset : OptionsItem
    {
        /// <summary>
        /// </summary>
        private RoundedButton Button { get; }

        /// <summary>
        /// </summary>
        /// <param name="containerWidth"></param>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        public OptionsItemCalibrateOffset(RectangleF containerRect, string name) : base(containerRect, name)
        {
            const float scale = 0.85f;

            Button = new RoundedButton
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(215 * scale, 36 * scale),
                Tint = ColorHelper.HexToColor("#0FBAE5")
            };

            Button.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), "CALIBRATE", 16, Color.White);

            Button.Clicked += (sender, args) =>
            {
                var game = GameBase.Game as QuaverGame;

                if (game == null)
                    return;

                if (game?.CurrentScreen?.Type != QuaverScreenType.Menu &&
                    game?.CurrentScreen?.Type != QuaverScreenType.Select)
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
                    DialogManager.Dismiss(DialogManager.Dialogs.Last());

                    return new GameplayScreen(qua, "", new List<Score>(), null, false, 0, true);
                });
            };
        }
    }
}