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
        internal Container BackgroundContainer { get; private set; }

        /// <summary>
        ///     The scroll container, which contains HitObjects + snap lines.
        /// </summary>
        private EditorScrollContainerKeys ScrollContainer { get; set; }

        /// <summary>
        ///     The size of each column.
        /// </summary>
        internal float ColumnSize { get; } = 75;

        /// <summary>
        ///     The scroll speed of the objects in the editor.
        /// </summary>
        private float _scrollSpeed = 15;
        internal float ScrollSpeed
        {
            get => _scrollSpeed / (20 * GameBase.AudioEngine.PlaybackRate);
            set => _scrollSpeed = value;
        }

         /// <summary>
        ///     The width of the playfield.
        /// </summary>
        internal float Width => ColumnSize * Screen.Map.GetKeyCount();

#region STAGE_SPRITES    
        /// <summary>
        ///     The background sprite of the stage
        /// </summary>
        private Sprite StageBackground { get; set; }

        /// <summary>
        ///     The width of the StageLeft/StageRight
        /// </summary>
        private int StageBorderWidth { get; } = 1;

        /// <summary>
        ///     The left side of the stage.
        /// </summary>
        private Sprite StageLeft { get; set; }

        /// <summary>
        ///     The right side of the stage.
        /// </summary>
        private Sprite StageRight { get; set; }

        /// <summary>
        ///     The y position of where the hitposition is located.
        /// </summary>
        internal float HitPositionY => StageBackground.SizeY - 200;

        /// <summary>
        ///     Sprite that displays where the hit position is.
        /// </summary>
        private Sprite StageHitPosition { get; set; }
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
            
            ScrollContainer = new EditorScrollContainerKeys(this);
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
                Size = new UDim2D(StageBorderWidth, StageBackground.SizeY),
                Alpha = 0.5f
            };
            
            StageRight = new Sprite()
            {
                Parent = BackgroundContainer,
                Tint = Color.Gray,
                Alignment = Alignment.TopRight,
                Size = new UDim2D(StageBorderWidth, StageBackground.SizeY),
                Alpha = 0.5f,
                PosX = StageBorderWidth
            };

            StageHitPosition = new Sprite()
            {
                Parent = BackgroundContainer,
                Tint = Color.Green,
                Alignment = Alignment.TopLeft,
                Size = new UDim2D(Width - StageBorderWidth, 5),
                PosY = HitPositionY
            };
        }
    }
}