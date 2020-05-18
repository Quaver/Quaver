using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Screens.Multi.UI.Status.Name;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Multi.UI.Status.Password
{
    public class MultiplayerEditPasswordButton : IconButton
    {
        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> Game { get; }

        /// <summary>
        /// </summary>
        public MultiplayerEditPasswordButton(Bindable<MultiplayerGame> game) : base(UserInterface.MultiplayerEditPassword)
        {
            Game = game;

            const float scale = 0.85f;

            Size = new ScalableVector2(Image.Width * scale, Image.Height * scale);

            Clicked += (sender, args) => DialogManager.Show(new ChangeGamePasswordDialog(Game.Value));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (Game.Value.HostId != OnlineManager.Self?.OnlineUser?.Id)
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
    }
}