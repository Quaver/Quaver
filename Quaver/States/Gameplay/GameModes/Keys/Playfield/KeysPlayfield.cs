using System;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Config;
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
        ///     The background of the playfield.
        /// </summary>
        internal QuaverContainer BackgroundContainer { get; }
        
        /// <summary>
        ///     The foreground of the playfield.
        /// </summary>
        internal QuaverContainer ForegroundContainer { get; }

        /// <summary>
        ///     The container that holds all of the HitObjects
        /// </summary>
        internal QuaverContainer HitObjectContainer { get; set; }

        /// <summary>
        ///     Reference to the map.
        /// </summary>
        internal Qua Map { get; }

        /// <summary>
        ///     The stage for this playfield.
        /// </summary>
        internal KeysPlayfieldStage Stage { get; }

        /// <summary>
        ///     The X size of the playfield.
        /// </summary>
        internal float Width => (LaneSize + ReceptorPadding) * Map.FindKeyCountFromMode() + Padding * 2 - ReceptorPadding;

        /// <summary>
        ///     Padding of the playfield.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal float Padding
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
        ///     The size of the each ane.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal float LaneSize
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
        ///     Padding of the receptor.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal float ReceptorPadding
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
        ///     The Y position of the receptors.
        /// </summary>
        internal float ReceptorPositionY
        {
            get
            {
                switch (Map.Mode)
                {
                    case GameMode.Keys4:
                        if (ConfigManager.DownScroll4K.Value)
                            return GameBase.WindowRectangle.Height - (GameBase.LoadedSkin.ReceptorPositionOffset4K * GameBase.WindowUIScale + LaneSize * GameBase.LoadedSkin.NoteReceptorsUp4K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp4K[0].Width);
                        else
                            return GameBase.LoadedSkin.ReceptorPositionOffset4K * GameBase.WindowUIScale;
                    case GameMode.Keys7:
                        if (ConfigManager.DownScroll7K.Value)
                            return GameBase.WindowRectangle.Height - (GameBase.LoadedSkin.ReceptorPositionOffset7K * GameBase.WindowUIScale + LaneSize * GameBase.LoadedSkin.NoteReceptorsUp7K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp7K[0].Width);
                        else
                            return GameBase.LoadedSkin.ReceptorPositionOffset7K * GameBase.WindowUIScale;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        ///     The Y position of the column lighting
        /// </summary>
        internal float ColumnLightingPositionY
        {
            get
            {
                switch (Map.Mode)
                {
                    case GameMode.Keys4:
                        if (ConfigManager.DownScroll4K.Value)
                            return ReceptorPositionY;
                        else
                        {
                            var receptor = GameBase.LoadedSkin.NoteReceptorsUp4K[0];
                            var hitObject = GameBase.LoadedSkin.NoteHitObjects4K[0][0];
                            
                            return ReceptorPositionY + GameBase.LoadedSkin.ColumnSize4K * GameBase.WindowUIScale * (float)((double)receptor.Height / receptor.Width - (double)hitObject.Height / hitObject.Width);
                        }
                    case GameMode.Keys7:
                        if (ConfigManager.DownScroll7K.Value)
                            return ReceptorPositionY;
                        else
                        {
                            var receptor = GameBase.LoadedSkin.NoteReceptorsUp7K[0];
                            var hitObject = GameBase.LoadedSkin.NoteHitObjects7K[0][0];
                            
                            return ReceptorPositionY + GameBase.LoadedSkin.ColumnSize7K * GameBase.WindowUIScale * (float)((double)receptor.Height / receptor.Width - (double)hitObject.Height / hitObject.Width);
                        }
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
                        
            // Create the foreground container.
            ForegroundContainer = new QuaverContainer
            {
                Parent = Container,
                Size = new UDim2D(Width, GameBase.WindowRectangle.Height),
                Alignment = Alignment.TopCenter
            };
            
            // Create a new playfield stage               
            Stage = new KeysPlayfieldStage(this);
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
            // Animate Column Lighting
            Stage.PeformAllColumnLightingAnimations(dt);
            
            
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
        ///     Creates the HitObjectContainer.
        /// </summary>
        internal void CreateHitObjectContainer()
        {
            HitObjectContainer = new QuaverContainer
            {
                Size = new UDim2D(Width, 0, 0, 1),
                Alignment = Alignment.TopCenter,
                Parent = ForegroundContainer
            };
        }
    }
}