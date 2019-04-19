using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Lobby.UI.Dialogs.Joining;
using Quaver.Shared.Screens.Lobby.UI.Dialogs.Password;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Lobby.UI
{
    public class DrawableMultiplayerGameButton : ImageButton
    {
        /// <summary>
        /// </summary>
        private LobbyMatchScrollContainer Container { get; }

        /// <summary>
        /// </summary>
        private bool HoverEffectPlayed { get; set; }

        /// <summary>
        /// </summary>
        private DrawableMultiplayerGame Game { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="game"></param>
        public DrawableMultiplayerGameButton(LobbyMatchScrollContainer container, DrawableMultiplayerGame game) : base(UserInterface.BlankBox)
        {
            Container = container;
            Game = game;
            Tint = Color.Black;
            Size = new ScalableVector2(game.Width, game.HEIGHT * 0.95f);
            Alignment = Alignment.MidLeft;
            UsePreviousSpriteBatchOptions = true;

            Clicked += (o, e) =>
            {
                if (game.Item.HasPassword)
                    DialogManager.Show(new JoinPasswordedGameDialog(game.Item));
                else
                {
                    DialogManager.Show(new JoiningGameDialog(JoiningGameDialogType.Joining));
                    ThreadScheduler.RunAfter(() => OnlineManager.Client.JoinGame(game.Item), 800);
                }
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Alpha = MathHelper.Lerp(Alpha, IsHovered ? 0.20f : 0.75f,
                (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 60, 1));

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        ///     In this case, we only want buttons to be clickable if they're in the bounds of the scroll container.
        /// </summary>
        /// <returns></returns>
        protected override bool IsMouseInClickArea()
        {
            var newRect = RectangleF.Intersect(ScreenRectangle, Container.ScreenRectangle);
            return GraphicsHelper.RectangleContains(newRect, MouseManager.CurrentState.Position);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void OnHover(GameTime gameTime)
        {
            if (HoverEffectPlayed)
                return;

            SkinManager.Skin.SoundHover.CreateChannel().Play();
            HoverEffectPlayed = true;
            base.OnHover(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void OnNotHovered(GameTime gameTime)
        {
            HoverEffectPlayed = false;
            base.OnNotHovered(gameTime);
        }
    }
}