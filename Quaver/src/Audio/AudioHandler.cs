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
        ///     Our Global list of used BASS audio streams
        ///     Essentially it's how many loaded streams we have.
        ///     Periodically all unused streams will get freed to prevent memory leaks,
        ///     as the game continuously loads new audio streams for GameEffects,
        ///     to play multiple of the same effect at one time if possible.
        ///     TODO: Find a better fix for this.
        /// </summary>
        internal static List<int> GlobalAudioStreams { get; set; } = new List<int>();

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
            var newStreamList = new List<int>();

            foreach (var audioStream in GlobalAudioStreams)
            {
                if (Bass.ChannelIsActive(audioStream) == PlaybackState.Stopped)
                    Bass.StreamFree(audioStream);
                else
                    newStreamList.Add(audioStream);
            }

            // Replace the list of streams with the new list of active ones.
            GlobalAudioStreams = newStreamList;
        }
    }
}
