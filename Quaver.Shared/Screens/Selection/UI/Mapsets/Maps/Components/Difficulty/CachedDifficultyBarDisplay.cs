using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Wobble;
using Wobble.Graphics.Sprites;
using Wobble.Logging;
using Wobble.Window;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets.Maps.Components.Difficulty
{
    public class CachedDifficultyBarDisplay : CacheableContainer
    {
        /// <summary>
        /// </summary>
        private DifficultyBarDisplay DifficultyBar { get; }

        /// <summary>
        /// </summary>
        private Sprite Background { get; }

        /// <summary>
        ///     Displays the cached version of <see cref="DifficultyBar"/>
        /// </summary>
        private Sprite CachedSprite { get; }

        /// <summary>
        ///     The RenderTarget that draws the bar for <see cref="CachedSprite"/> to use
        /// </summary>
        private RenderTarget2D RenderTarget { get; }

        /// <summary>
        /// </summary>
        /// <param name="bar"></param>
        public CachedDifficultyBarDisplay(DifficultyBarDisplay bar)
        {
            DifficultyBar = bar;

            Size = DifficultyBar.Size;

            Background = new Sprite()
            {
                Parent = this,
                Size = DifficultyBar.Size,
                Image = UserInterface.DifficultyBarBackground
            };

            CachedSprite = new Sprite
            {
                Parent = this,
                Size = DifficultyBar.Size
            };

            var (pixelWidth, pixelHeight) = AbsoluteSize * WindowManager.ScreenScale;

            RenderTarget = new RenderTarget2D(GameBase.Game.GraphicsDevice, (int) pixelWidth, (int) pixelHeight, false,
                GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

            NeedsToCache = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            DifficultyBar.Update(gameTime);

            if (DifficultyBar.Container.Animations.Count != 0)
                NeedsToCache = true;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            RenderTarget.Dispose();
            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Cache()
        {
            GameBase.Game.ScheduledRenderTargetDraws.Add(() =>
            {
                GameBase.Game.GraphicsDevice.SetRenderTarget(RenderTarget);
                GameBase.Game.GraphicsDevice.Clear(Color.Transparent);

                DifficultyBar.Draw(new GameTime());
                GameBase.Game.SpriteBatch.End();

                GameBase.Game.GraphicsDevice.SetRenderTarget(null);
                CachedSprite.Image = RenderTarget;
            });
        }
    }
}