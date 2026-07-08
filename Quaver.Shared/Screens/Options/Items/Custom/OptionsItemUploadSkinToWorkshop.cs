using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Buttons;
using Quaver.Shared.Graphics.Notifications;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemUploadSkinToWorkshop : OptionsItem
    {
        /// <summary>
        /// </summary>
        private RoundedButton Button { get; }

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

            Button = new RoundedButton
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(215 * scale, 36 * scale),
                Tint = ColorHelper.HexToColor("#27B06E")
            };

            Button.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), "UPLOAD", 20, Color.White);

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