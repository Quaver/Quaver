using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.StepMania
{
    internal class Sm
    {
        /// <summary>
        ///     The title of the track
        /// </summary>
        internal string Title { get; set; }

        /// <summary>
        ///     The source equivalent in osu?
        /// </summary>
        internal string Subtitle { get; set; }

        /// <summary>
        ///     The artist of the track
        /// </summary>
        internal string Artist { get; set; }

        /// <summary>
        ///     The creator of the map
        /// </summary>
        internal string Credit { get; set; }

        /// <summary>
        ///     The audio file
        /// </summary>
        internal string Music { get; set; }

        /// <summary>
        ///     The background file
        /// </summary>
        internal string Background { get; set; }

        /// <summary>
        ///     The offset that the song starts at
        /// </summary>
        internal float Offset { get; set; }

        /// <summary>
        ///     The time in the song where the song's preview is played.
        /// </summary>
        internal float SampleStart { get; set; }
    }

    internal struct Bpms
    {
        /// <summary>
        ///     The start time of the BPM section
        /// </summary>
        internal float StartTime { get; set; }

        /// <summary>
        ///     The actual BPM
        /// </summary>
        internal float Bpm { get; set; }
    }

    internal struct Charts
    {
        /// <summary>
        ///     The type of chart
        ///     dance-single, etc.
        /// </summary>
        internal string ChartType { get; set; }

        /// <summary>
        ///     The description/author of the chart
        /// </summary>
        internal string Description { get; set; }

        /// <summary>
        ///     The difficulty name of the chart
        /// </summary>
        internal string Difficulty { get; set; }

        /// <summary>
        ///     The list of ntoe data
        /// </summary>
        internal List<NoteType> Notes { get; set; }
    }

    internal enum NoteType
    {
        None, // 0
        Normal, // 1
        HoldHead, // 2
        HoldTail, // 3
        RollHead, // 4
        Mine // M
    }
}
