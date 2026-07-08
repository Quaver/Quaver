using System.IO;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Buttons;
using Quaver.Shared.Graphics.Notifications;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Managers;
using Wobble.Platform;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemOpenSkinFolder : OptionsItem
    {
        /// <summary>
        /// </summary>
        private RoundedButton Button { get; }

        /// <summary>
        /// </summary>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        public OptionsItemOpenSkinFolder(RectangleF containerRect, string name) : base(containerRect, name)
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

            Button.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), "OPEN FOLDER", 20, Color.White);

            Button.Clicked += (sender, args) =>
            {
                if (ConfigManager.SkinDirectory == null)
                    return;

                if (string.IsNullOrEmpty(ConfigManager.Skin.Value))
                {
                    NotificationManager.Show(NotificationLevel.Warning, "You currently do not have a skin selected!");
                    return;
                }

                var dir = ConfigManager.UseSteamWorkshopSkin.Value ?
                    $"{ConfigManager.SteamWorkshopDirectory.Value}/{ConfigManager.Skin.Value}"
                    : $"{ConfigManager.SkinDirectory.Value}/{ConfigManager.Skin.Value}";

                if (!Directory.Exists(dir))
                {
                    NotificationManager.Show(NotificationLevel.Warning, "Your skin folder does not exist!");
                    return;
                }

                Utils.NativeUtils.OpenNatively(dir);
            };
        }
    }
}