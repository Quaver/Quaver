﻿using Quaver.GameState;
using Quaver.Graphics.Particles;
using Quaver.Graphics.Sprite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState.Gameplay.PlayScreen
{
    class ParticleManager : IHelper
    {
        /// <summary>
        ///     Particle Container
        /// </summary>
        internal Boundary Boundary { get; set; }

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
            Boundary.Update(dt);
        }
    }
}
