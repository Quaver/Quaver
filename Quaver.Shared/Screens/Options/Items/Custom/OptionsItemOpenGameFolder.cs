using System.IO;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Platform;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemOpenGameFolder : OptionsItem
    {
        /// <summary>
        /// </summary>
        private IconButton Button { get; }

        public OptionsItemOpenGameFolder(RectangleF containerRect, string name) : base(containerRect, name)
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