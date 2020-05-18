using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Server.Client;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Graphics.Overlays.Hub.SongRequests.Header
{
    public class ConnectTwitchButton : IconButton
    {
        /// <summary>
        ///     Returns if the user's twitch account is connected
        /// </summary>
        private bool IsTwitchConnected => !string.IsNullOrEmpty(OnlineManager.TwitchUsername);

        /// <summary>
        /// </summary>
        public ConnectTwitchButton() : base(UserInterface.BlankBox)
        {
            Size = new ScalableVector2(135, 27);
            Clicked += OnClicked;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Image = IsTwitchConnected ? UserInterface.UnlinkTwitch : UserInterface.ConnectTwitch;

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