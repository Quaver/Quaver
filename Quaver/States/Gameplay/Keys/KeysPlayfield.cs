using System;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.GameState;
using Quaver.Graphics.Sprites;
using Quaver.Main;

namespace Quaver.States.Gameplay.Keys
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
        private Qua Map { get; set; }

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
        ///     Ctor - 
        /// </summary>
        internal KeysPlayfield(Qua map)
        {
            Container = new QuaverContainer();
            Map = map;
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
    }
}