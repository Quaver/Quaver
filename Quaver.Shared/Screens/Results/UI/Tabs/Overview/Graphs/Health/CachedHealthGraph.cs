using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Result.UI;
using Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs.Deviance;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;
using Wobble.Window;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs.Health
{
    public class CachedHealthGraph : CacheableContainer
    {
        /// <summary>
        /// </summary>
        private Map Map { get; }

        /// <summary>
        /// </summary>
        private Bindable<ScoreProcessor> Processor { get; }

        /// <summary>
        /// </summary>
        private HealthGraph HealthGraph { get; set; }

        /// <summary>
        ///     Displays the cached version of <see cref="HealthGraph"/>
        /// </summary>
        public Sprite CachedSprite { get; }

        /// <summary>
        ///     The RenderTarget that draws the bar for <see cref="CachedSprite"/> to use
        /// </summary>
        private RenderTarget2D RenderTarget { get; }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="processor"></param>
        /// <param name="size"></param>
        public CachedHealthGraph(Map map, Bindable<ScoreProcessor> processor, ScalableVector2 size)
        {
            Map = map;
            Processor = processor;
            Size = size;

            CreateHealthGraph();

            CachedSprite = new Sprite
            {
                Size = Size,
                SpriteBatchOptions = new SpriteBatchOptions { BlendState = BlendState.AlphaBlend }
            };

            var (pixelWidth, pixelHeight) = AbsoluteSize * WindowManager.ScreenScale;

            // ReSharper disable twice CompareOfFloatsByEqualityOperator
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

                    HealthGraph.Draw(new GameTime());
                    GameBase.Game.SpriteBatch.End();

                    GameBase.Game.GraphicsDevice.SetRenderTarget(null);
                    CachedSprite.Image = RenderTarget;
                    CachedSprite.Parent = this;
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            });
        }

        /// <summary>
        /// </summary>
        private void CreateHealthGraph() => HealthGraph = new HealthGraph(Map, Processor, Size);
    }
}