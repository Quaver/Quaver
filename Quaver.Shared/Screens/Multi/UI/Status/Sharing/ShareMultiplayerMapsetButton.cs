using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Multi.UI.Status.Sharing
{
    public class ShareMultiplayerMapsetButton : IconButton
    {
        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> Game { get; }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        public ShareMultiplayerMapsetButton(Bindable<MultiplayerGame> game) : base(UserInterface.MultiplayerUploadMapset)
        {
            Game = game;

            var scale = 0.85f;

            Size = new ScalableVector2(Image.Width * scale, Image.Height * scale);

            Clicked += (sender, args) => ShareMapset();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (Game.Value.HostId != OnlineManager.Self?.OnlineUser?.Id || Game.Value.IsMapsetShared)
            {
                IsPerformingFadeAnimations = false;
                Alpha = 0.45f;
                IsClickable = false;
            }
            else
            {
                IsPerformingFadeAnimations = true;
                IsClickable = true;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void ShareMapset()
        {
            if (Game.Value.HostId != OnlineManager.Self?.OnlineUser?.Id)
            {
                NotificationManager.Show(NotificationLevel.Warning,
                    "You must be the host to share unsubmitted mapsets!");
                return;
            }

            if (!OnlineManager.IsDonator)
            {
                NotificationManager.Show(NotificationLevel.Warning, "You must have donator status to be able to share unsubmitted mapsets!");
                return;
            }

            if (MapManager.Selected.Value == null || MapManager.Selected.Value.MapId != -1)
            {
                NotificationManager.Show(NotificationLevel.Warning, "You can only share mapsets that are not submitted online!");
                return;
            }

            if (Game.Value.IsMapsetShared)
            {
                NotificationManager.Show(NotificationLevel.Warning, "This mapset is already shared to everyone in the game!");
                return;
            }

            DialogManager.Show(new ShareMultiplayerMapsetConfirmDialog());
        }
    }
}