using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Graphics.Particles
{
    /// <summary>
    ///     Interface of Particle Object. Used for objects that will be manipulated by a Particle Manager
    /// </summary>
    internal abstract class Particle
    {
        /// <summary>
        ///     Is determined by how long the particle is active since it was first created
        /// </summary>
        public abstract double TimeElapsed { get; set; }

        /// <summary>
        ///     Determines how long the particle will be active for
        /// </summary>
        public abstract double DisplayTime { get; set; }

        /// <summary>
        ///     Determines if the particle is ready to be destroyed
        /// </summary>
        public abstract bool DestroyReady { get; set; }

        /// <summary>
        ///     Destroys particle
        /// </summary>
        public abstract void Destroy();

        /// <summary>
        ///     Updates particle
        /// </summary>
        /// <param name="dt"></param>
        public abstract void Update(double dt);
    }
}
