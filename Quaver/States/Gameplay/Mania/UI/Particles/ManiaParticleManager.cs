using System.Collections.Generic;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Particles;
using Quaver.Graphics.Particles.Gameplay;
using Quaver.Graphics.Sprites;

namespace Quaver.States.Gameplay.Mania.UI.Particles
{
    internal class ManiaParticleManager : IGameStateComponent
    {
        /// <summary>
        ///     Particle Container
        /// </summary>
        private Container Container { get; set; }

        internal List<Particle> Particles { get; set; }

        public void Draw()
        {
            Container.Draw();
        }

        public void Initialize(IGameState state)
        {
            Container = new Container();
            Particles = new List<Particle>();
        }

        public void UnloadContent()
        {
            Container.Destroy();
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
            Container.Update(dt);
        }

        internal void CreateHitBurst(DrawRectangle rect, int keyIndex)
        {
            Particles.Add(new HitEffect(rect, Container, keyIndex));
        }
    }
}
