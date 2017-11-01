using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;
using Configuration = Quaver.Config.Configuration;

namespace Quaver.Audio
{
    internal static class AudioHandler
    {
        /// <summary>
        ///     Contains all of the audio streams which will be continuously freed throughout the game's lifecycle.
        /// </summary>
        internal static List<AudioStream> AllStreams { get; set; } = new List<AudioStream>();

        /// <summary>
        ///     Changes the master volume of all streams
        /// </summary>
        internal static void ChangeMasterVolume()
        {
            Bass.GlobalStreamVolume = Configuration.VolumeGlobal * 100;
        }

        /// <summary>
        ///     Frees all available streams, ran continuously throughout the game.
        ///     See: QuaverGame.elapsedEventHandler
        /// </summary>
        internal static void FreeAvailableStreams()
        {
            var newStreamList = new List<AudioStream>();
            Console.WriteLine(AllStreams.Count);

            foreach (var audioStream in AllStreams)
            {
                if (Bass.ChannelIsActive(audioStream.Stream) == PlaybackState.Stopped)
                    Bass.StreamFree(audioStream.Stream);
                else
                    newStreamList.Add(audioStream);
            }

            // Replace the list of streams with the new list of active ones.
            AllStreams = newStreamList;
        }
    }
}
