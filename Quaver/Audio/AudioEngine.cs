using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;

namespace Quaver.Audio
{
    internal class AudioImplementation
    {
        internal static void Play(string filePath)
        {
            if (Bass.Init())
            {
                var stream = Bass.CreateStream(filePath);

                if (stream != 0)
                    Bass.ChannelPlay(stream);
                else Console.WriteLine("[AUDIO ENGINE] Error: {0}", Bass.LastError);
            }
        }
    }
}
