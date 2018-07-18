using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Sprites;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.Resources;
using Quaver.Shaders;
using Quaver.States.Gameplay.GameModes.Keys.Playfield;

namespace Quaver.States.Gameplay.UI.Components.Health
{
    internal class HealthBar : IGameStateComponent
    {
        /// <summary>
        ///     The type of health bar this is. Whether horizontal or vertical.
        /// </summary>
        internal HealthBarType Type { get; }

        /// <summary>
        ///     The bar displayed in the background. This one doesn't move.
        /// </summary>
        protected AnimatableSprite BackgroundBar { get; set; }

        /// <summary>
        ///     The bar displayed in the foreground. This one dictates the amount
        ///     of health the user currently has.
        /// </summary>
        protected AnimatableSprite ForegroundBar { get; set; }

        /// <summary>
        ///     Reference to the current score processor.
        /// </summary>
        private ScoreProcessor Processor { get; }

        /// <summary>
        ///     Shader to make health bar foreground partially transparent.
        /// </summary>
        private Effect SemiTransparent { get; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="processor"></param>
        internal HealthBar(HealthBarType type, ScoreProcessor processor)
        {
            Type = type;
            Processor = processor;
            SemiTransparent = ResourceHelper.LoadShader(ShaderStore.semi_transparent);
        }

        /// <summary>
        ///     Initialize Sprites
        /// </summary>
        /// <param name="state"></param>
        public virtual void Initialize(IGameState state)
        {
            // Create the background bar sprite.
            BackgroundBar = new AnimatableSprite(GameBase.Skin.HealthBarBackground);
            BackgroundBar.Size = new UDim2D(BackgroundBar.Frames.First().Width, BackgroundBar.Frames.First().Height);

            // Start animation
            BackgroundBar.StartLoop(LoopDirection.Forward, 60);

            // Create the foreground bar (the one that'll serve as the gauge progress).
            ForegroundBar = new AnimatableSprite(GameBase.Skin.HealthBarForeground);
            ForegroundBar.Size = new UDim2D(ForegroundBar.Frames.First().Width, ForegroundBar.Frames.First().Height);

            // Start animation.
            ForegroundBar.StartLoop(LoopDirection.Forward, 60);

            switch (Type)
            {
                case HealthBarType.Horizontal:
                    BackgroundBar.Alignment = Alignment.TopLeft;
                    ForegroundBar.Alignment = Alignment.TopLeft;

                    BackgroundBar.PosX = 8;
                    ForegroundBar.PosX = 8;

                    SemiTransparent.Parameters["p_position"].SetValue(new Vector2(ForegroundBar.SizeX, 0f));
                    break;
                case HealthBarType.Vertical:
                    BackgroundBar.Alignment = Alignment.BotLeft;
                    ForegroundBar.Alignment = Alignment.BotLeft;

                    BackgroundBar.PosY = -8;
                    ForegroundBar.PosY = -8;

                    SemiTransparent.Parameters["p_position"].SetValue(new Vector2(0, 0));
                    break;
                default:
                    throw new NotImplementedException();
            }

            // Set default shader params.
            SemiTransparent.Parameters["p_rectangle"].SetValue(new Vector2(ForegroundBar.SizeX, ForegroundBar.SizeY));
            SemiTransparent.Parameters["p_dimensions"].SetValue(new Vector2(ForegroundBar.SizeX, ForegroundBar.SizeY));
            SemiTransparent.Parameters["p_alpha"].SetValue(0f);
        }

        /// <summary>
        ///     Destroy Sprites
        /// </summary>
        public void UnloadContent()
        {
            BackgroundBar.Destroy();
            ForegroundBar.Destroy();
        }

        /// <summary>
        ///     Update Sprites
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            BackgroundBar.Update(dt);
            ForegroundBar.Update(dt);
            SetHealthBarProgress(dt);
        }

        /// <summary>
        ///     Draw Sprites
        /// </summary>
        public void Draw()
        {
            // Draw Background Bar.
            GameBase.SpriteBatch.Begin();
            BackgroundBar.Draw();
            GameBase.SpriteBatch.End();

            // Use new spritebatch for ST Shader.
            GameBase.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied,
                                   SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, SemiTransparent);
            ForegroundBar.Draw();
            GameBase.SpriteBatch.End();
        }

        /// <summary>
        ///        Moves the shader rect's position/size back and forth based on the user's health.
        ///        This creates that health gauge effect.
        /// </summary>
        /// <param name="dt"></param>
        private void SetHealthBarProgress(double dt)
        {
            switch (Type)
            {
                // We handle horizontal bar types with the position in this case, so we can alpha mask it
                // with a full bar from right to left.
                case HealthBarType.Horizontal:
                    // Target position based on the user's current health.
                    var targetPosX = Processor.Health / 100 * ForegroundBar.SizeX;

                    var newPosX = MathHelper.Lerp(SemiTransparent.Parameters["p_position"].GetValueVector2().X, targetPosX, (float)Math.Min(dt / 30, 1));
                    SemiTransparent.Parameters["p_position"].SetValue(new Vector2(newPosX, 0f));
                    break;
                // We handle vertical bar types with the size of the bar instead of position, since we're
                // going from top to bottom.
                case HealthBarType.Vertical:
                    // Get new size of the bar based on the user's current health.
                    var targetSizeY = ForegroundBar.SizeY - Processor.Health / 100 * ForegroundBar.SizeY;

                    var newSizeY = MathHelper.Lerp(SemiTransparent.Parameters["p_rectangle"].GetValueVector2().Y, targetSizeY, (float)Math.Min(dt / 30, 1));
                    SemiTransparent.Parameters["p_rectangle"].SetValue(new Vector2(ForegroundBar.SizeX, newSizeY));
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }
        }
    }
}