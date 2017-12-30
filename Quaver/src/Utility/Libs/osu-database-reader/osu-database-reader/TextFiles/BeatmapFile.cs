using System;
using System.Collections.Generic;
using System.IO;
using osu_database_reader;
using osu_database_reader.Components.Beatmaps;
using osu_database_reader.Components.HitObjects;

namespace osu_database_reader.TextFiles
{
    public class BeatmapFile
    {
        public int FileFormatVersion;

        public Dictionary<string, string> SectionGeneral;
        public Dictionary<string, string> SectionEditor;
        public Dictionary<string, string> SectionMetadata;
        public Dictionary<string, string> SectionDifficulty;
        public Dictionary<string, string> SectionColours;

        public List<TimingPoint> TimingPoints = new List<TimingPoint>();
        public List<HitObject> HitObjects = new List<HitObject>();

        //making some stuff easier to access
        public string Artist        => SectionMetadata.GetValueOrNull("Artist");
        public string ArtistUnicode => SectionMetadata.GetValueOrNull("ArtistUnicode");
        public string Creator       => SectionMetadata.GetValueOrNull("Creator");
        public string Title         => SectionMetadata.GetValueOrNull("Title");
        public string TitleUnicode  => SectionMetadata.GetValueOrNull("TitleUnicode");
        public string Version       => SectionMetadata.GetValueOrNull("Version");
        public string Source        => SectionMetadata.GetValueOrNull("Source");
        public string[] Tags        => SectionMetadata.GetValueOrNull("Tags")?.Split(' ');

        public float ApproachRate      => float.Parse(SectionDifficulty.GetValueOrNull("ApproachRate"), Constants.NumberFormat);
        public float HPDrainRate       => float.Parse(SectionDifficulty.GetValueOrNull("HPDrainRate"), Constants.NumberFormat);
        public float CircleSize        => float.Parse(SectionDifficulty.GetValueOrNull("CircleSize"), Constants.NumberFormat);
        public float OverallDifficulty => float.Parse(SectionDifficulty.GetValueOrNull("OverallDifficulty"), Constants.NumberFormat);
        public float SliderMultiplier  => float.Parse(SectionDifficulty.GetValueOrNull("SliderMultiplier"), Constants.NumberFormat);
        public float SliderTickRate    => float.Parse(SectionDifficulty.GetValueOrNull("SliderTickRate"), Constants.NumberFormat);

        public static BeatmapFile Read(string path)
        {
            var file = new BeatmapFile();

            using (var r = new StreamReader(path)) {
                if (!int.TryParse(r.ReadLine()?.Replace("osu file format v", string.Empty), out file.FileFormatVersion))
                    throw new Exception("Not a valid beatmap"); //very simple check, could be better

                BeatmapSection bs;
                while ((bs = r.ReadUntilSectionStart()) != BeatmapSection._EndOfFile) {
                    switch (bs) {
                        case BeatmapSection.General:
                            file.SectionGeneral = r.ReadBasicSection();
                            break;
                        case BeatmapSection.Editor:
                            file.SectionEditor = r.ReadBasicSection();
                            break;
                        case BeatmapSection.Metadata:
                            file.SectionMetadata = r.ReadBasicSection(false);
                            break;
                        case BeatmapSection.Difficulty:
                            file.SectionDifficulty = r.ReadBasicSection(false);
                            break;
                        case BeatmapSection.Events:
                            //TODO
                            r.SkipSection();
                            break;
                        case BeatmapSection.TimingPoints:
                            file.TimingPoints.AddRange(r.ReadTimingPoints());
                            break;
                        case BeatmapSection.Colours:
                            file.SectionColours = r.ReadBasicSection(true, true);
                            break;
                        case BeatmapSection.HitObjects:
                            file.HitObjects.AddRange(r.ReadHitObjects());
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return file;
        }
    }
}
