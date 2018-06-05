using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Sprites;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.Resources;

namespace Quaver.States.Gameplay.UI.Components.Health
{
    internal class HealthBar : IGameStateComponent
    {
        /// <summary>
        ///     The type of health bar this is. Whether horizontal or vertical.
        /// </summary>
        internal HealthBarType Type { get; }

        /// <summary>
        ///     Shader to make health bar foreground partially transparent.
        /// </summary>
        internal Effect SemiTransparent { get; }

        /// <summary>
        ///     The bar displayed in the foreground. This one dictates the amount
        ///     of health the user currentl has.
        /// </summary>
        private Sprite ForegroundBar { get; set; }

        /// <summary>
        ///     Reference to the current score processor.
        /// </summary>
        private ScoreProcessor Processor { get; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="procesor"></param>
        internal HealthBar(HealthBarType type, ScoreProcessor procesor)
        {
            Type = type;
            Processor = procesor;
            SemiTransparent = ResourceHelper.LoadShader(QuaverResources.semi_transparent);
        }

        /// <summary>
        ///     Initialize Sprites
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(IGameState state)
        {
            // Create the foreground bar (the one that'll serve as the gauge progress).
            ForegroundBar = new Sprite
            {
                Image = GameBase.QuaverUserInterface.BlankBox,         
                Alignment = Alignment.TopLeft,
            };
            
            // Set initial position of the foreground health bar.
            switch (Type)
            {
                case HealthBarType.Horizontal:
                    ForegroundBar.Alignment = Alignment.TopLeft;
                    ForegroundBar.Size = new UDim2D(GameBase.WindowRectangle.Width, 20);
                    ForegroundBar.PosY = 40;
                    
                    SemiTransparent.Parameters["p_position"].SetValue(new Vector2(ForegroundBar.SizeX, 0f));
                    break;
                case HealthBarType.Vertical:
                    ForegroundBar.Alignment = Alignment.BotLeft;
                    ForegroundBar.Size = new UDim2D(20, GameBase.WindowRectangle.Height);
                    ForegroundBar.PosX = 50;
                    
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
            ForegroundBar.Destroy();
        }

        /// <summary>
        ///     Update Sprites
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            ForegroundBar.Update(dt);
            SetHealthBarProgress(dt);
        }

        /// <summary>
        ///     Draw Sprites
        /// </summary>
        public void Draw()
        {        
            // Use new spritebatch for ST Shader.
            GameBase.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied,
                                            SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, 
                                            SemiTransparent);
            
            ForegroundBar.Draw();
            
            GameBase.SpriteBatch.End();
        }

        /// <summary>
        ///        Moves the shader rect's position back and forth based on the user's health.
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
                    
                    // Get the new lerped y pos.
                    var newPosX = MathHelper.Lerp(SemiTransparent.Parameters["p_position"].GetValueVector2().X, targetPosX, (float)Math.Min(dt / 30, 1));
                    
                    // Set new pos
                    SemiTransparent.Parameters["p_position"].SetValue(new Vector2(newPosX, 0f));
                    break;
                // We handle vertical bar types with the size of the bar instead of position, since we're 
                // going from top to bottom.
                case HealthBarType.Vertical:
                    // Get new size of the bar based on the user's current health.
                    var targetSizeY = ForegroundBar.SizeY - Processor.Health / 100 * ForegroundBar.SizeY;
                    
                    // Get lerped size of the bar
                    var newSizeY = MathHelper.Lerp(SemiTransparent.Parameters["p_rectangle"].GetValueVector2().Y, targetSizeY, (float)Math.Min(dt / 30, 1));
                    
                    // Set param.
                    SemiTransparent.Parameters["p_rectangle"].SetValue(new Vector2(ForegroundBar.SizeX, newSizeY));
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }
        }
    }
}