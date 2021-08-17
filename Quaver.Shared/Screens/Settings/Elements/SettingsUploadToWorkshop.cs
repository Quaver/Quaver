using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Menu.UI.Navigation.User;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Settings.Elements
{
    public class SettingUploadToWorkshop : SettingsItem
    {
        /// <summary>
        /// </summary>
        private BorderedTextButton UploadButton { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        public SettingUploadToWorkshop(SettingsDialog dialog, string name) : base(dialog, name) => CreateUploadButton();

        /// <summary>
        /// </summary>
        private void CreateUploadButton()
        {
            UploadButton = new BorderedTextButton("Upload", Colors.MainAccent)
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

            UploadButton.Clicked += (o, e) =>
            {
                if (string.IsNullOrEmpty(ConfigManager.Skin.Value))
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