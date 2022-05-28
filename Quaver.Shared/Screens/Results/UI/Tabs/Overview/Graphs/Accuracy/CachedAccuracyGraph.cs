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
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;
using Wobble.Window;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs.Accuracy
{
    public class CachedAccuracyGraph : CacheableContainer
    {
        /// <summary>
        /// </summary>
        private Map Map { get; }

        /// <summary>
        /// </summary>
        private Bindable<ScoreProcessor> Processor { get; }

        /// <summary>
        /// </summary>
        private AccuracyGraph AccuracyGraph { get; set; }

        /// <summary>
        ///     Displays the cached version of <see cref="AccuracyGraph"/>
        /// </summary>
        public Sprite CachedSprite { get; }

        /// <summary>
        ///     The RenderTarget that draws the bar for <see cref="CachedSprite"/> to use
        /// </summary>
        private RenderTarget2D RenderTarget { get; }

        /// <summary>
        /// </summary>
        private ImageButton ToolTipArea { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="processor"></param>
        /// <param name="size"></param>
        public CachedAccuracyGraph(Map map, Bindable<ScoreProcessor> processor, ScalableVector2 size)
        {
            Map = map;
            Processor = processor;
            Size = size;

            CreateAccuracyGraph();

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
            CreateTooltip();
        }

        public override void Update(GameTime gameTime)
        {
            ToolTipArea.IsClickable = ConfigManager.ResultGraph?.Value == ResultGraphs.Accuracy;
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            RenderTarget?.Dispose();
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

                    AccuracyGraph.Draw(new GameTime());
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
        private void CreateAccuracyGraph() => AccuracyGraph = new AccuracyGraph(Map, Processor, Size);

        /// <summary>
        /// </summary>
        private void CreateTooltip()
        {
            ToolTipArea = new ImageButton(UserInterface.BlankBox)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = Size,
                Alpha = 0f,
            };

            const string tooltipText = "This displays the course of accuracy throughout the score.\n\n" +
                                       "If the map was not completed, then it will additionally\n" +
                                       "show the accuracy if all subsequent hits had been\n" +
                                       "Marvelous instead.";

            var game = GameBase.Game as QuaverGame;
            ToolTipArea.Hovered += (sender, args) => game?.CurrentScreen?.ActivateTooltip(new Tooltip(tooltipText, ColorHelper.HexToColor("#5dc7f9")));
            ToolTipArea.LeftHover += (sender, args) => game?.CurrentScreen?.DeactivateTooltip();
        }
    }
}