using System;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Sprites;
using Quaver.Main;

namespace Quaver.States.Edit.UI.Modes.Keys.Playfield
{
    internal class EditorPlayfieldKeys : IEditorPlayfield
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorScreen Screen { get; }
         
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public GameMode Mode { get; }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public Container Container { get; private set; }

        /// <summary>
        ///     The background of the playfield.
        /// </summary>
        private Container BackgroundContainer { get; set; }

        /// <summary>
        ///     The foreground of the playfield.
        /// </summary>
        private Container ForegroundContainer { get; set; }

        /// <summary>
        ///     The width of the playfield.
        /// </summary>
        private float Width => 75 * Screen.Map.GetKeyCount();

#region STAGE_SPRITES    
        /// <summary>
        ///     The background sprite of the stage
        /// </summary>
        private Sprite StageBackground { get; set; }

        /// <summary>
        ///     The left side of the stage.
        /// </summary>
        private Sprite StageLeft { get; set; }

        /// <summary>
        ///     The right side of the stage.
        /// </summary>
        private Sprite StageRight { get; set; }

#endregion


        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="mode"></param>
        internal EditorPlayfieldKeys(EditorScreen screen, GameMode mode)
        {
            Screen = screen;
            Mode = mode;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(IGameState state)
        {
            // Create parent container.
            Container = new Container();
            
            // Create container for background elements
            BackgroundContainer = new Container
            {
                Parent = Container,
                Size = new UDim2D(Width, GameBase.WindowRectangle.Height),
                Alignment = Alignment.TopCenter
            };
            
            // Create container for foreground elements.
            ForegroundContainer = new Container
            {
                Parent = Container,
                Size = new UDim2D(Width, GameBase.WindowRectangle.Height),
                Alignment = Alignment.TopCenter
            };
            
            CreateStage();
        }

        /// <summary>
        /// 
        /// </summary>
        public void UnloadContent()
        {
            Container.Destroy();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            Container.Update(dt);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Draw()
        {
            GameBase.SpriteBatch.Begin();
            Container.Draw();
            GameBase.SpriteBatch.End();
        }

        /// <summary>
        ///     Creates a mask for the background of where the editor will live.
        /// </summary>
        private void CreateStage()
        {
            var columnRatio = Width / BackgroundContainer.SizeY;
            var bgMaskSize = Math.Max(GameBase.WindowRectangle.Height * columnRatio, GameBase.WindowRectangle.Height);
            
            StageBackground = new Sprite()
            {
                Image = GameBase.QuaverUserInterface.BlankBox,
                Tint = Color.Black,
                Alpha = 0.5f,
                Size = new UDim2D(Width, bgMaskSize),
                Alignment = Alignment.MidCenter,
                Parent = BackgroundContainer
            };

            StageLeft = new Sprite()
            {
                Parent = BackgroundContainer,
                Tint = Color.Gray,
                Alignment = Alignment.TopLeft,
                Size = new UDim2D(1, StageBackground.SizeY),
                Alpha = 0.5f
            };
            
            StageRight = new Sprite()
            {
                Parent = BackgroundContainer,
                Tint = Color.Gray,
                Alignment = Alignment.TopRight,
                Size = new UDim2D(1, StageBackground.SizeY),
                Alpha = 0.5f,
                PosX = 1
            };
        }
    }
}