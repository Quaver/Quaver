using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Quaver.Logging
{
    class LogObject
    {
        /// <summary>
        /// The name of the Log-Tracking Object
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// What message the Log Object will display
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The color of the message
        /// </summary>
        public Color LogColor { get; set; }

        /// <summary>
        /// How long the message will be shown for.
        /// </summary>
        public float Duration { get; set; }

        /// <summary>
        /// This bool determines whether the object will be removed after the duration variable.
        /// </summary>
        public bool NoDuration { get; set; } = true;
    }
}
