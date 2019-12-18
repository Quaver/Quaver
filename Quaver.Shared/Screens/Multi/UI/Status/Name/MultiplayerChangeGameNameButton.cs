using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Multi.UI.Status.Name
{
    public class MultiplayerChangeGameNameButton : IconButton
    {
        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> Game { get; }

        /// <summary>
        /// </summary>
        public MultiplayerChangeGameNameButton(Bindable<MultiplayerGame> game) : base(UserInterface.MultiplayerChangeName)
        {
            Game = game;

            const float scale = 0.85f;

            Size = new ScalableVector2(Image.Width * scale, Image.Height * scale);

            Clicked += (sender, args) => DialogManager.Show(new ChangeGameNameDialog(Game.Value));
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