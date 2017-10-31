using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Audio
{
    internal class AudioStream
    {
        /// <summary>
        ///     The actual stream
        /// </summary>
        internal int Stream { get; set; }

        /// <summary>
        ///     Is the stream an effect?
        /// </summary>
        internal bool IsEffect { get; set; }
    }
}
