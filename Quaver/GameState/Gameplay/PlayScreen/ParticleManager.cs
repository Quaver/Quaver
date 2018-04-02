using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Graphics.Base;
using Quaver.Graphics.Particles.Gameplay;
using Quaver.Graphics.Sprites;

namespace Quaver.GameState.Gameplay.PlayScreen
{
    internal class ParticleManager : IHelper
    {
        /// <summary>
        ///     Particle Container
        /// </summary>
        private QuaverContainer QuaverContainer { get; set; }

        internal List<Particle> Particles { get; set; }

        public void Draw()
        {
            QuaverContainer.Draw();
        }

        public void Initialize(IGameState state)
        {
            QuaverContainer = new QuaverContainer();
            Particles = new List<Particle>();
        }

        public void UnloadContent()
        {
            QuaverContainer.Destroy();
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

            // Update QuaverContainer
            QuaverContainer.Update(dt);
        }

        internal void CreateHitBurst(DrawRectangle rect, int keyIndex)
        {
            Particles.Add(new HitEffect(rect, QuaverContainer, keyIndex));
        }
    }
}
