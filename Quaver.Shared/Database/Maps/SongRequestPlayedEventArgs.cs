using System;
using Quaver.Server.Common.Objects.Twitch;

namespace Quaver.Shared.Database.Maps
{
    public class SongRequestPlayedEventArgs : EventArgs
    {
        /// <summary>
        /// </summary>
        public SongRequest SongRequest { get; }

        /// <summary>
        /// </summary>
        public Map Map { get; }

        /// <summary>
        /// </summary>
        /// <param name="songRequest"></param>
        /// <param name="map"></param>
        public SongRequestPlayedEventArgs(SongRequest songRequest, Map map)
        {
            SongRequest = songRequest;
            Map = map;
        }
    }
}