
using Microsoft.Xna.Framework;
using Quaver.GameState;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.States;

namespace Quaver.Graphics.Overlays.Volume
{
    /// <summary>
    ///     Volume controller
    /// </summary>
    internal class VolumeController : IGameStateComponent
    {
        /// <summary>
        ///     The container for the entire VC.
        /// </summary>
        private QuaverContainer Container { get; set; }

        /// <summary>
        ///     The volume control box
        /// </summary>
        private QuaverSprite SurroundingBox { get; set; }

        /// <summary>
        ///     Init
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(IGameState state)
        {
            Container = new QuaverContainer();

            // Create the surrounding box where the volume control will 
            
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
        ///     Draw
        /// </summary>
        public void Draw()
        {
            Container.Draw();
        }
    }
}