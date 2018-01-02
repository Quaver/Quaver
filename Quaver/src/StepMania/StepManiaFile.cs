using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quaver.StepMania
{
    internal class StepManiaFile
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

        /// <summary>
        ///     The BPMs of the map
        /// </summary>
        internal List<Bpm> Bpms { get; set; }

        /// <summary>
        ///     The list of charts in the map.
        /// </summary>
        internal List<Chart> Charts { get; set; }

        /// <summary>
        ///     Parses a StepManiaFile
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static StepManiaFile Parse(string path)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            var sm = new StepManiaFile { Bpms = new List<Bpm>(), Charts = new List<Chart>() };

            foreach (var line in File.ReadAllLines(path))
            {
                if (line.Contains("#"))
                {
                    var key = line.Substring(0, line.IndexOf(':')).Trim().ToUpper();
                    var value = line.Split(':').Last().Trim();

                    if (key != "#NOTES")
                        value = value.Replace(";", "");

                    switch (key)
                    {
                        case "#TITLE":
                            sm.Title = value;
                            Console.WriteLine(value);
                            break;
                        case "#SUBTITLE":
                            sm.Subtitle = value;
                            Console.WriteLine(value);
                            break;
                        case "#ARTIST":
                            sm.Artist = value;
                            Console.WriteLine(value);
                            break;
                        case "#CREDIT":
                            sm.Credit = value;
                            Console.WriteLine(value);
                            break;
                        case "#MUSIC":
                            sm.Music = value;
                            Console.WriteLine(value);
                            break;
                        case "#BACKGROUND":
                            sm.Background = value;
                            Console.WriteLine(value);
                            break;
                        case "#OFFSET":
                            sm.Offset = float.Parse(value);
                            Console.WriteLine(sm.Offset);
                            break;
                        case "#SAMPLESTART":
                            sm.SampleStart = float.Parse(value);
                            Console.WriteLine(sm.SampleStart);
                            break;
                        case "#BPMS":
                            var bpms = value.Split(',').ToList();

                            foreach (var bpm in bpms)
                            {
                                // An individual bpm is split by "offset=bpm"
                                var bpmSplit = bpm.Split('=').ToList();
                                sm.Bpms.Add(new Bpm { StartTime = float.Parse(bpmSplit[0]), BeatsPerMinute = float.Parse(bpmSplit[1])});
                                Console.WriteLine(bpmSplit[0] + "|" + bpmSplit[1]);
                            }
                            break;
                    }
                }
            }

            return sm;
        }
    }

    internal struct Bpm
    {
        /// <summary>
        ///     The start time of the BPM section
        /// </summary>
        internal float StartTime { get; set; }

        /// <summary>
        ///     The actual BPM
        /// </summary>
        internal float BeatsPerMinute { get; set; }
    }

    internal struct Chart
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
