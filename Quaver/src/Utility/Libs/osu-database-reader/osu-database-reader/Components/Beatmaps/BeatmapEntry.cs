using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Shared;
using osu.Shared.Serialization;

namespace osu_database_reader.Components.Beatmaps
{
    public class BeatmapEntry
    {
        public string Artist, ArtistUnicode;
        public string Title, TitleUnicode;
        public string Creator;  //mapper
        public string Version;  //difficulty name
        public string AudioFileName;
        public string BeatmapChecksum;
        public string BeatmapFileName;
        public SubmissionStatus RankedStatus;
        public ushort CountHitCircles, CountSliders, CountSpinners;
        public DateTime LastModifiedTime;
        public float ApproachRate, CircleSize, HPDrainRate, OveralDifficulty;
        public double SliderVelocity;
        public Dictionary<Mods, double> DiffStarRatingStandard, DiffStarRatingTaiko, DiffStarRatingCtB, DiffStarRatingMania;
        public int DrainTimeSeconds;    //NOTE: in s
        public int TotalTime;           //NOTE: in ms
        public int AudioPreviewTime;    //NOTE: in ms
        public List<TimingPoint> TimingPoints;
        public int BeatmapId, BeatmapSetId, ThreadId;
        public Ranking GradeStandard, GradeTaiko, GradeCtB, GradeMania;
        public short OffsetLocal;
        public float StackLeniency;
        public GameMode GameMode;
        public string SongSource, SongTags;
        public short OffsetOnline;
        public string TitleFont;
        public bool Unplayed;
        public DateTime LastPlayed;
        public bool IsOsz2;
        public string FolderName;
        public DateTime LastCheckAgainstOsuRepo;
        public bool IgnoreBeatmapSounds, IgnoreBeatmapSkin, DisableStoryBoard, DisableVideo, VisualOverride;
        public short OldUnknown1;   //unused
        public int Unknown2;
        public byte ManiaScrollSpeed;

        public static BeatmapEntry ReadFromReader(SerializationReader r, bool readLength = true, int version = 20160729) {
            BeatmapEntry e = new BeatmapEntry();

            int length = 0;
            if (readLength) length = r.ReadInt32();
            int startPosition = (int) r.BaseStream.Position;

            e.Artist = r.ReadString();
            e.ArtistUnicode = r.ReadString();
            e.Title = r.ReadString();
            e.TitleUnicode = r.ReadString();
            e.Creator = r.ReadString();
            e.Version = r.ReadString();
            e.AudioFileName = r.ReadString();
            e.BeatmapChecksum = r.ReadString(); //always 32 in length, so the 2 preceding bytes in the file are practically wasting space
            e.BeatmapFileName = r.ReadString();

            e.RankedStatus = (SubmissionStatus)r.ReadByte();
            e.CountHitCircles = r.ReadUInt16();
            e.CountSliders = r.ReadUInt16();
            e.CountSpinners = r.ReadUInt16();
            e.LastModifiedTime = r.ReadDateTime();

            if (version >= 20140609) {
                e.ApproachRate = r.ReadSingle();
                e.CircleSize = r.ReadSingle();
                e.HPDrainRate = r.ReadSingle();
                e.OveralDifficulty = r.ReadSingle();
            }
            else {
                e.ApproachRate = r.ReadByte();
                e.CircleSize = r.ReadByte();
                e.HPDrainRate = r.ReadByte();
                e.OveralDifficulty = r.ReadByte();
            }

            e.SliderVelocity = r.ReadDouble();

            if (version >= 20140609) {
                e.DiffStarRatingStandard = r.ReadDictionary<Mods, double>();
                e.DiffStarRatingTaiko = r.ReadDictionary<Mods, double>();
                e.DiffStarRatingCtB = r.ReadDictionary<Mods, double>();
                e.DiffStarRatingMania = r.ReadDictionary<Mods, double>();
            }

            e.DrainTimeSeconds = r.ReadInt32();
            e.TotalTime = r.ReadInt32();
            e.AudioPreviewTime = r.ReadInt32();

            e.TimingPoints = r.ReadSerializableList<TimingPoint>();
            e.BeatmapId = r.ReadInt32();
            e.BeatmapSetId = r.ReadInt32();
            e.ThreadId = r.ReadInt32();

            e.GradeStandard = (Ranking)r.ReadByte();
            e.GradeTaiko = (Ranking)r.ReadByte();
            e.GradeCtB = (Ranking)r.ReadByte();
            e.GradeMania = (Ranking)r.ReadByte();

            e.OffsetLocal = r.ReadInt16();
            e.StackLeniency = r.ReadSingle();
            e.GameMode = (GameMode)r.ReadByte();

            e.SongSource = r.ReadString();
            e.SongTags = r.ReadString();
            e.OffsetOnline = r.ReadInt16();
            e.TitleFont = r.ReadString();
            e.Unplayed = r.ReadBoolean();
            e.LastPlayed = r.ReadDateTime();

            e.IsOsz2 = r.ReadBoolean();
            e.FolderName = r.ReadString();
            e.LastCheckAgainstOsuRepo = r.ReadDateTime();

            e.IgnoreBeatmapSounds = r.ReadBoolean();
            e.IgnoreBeatmapSkin = r.ReadBoolean();
            e.DisableStoryBoard = r.ReadBoolean();
            e.DisableVideo = r.ReadBoolean();
            e.VisualOverride = r.ReadBoolean();
            if (version < 20140609)
                e.OldUnknown1 = r.ReadInt16();
            e.Unknown2 = r.ReadInt32();
            e.ManiaScrollSpeed = r.ReadByte();

            int endPosition = (int) r.BaseStream.Position;
            Debug.Assert(!readLength || length == endPosition - startPosition); //could throw error here

            return e;
        }
    }
}
