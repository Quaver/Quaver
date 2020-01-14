using System.IO;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Platform;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemOpenSkinFolder : OptionsItem
    {
        /// <summary>
        /// </summary>
        private IconButton Button { get; }

        /// <summary>
        /// </summary>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        public OptionsItemOpenSkinFolder(RectangleF containerRect, string name) : base(containerRect, name)
        {
            const float scale = 0.85f;

            Button = new IconButton(UserInterface.OptionsOpenSkinFolderButton)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(215 * scale, 36 * scale),
                UsePreviousSpriteBatchOptions = true
            };

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