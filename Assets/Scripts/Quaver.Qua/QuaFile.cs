
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Qua
{
    public class QuaFile
    {
        // # General
        public bool IsValidQua;
        public string AudioFile;
        public int AudioLeadIn;
        public int SongPreviewTime;
        public string BackgroundFile;

        // # Metadata
        public string Title;
        public string TitleUnicode;
        public string Artist;
        public string ArtistUnicode;
        public string Source;
        public string Tags;
        public string Creator;
        public string DifficultyName;
        public int MapID;
        public int MapSetID;
        public string Description;

        // Difficulty
        public float HPDrain;
        public float AccuracyStrain;

        // Timing
        public List<TimingPoint> TimingPoints;

        // SV
        public List<SliderVelocity> SliderVelocities;

        // HitObjects
        public List<HitObject> HitObjects;
    
        // Constructor
        public QuaFile()
        {
            // Initialize a valid QuaFile
            this.TimingPoints = new List<TimingPoint>();
            this.SliderVelocities = new List<SliderVelocity>();
            this.HitObjects = new List<HitObject>();
            this.IsValidQua = true;
        }
    }
}

