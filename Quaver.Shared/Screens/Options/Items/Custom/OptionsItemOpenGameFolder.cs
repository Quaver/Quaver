using System.IO;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Wobble.Graphics.Buttons;
using Quaver.Shared.Graphics.Notifications;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;
using Wobble.Graphics;
using Wobble.Managers;
using Wobble.Platform;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemOpenGameFolder : OptionsItem
    {
        /// <summary>
        /// </summary>
        private RoundedButton Button { get; }

        public OptionsItemOpenGameFolder(RectangleF containerRect, string name) : base(containerRect, name)
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

            Button.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), "OPEN FOLDER", 18, Color.White);

            Button.Clicked += (sender, args) =>
            {
                var dir = ConfigManager.GameDirectory.Value;

                if (!Directory.Exists(dir))
                {
                    NotificationManager.Show(NotificationLevel.Warning, "That folder does not exist!");
                    return;
                }

                Utils.NativeUtils.OpenNatively(dir);
            };
        }
    }
}