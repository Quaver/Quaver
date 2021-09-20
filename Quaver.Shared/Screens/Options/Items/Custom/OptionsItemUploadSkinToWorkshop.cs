using System.Collections.Generic;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Skinning;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemUploadSkinToWorkshop : OptionsItem
    {
        /// <summary>
        /// </summary>
        private IconButton Button { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        public OptionsItemUploadSkinToWorkshop(RectangleF containerRect, string name) : base(containerRect, name)
        {
            Tags = new List<string>()
            {
                "steam"
            };

            const float scale = 0.85f;

            Button = new IconButton(UserInterface.OptionsUploadSkinButton)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(215 * scale, 36 * scale),
                UsePreviousSpriteBatchOptions = true
            };

            Button.Clicked += (sender, args) =>
            {
                if (string.IsNullOrEmpty(ConfigManager.Skin.Value) || ConfigManager.Skin.Value == "Default Skin")
                {
                    NotificationManager.Show(NotificationLevel.Warning, "You currently do not have a selected custom skin!");
                    return;
                }

                var skin = new SteamWorkshopItem(ConfigManager.Skin.Value, SkinManager.Skin.Dir.Replace("\\", "/"));

                if (skin.HasUploaded)
                    return;

                DialogManager.Show(new UploadWorkshopSkinDialog(skin));
            };
        }
    }
}