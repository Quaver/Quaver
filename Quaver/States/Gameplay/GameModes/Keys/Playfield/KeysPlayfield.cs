using System;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.GameState;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Main;

namespace Quaver.States.Gameplay.GameModes.Keys.Playfield
{
    internal class KeysPlayfield : IGameplayPlayfield
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public QuaverContainer Container { get; set; }

        /// <summary>
        ///     The background of this container.
        /// </summary>
        internal QuaverContainer BackgroundContainer { get; }
        
        /// <summary>
        ///     Reference to the map.
        /// </summary>
        internal Qua Map { get; }

        /// <summary>
        ///     The stage for this playfield.
        /// </summary>
        private KeysPlayfieldStage Stage { get; set; }

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
        internal float Width => (LaneSize + ReceptorPadding) * Map.FindKeyCountFromMode() + Padding * 2 - ReceptorPadding;

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
                Size = new UDim2D(Width, GameBase.WindowRectangle.Height),
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
            Stage = new KeysPlayfieldStage(this);   
        }
    }
}