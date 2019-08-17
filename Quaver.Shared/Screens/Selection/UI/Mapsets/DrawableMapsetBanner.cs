using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Window;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets
{
    public class DrawableMapsetBanner : Sprite
    {
        /// <summary>
        /// </summary>
        private DrawableMapset Mapset { get; }

        /// <summary>
        /// </summary>
        private RenderTarget2D RenderTarget { get; set; }

        /// <summary>
        /// </summary>
        private Texture2D MapTexture { get; set; }

        /// <summary>
        /// </summary>
        private Texture2D DefaultBanner => UserInterface.DefaultBanner;

        /// <summary>
        /// </summary>
        /// <param name="mapset"></param>
        public DrawableMapsetBanner(DrawableMapset mapset)
        {
            Mapset = mapset;

            Alpha = 0;
            Image = DefaultBanner;
            LoadBackground();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            DisposeTextures();
            base.Destroy();
        }

        /// <summary>
        ///     Loads the map's background/banner under a separate thread
        ///
        /// </summary>
        public void LoadBackground() => ThreadScheduler.Run(() =>
        {
            DisposeTextures();

            // TODO: Banner Support
            var path = MapManager.GetBackgroundPath(Mapset.Item.Maps.First());
            MapTexture = File.Exists(path) ? AssetLoader.LoadTexture2DFromFile(path) : DefaultBanner;

            // Default Banner
            if (MapTexture == DefaultBanner)
            {
                Image = MapTexture;
                FadeIn();
                return;
            }

            // Mask the image and draw it to a RenderTarget
            GameBase.Game.ScheduledRenderTargetDraws.Add(() =>
            {
                var scrollContainer = new ScrollContainer(new ScalableVector2(Width, Height),
                    new ScalableVector2(Width, Height));

                var maskedSprite = new Sprite()
                {
                    Alignment = Alignment.MidCenter,
                    // Small 16:9 resolution size to make backgrounds look a bit better and zoomed out
                    Size = new ScalableVector2(1024, 576),
                    // This y offset usually captures the best part of the image (such as faces or text)
                    Y = 100,
                    Image = MapTexture
                };

                scrollContainer.AddContainedDrawable(maskedSprite);

                var (pixelWidth, pixelHeight) = scrollContainer.AbsoluteSize * WindowManager.ScreenScale;

                RenderTarget = new RenderTarget2D(GameBase.Game.GraphicsDevice, (int) pixelWidth, (int) pixelHeight, false,
                    GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

                GameBase.Game.GraphicsDevice.SetRenderTarget(RenderTarget);
                GameBase.Game.GraphicsDevice.Clear(Color.Transparent);

                scrollContainer.Draw(new GameTime());
                GameBase.Game.SpriteBatch.End();

                GameBase.Game.GraphicsDevice.SetRenderTarget(null);
                scrollContainer?.Destroy();
                maskedSprite?.Destroy();

                Image = RenderTarget;

                FadeIn();
            });
        });

        /// <summary>
        ///     Disposes of the map textures
        /// </summary>
        public void DisposeTextures()
        {
            if (MapTexture != null && MapTexture != DefaultBanner)
                MapTexture?.Dispose();


            RenderTarget?.Dispose();
        }

        /// <summary>
        ///     Fades in the image from 0 alpha
        /// </summary>
        private void FadeIn()
        {
            Alpha = 0;
            ClearAnimations();
            FadeTo(1, Easing.OutQuint, 1000);
        }
    }
}