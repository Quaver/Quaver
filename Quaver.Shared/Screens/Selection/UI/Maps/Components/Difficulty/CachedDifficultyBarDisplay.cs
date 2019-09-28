using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Wobble;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Logging;
using Wobble.Window;

namespace Quaver.Shared.Screens.Selection.UI.Maps.Components.Difficulty
{
    public class CachedDifficultyBarDisplay : CacheableContainer, IDrawableMapComponent
    {
        /// <summary>
        /// </summary>
        public DifficultyBarDisplay DifficultyBar { get; }

        /// <summary>
        /// </summary>
        public Sprite Background { get; }

        /// <summary>
        ///     Displays the cached version of <see cref="DifficultyBar"/>
        /// </summary>
        public Sprite CachedSprite { get; }

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
                Image = UserInterface.DifficultyBarBackground,
                SetChildrenVisibility = true,
                SetChildrenAlpha = true,
                Alpha = 0,
                Visible = false
            };

            CachedSprite = new Sprite
            {
                Parent = Background,
                Size = DifficultyBar.Size,
                Alpha = 0,
                Visible = false,
                UsePreviousSpriteBatchOptions = true
            };

            var (pixelWidth, pixelHeight) = AbsoluteSize * WindowManager.ScreenScale;

            if (pixelWidth == 0)
                pixelWidth = 1;

            if (pixelHeight == 0)
                pixelHeight = 1;

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
            if (Visible)
            {
                DifficultyBar.Update(gameTime);

                if (DifficultyBar.Container.Animations.Count != 0)
                    NeedsToCache = true;
            }

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
                try
                {
                    if (RenderTarget.IsDisposed)
                        return;

                    GameBase.Game.GraphicsDevice.SetRenderTarget(RenderTarget);
                    GameBase.Game.GraphicsDevice.Clear(Color.Transparent);

                    DifficultyBar.Draw(new GameTime());
                    GameBase.Game.SpriteBatch.End();

                    GameBase.Game.GraphicsDevice.SetRenderTarget(null);
                    CachedSprite.Image = RenderTarget;
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            });
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Open()
        {
            Background.ClearAnimations();
            Background.Wait(200);
            Background.FadeTo(1, Easing.Linear, 250);
            Visible = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Close()
        {
            Background.ClearAnimations();
            Background.Alpha = 0;
            Background.Visible = false;
        }
    }
}