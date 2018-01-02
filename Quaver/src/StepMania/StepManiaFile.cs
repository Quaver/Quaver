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

            var file = File.ReadAllLines(path);

            var inNotes = false; // Keeps track of if we are currently in the #NOTES section
            var inNoteData = false;
            var currentColons = 0; // Keeps track of the c

            foreach (var line in file)
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
                            continue;
                        case "#SUBTITLE":
                            sm.Subtitle = value;
                            continue;
                        case "#ARTIST":
                            sm.Artist = value;
                            continue;
                        case "#CREDIT":
                            sm.Credit = value;
                            continue;
                        case "#MUSIC":
                            sm.Music = value;
                            continue;
                        case "#BACKGROUND":
                            sm.Background = value;
                            continue;
                        case "#OFFSET":
                            sm.Offset = float.Parse(value);
                            continue;
                        case "#SAMPLESTART":
                            sm.SampleStart = float.Parse(value);
                            continue;
                        case "#BPMS":
                            var bpms = value.Split(',').ToList();

                            foreach (var bpm in bpms)
                            {
                                // An individual bpm is split by "offset=bpm"
                                var bpmSplit = bpm.Split('=').ToList();
                                sm.Bpms.Add(new Bpm { StartTime = float.Parse(bpmSplit[0]), BeatsPerMinute = float.Parse(bpmSplit[1])});
                            }
                            continue;
                        case "#NOTES":
                            inNotes = true;
                            sm.Charts.Add(new Chart { Measures = new List<NoteMeasure>() });
                            currentColons = 0; // Reset colon counter.
                            inNoteData = false;
                            continue;
                    }                    
                }

                if (inNotes)
                {
                    // Skip comments
                    if (line.Contains("//"))
                        continue;

                    if (line.Contains(":"))
                        currentColons++;

                    switch (currentColons)
                    {
                        // Type of map
                        case 1:
                            // Only parse dance-single maps. Setting inNotes to false will 
                            // cause the parser to skip this step in the parsing phase until it reaches
                            // another #NOTES section - Remove the current chart as well
                            if (!line.ToLower().Contains("dance-single"))
                            {
                                sm.Charts.Remove(sm.Charts.Last());
                                inNotes = false;
                                continue;
                            }

                            sm.Charts.Last().ChartType = line.Trim().Trim(':');
                            break;
                        // Creator? Don't need to parse this.
                        case 2:
                            break;
                        // Difficulty
                        case 3:
                            if (line.ToLower().Contains("beginner") || line.ToLower().Contains("easy") ||
                                line.ToLower().Contains("medium") || line.ToLower().Contains("hard") ||
                                line.ToLower().Contains("challenge"))
                            {
                                sm.Charts.Last().Difficulty = line.Trim().Trim(':');
                            }
                            break;
                        // Numerical meter, doesn't need parsing.
                        case 4:
                            break;
                        // Groove Radar Values - doesn't need parsing.
                        case 5:
                            // However, after this line, we will be parsing note data, 
                            inNoteData = true;
                            currentColons = -1; // Set the current colons back to -1 to avoid skipping.
                            sm.Charts.Last().Measures.Add(new NoteMeasure { NoteRows = new List<NoteRow>() }); // Add first measure of notes
                            continue;
                    }

                    // Parse Note Data
                    if (inNoteData)
                    {
                        // If there are 4 characters in this line, that must mean we're at a row of objects
                        if (line.Trim().Length == 4)
                        {
                            var row = new NoteRow
                            {
                                Lane1 = NoteType.None,
                                Lane2 = NoteType.None,
                                Lane3 = NoteType.None,
                                Lane4 = NoteType.None
                            };

                            // Turn the row of objects into an array so we can try to parse each row.
                            var rowArray = line.Trim().ToCharArray();

                            for (var i = 0; i < rowArray.Length; i++)
                            {
                                switch (i)
                                {
                                    // Lane 1
                                    case 0:
                                        row.Lane1 = Enum.TryParse(rowArray[i].ToString(), out NoteType typeLane1) ? typeLane1 : NoteType.None;
                                        break;
                                    // Lane 2
                                    case 1:
                                        row.Lane2 = Enum.TryParse(rowArray[i].ToString(), out NoteType typeLane2) ? typeLane2 : NoteType.None;
                                        break;
                                    // Lane 3
                                    case 2:
                                        row.Lane3 = Enum.TryParse(rowArray[i].ToString(), out NoteType typeLane3) ? typeLane3 : NoteType.None;
                                        break;
                                    // Lane 4
                                    case 3:
                                        row.Lane4 = Enum.TryParse(rowArray[i].ToString(), out NoteType typeLane4) ? typeLane4 : NoteType.None;
                                        break;
                                }
                            }

                            // Add the row to the list of measures
                            sm.Charts.Last().Measures.Last().NoteRows.Add(row);
                            continue;
                        }

                        // If the line is a ',', that means it marks the end of a new measure.
                        if (line.Trim().Contains(","))
                            sm.Charts.Last().Measures.Add(new NoteMeasure {NoteRows = new List<NoteRow>()});
                    }
                }
            }

            return sm;
        }
    }

    /// <summary>
    ///     The #BPMS section of the metadata header
    /// </summary>
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

    /// <summary>
    ///     The chart date itself, also known as the #NOTES section of the file
    /// </summary>
    internal class Chart
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
        ///     The list measures containing note data
        /// </summary>
        internal List<NoteMeasure> Measures { get; set; }
    }

    // An individual measure for the map (as separated by ',' in the .sm file)
    internal struct NoteMeasure
    {
        /// <summary>
        ///     The list of notes in this measure
        /// </summary>
        internal List<NoteRow> NoteRows { get; set; }
    }

    /// <summary>
    ///     An individual row of notes
    /// </summary>
    internal struct NoteRow
    {
        internal NoteType Lane1 { get; set; }
        internal NoteType Lane2 { get; set; }
        internal NoteType Lane3 { get; set; }
        internal NoteType Lane4 { get; set; }
    }

    /// <summary>
    ///     The type of note
    /// </summary>
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
