using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Particles;
using Quaver.Graphics.Sprite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Graphics.Particles.Gameplay;

namespace Quaver.GameState.Gameplay.PlayScreen
{
    internal class ParticleManager : IHelper
    {
        /// <summary>
        ///     Particle Container
        /// </summary>
        private Boundary Boundary { get; set; }

        internal List<Particle> Particles { get; set; }

        public void Draw()
        {
            Boundary.Draw();
        }

        public void Initialize(IGameState state)
        {
            Boundary = new Boundary();
            Particles = new List<Particle>();
        }

        public void UnloadContent()
        {
            Boundary.Destroy();
        }

        public void Update(double dt)
        {
            // Update Every Particle
            foreach(var ob in Particles)
            {
                ob.Update(dt);
            }

            // Check if any particles are ready to be destroyed. Will loop through it 8 times max per frame.
            for (var i = 0; i < 8; i++)
            {
                if (Particles.Count <= 0)
                    break;

                if (Particles[0].DestroyReady)
                {
                    Particles[0].Destroy();
                    Particles.RemoveAt(0);
                }
            }

            // Update Boundary
            Boundary.Update(dt);
        }

        internal void CreateHitBurst(DrawRectangle rect, int keyIndex)
        {
            Particles.Add(new HitEffect(rect, Boundary, keyIndex));
        }
    }
}
