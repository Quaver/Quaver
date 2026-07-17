using System;
using Microsoft.Xna.Framework;
using Quaver.Server.Client;
using Quaver.Shared.Assets;
using Wobble.Graphics.Buttons;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.Hub.SongRequests.Header
{
    public class ConnectTwitchButton : RoundedButton
    {
        /// <summary>
        ///     Returns if the user's twitch account is connected
        /// </summary>
        private bool IsTwitchConnected => !string.IsNullOrEmpty(OnlineManager.TwitchUsername);

        /// <summary>
        /// </summary>
        public ConnectTwitchButton()
        {
            Size = new ScalableVector2(135, 27);
            Tint = ColorHelper.HexToColor("#9146FF");
            SetIcon(UserInterface.TwitchIconWhite, new Vector2(16, 16));
            SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), "CONNECT", 16, Color.White);
            Clicked += OnClicked;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), IsTwitchConnected ? "UNLINK" : "CONNECT", 16, Color.White);

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClicked(object sender, EventArgs e)
        {
            // User needs to connect their twitch
            if (!IsTwitchConnected)
            {
                BrowserHelper.OpenURL(OnlineClient.CONNECT_TWITCH_URL, true);
                return;
            }

            // User wants to disconnect their twitch
            if (!OnlineManager.Connected)
            {
                NotificationManager.Show(NotificationLevel.Error, "You need to be logged in to unlink your Twitch account!");
                return;
            }

            DialogManager.Show(new UnlinkTwitchConfirmationDialog());
        }
    }
}