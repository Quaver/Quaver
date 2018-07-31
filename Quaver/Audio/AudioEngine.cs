using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Database.Maps;
using Wobble.Audio.Tracks;

namespace Quaver.Audio
{
    public static class AudioEngine
    {
        /// <summary>
        ///     The AudioTrack for the currently selected map.
        /// </summary>
        public static AudioTrack Track { get; internal set; }

        /// <summary>
        ///     Loads the track for the currently selected map.
        /// </summary>
        public static void LoadCurrentTrack()
        {
            if (Track != null && !Track.IsDisposed)
                Track.Dispose();

            if (!File.Exists(MapManager.CurrentAudioPath))
                throw new FileNotFoundException($"The audio file at path: {MapManager.CurrentAudioPath} could not be found.");

            Track = new AudioTrack(MapManager.CurrentAudioPath);
        }
    }
}
