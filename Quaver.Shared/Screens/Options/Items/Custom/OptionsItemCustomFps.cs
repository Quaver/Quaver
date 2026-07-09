using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Buttons;
using Quaver.Shared.Graphics.Dialogs;
using Quaver.Shared.Graphics.Notifications;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Gameplay.UI.Offset;
using Quaver.Shared.Screens.Selection.UI.Profile;
using Wobble;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    /// <summary>
    ///    The options item for custom FPS.
    /// </summary>
    public class OptionsItemCustomFps : OptionsItem
    {
        /// <summary>
        ///    The button to open the dialog.
        /// </summary>
        private RoundedButton Button { get; }

        /// <summary>
        /// </summary>
        /// <param name="containerWidth"></param>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        public OptionsItemCustomFps(RectangleF containerRect, string name) : base(containerRect, name)
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

            Button.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), "SET FPS", 18, Color.White);

            Button.Clicked += (sender, args) =>
            {
                Focused = true;
                DialogManager.Show(new YesNoTextDialog("Custom FPS", "Enter a custom FPS value.", ConfigManager.CustomFpsLimit.Value.ToString(), "", (s) =>
                {
                    if (!int.TryParse(s, out var fps))
                    {
                        NotificationManager.Show(NotificationLevel.Error, "Please enter a valid FPS value.");
                        return;
                    }
                    ConfigManager.CustomFpsLimit.Value = fps;
                    NotificationManager.Show(NotificationLevel.Success, $"Custom FPS set to {ConfigManager.CustomFpsLimit.Value}.");

                    Focused = false;
                }, () =>
                {
                    Focused = false;
                }));
            };
        }
    }
}