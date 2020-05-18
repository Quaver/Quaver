using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Screens.Multi.UI.Status.Name;
using Quaver.Shared.Screens.Selection;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Multi.UI.Status.Selection
{
    public class MultiplayerSelectMapButton : IconButton
    {
        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> Game { get; }

        /// <summary>
        /// </summary>
        public MultiplayerSelectMapButton(Bindable<MultiplayerGame> game) : base(UserInterface.MultiplayerSelectMap)
        {
            Game = game;

            const float scale = 0.85f;

            Size = new ScalableVector2(Image.Width * scale, Image.Height * scale);

            Clicked += (sender, args) =>
            {
                var quaver = GameBase.Game as QuaverGame;

                if (quaver?.CurrentScreen is MultiplayerGameScreen multi)
                {
                    multi.DontLeaveGameUponScreenSwitch = true;
                    multi.Exit(() => new SelectionScreen());
                }
            };
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