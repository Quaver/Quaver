using System;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.GameState;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Main;

namespace Quaver.States.Gameplay.Keys.Playfield
{
    internal class KeysPlayfield : IGameplayPlayfield
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public QuaverContainer Container { get; set; }

        /// <summary>
        ///     Reference to the map.
        /// </summary>
        private Qua Map { get; }

        /// <summary>
        ///     The background of this container.
        /// </summary>
        private QuaverContainer BackgroundContainer { get; }

        /// <summary>
        ///     The left side of the stage.
        /// </summary>
        private QuaverSprite StageLeft { get; set; }

        /// <summary>
        ///     The right side of the stage.
        /// </summary>
        private QuaverSprite StageRight { get; set; }

        /// <summary>
        ///     Bg Mask.
        /// </summary>
        private QuaverSprite BgMask { get; set; }

        /// <summary>
        ///     The size of the each ane.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private float LaneSize
        {
            get
            {
                switch (Map.Mode)
                {
                    case GameMode.Keys4:
                        return (int)(GameBase.LoadedSkin.ColumnSize4K * GameBase.WindowUIScale);
                    case GameMode.Keys7:
                        return (int)(GameBase.LoadedSkin.ColumnSize7K * GameBase.WindowUIScale);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        ///     The X size of the playfield.
        /// </summary>
        private float SizeX => (LaneSize + ReceptorPadding) * Map.FindKeyCountFromMode() + Padding * 2 - ReceptorPadding;

        /// <summary>
        ///     Padding of the playfield.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private float Padding
        {
            get
            {
                switch (Map.Mode)
                {
                    case GameMode.Keys4:
                        return (int)(GameBase.LoadedSkin.BgMaskPadding4K * GameBase.WindowUIScale);
                    case GameMode.Keys7:
                        return (int)(GameBase.LoadedSkin.BgMaskPadding7K * GameBase.WindowUIScale);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        ///     Padding of the receptor.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private float ReceptorPadding
        {
            get
            {
                switch (Map.Mode)
                {
                    case GameMode.Keys4:
                        return (int)(GameBase.LoadedSkin.NotePadding4K * GameBase.WindowUIScale);
                    case GameMode.Keys7:
                        return (int)(GameBase.LoadedSkin.NotePadding7K * GameBase.WindowUIScale);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        /// <summary>
        ///     Ctor - 
        /// </summary>
        internal KeysPlayfield(Qua map)
        {            
            Map = map;
            
            // Create the playfield's container.
            Container = new QuaverContainer();
            
            // Create background container
            BackgroundContainer = new QuaverContainer
            {
                Parent = Container,
                Size = new UDim2D(SizeX, GameBase.WindowRectangle.Height),
                Alignment = Alignment.TopCenter
            };
                 
            CreateBackgroundContainer();
        }
        
        /// <summary>
        ///     Init
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(IGameState state)
        {
        }

        /// <summary>
        ///     Unload
        /// </summary>
        public void UnloadContent()
        {
            Container.Destroy();
        }
        
        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            Container.Update(dt);
        }

        /// <summary>
        ///     Destroy
        /// </summary>
        public void Draw()
        {
            Container.Draw();
        }

        /// <summary>
        ///     Creates the entire background of the playfield.
        /// </summary>
        private void CreateBackgroundContainer()
        {
            CreateStage();     
        }

        /// <summary>
        ///     Creates the keys stage.
        /// </summary>
        private void CreateStage()
        {
            // Create the left side of the stage.
            var stageLeftX = GameBase.LoadedSkin.StageLeftBorder.Width * GameBase.WindowRectangle.Height / GameBase.LoadedSkin.StageLeftBorder.Height;
            StageLeft = new QuaverSprite()
            {
                Image = GameBase.LoadedSkin.StageLeftBorder,
                Size = new UDim2D(stageLeftX, GameBase.WindowRectangle.Height),
                Position = new UDim2D(-stageLeftX + 1, 0),
                Alignment = Alignment.TopLeft,
                Parent = BackgroundContainer
            };   
            
            // Create the right side of the stage.
            // Create Stage Right
            var stageRightX = GameBase.LoadedSkin.StageRightBorder.Width * GameBase.WindowRectangle.Height / GameBase.LoadedSkin.StageRightBorder.Height;
            StageRight = new QuaverSprite()
            {
                Image = GameBase.LoadedSkin.StageRightBorder,
                Size = new UDim2D(stageRightX, GameBase.WindowRectangle.Height),
                Position = new UDim2D(stageRightX - 1, 0),
                Alignment = Alignment.TopRight,
                Parent = BackgroundContainer
            };
            
            // Create BG Mask
            switch (Map.Mode)
            {
                case GameMode.Keys4:
                    CreateBgMask4K();
                    break;
                case GameMode.Keys7:
                    CreateBgMask7K();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
       ///     Creates the BG Mask for 4K.
       /// </summary>
        private void CreateBgMask4K()
        {
            var imageRatio = (double)GameBase.LoadedSkin.StageBgMask4K.Width / GameBase.LoadedSkin.StageBgMask4K.Height;
            var columnRatio = SizeX / GameBase.WindowRectangle.Height;
            var bgMaskSize = (float)Math.Max(GameBase.WindowRectangle.Height * columnRatio / imageRatio, GameBase.WindowRectangle.Height);
            
            BgMask = new QuaverSprite()
            {
                Image = GameBase.LoadedSkin.StageBgMask4K,
                Alpha = GameBase.LoadedSkin.BgMaskAlpha,
                Size = new UDim2D(SizeX, bgMaskSize),
                Alignment = Alignment.MidCenter,
                Parent = BackgroundContainer
            };
        }

        /// <summary>
        ///      Creates the BG Mask for 7K
        /// </summary>
        private void CreateBgMask7K()
        {
            // Create BG Mask
            var imageRatio = (double)GameBase.LoadedSkin.StageBgMask7K.Width / GameBase.LoadedSkin.StageBgMask7K.Height;
            var columnRatio = SizeX / GameBase.WindowRectangle.Height;
            var bgMaskSize = (float)Math.Max(GameBase.WindowRectangle.Height * columnRatio / imageRatio, GameBase.WindowRectangle.Height);

            BgMask = new QuaverSprite()
            {
                Image = GameBase.LoadedSkin.StageBgMask7K,
                Alpha = GameBase.LoadedSkin.BgMaskAlpha,
                Size = new UDim2D(SizeX, bgMaskSize),
                Alignment = Alignment.MidCenter,
                Parent = BackgroundContainer
            };
        }
    }
}