using System.Collections.Generic;
using Quaver.API.Maps.Parsers;

namespace Quaver.Parsers.Etterna
{
    /// <summary>
    ///     Class that 
    /// </summary>
    internal class EtternaCache
    {
        /// <summary>
        ///     The version of the cache I assume
        /// </summary>
        internal string Version { get; set; }

        /// <summary>
        ///     The title of the song
        /// </summary>
        internal string Title { get; set; }

        /// <summary>
        ///     Subtitle of the song
        /// </summary>
        internal string Subtitle { get; set; }

        /// <summary>
        ///     The artist of the song
        /// </summary>
        internal string Artist { get; set; }

        /// <summary>
        ///     The creator of the chart
        /// </summary>
        internal string Credit { get; set; }

        /// <summary>
        ///     The file path of the banner
        /// </summary>
        internal string Banner { get; set; }

        /// <summary>
        ///     The file path of the background file
        /// </summary>
        internal string Background { get; set; }

        /// <summary>
        ///     Not sure
        /// </summary>
        internal string CdTitle { get; set; }

        /// <summary>
        ///     Path to the music file
        /// </summary>
        internal string Music { get; set; }

        /// <summary>
        ///     The time the audio plays 
        /// </summary>
        internal float SampleStart { get; set; }

        /// <summary>
        ///     Where the map file is located
        /// </summary>
        internal string SongFileName { get; set; }

        /// <summary>
        ///     Another value that determines where the file is located
        /// </summary>
        internal string StepFileName { get; set; }

        /// <summary>
        ///     The data for each chart in this .sm file
        /// </summary>
        internal List<Chart> ChartData { get; set; }
    }
}
