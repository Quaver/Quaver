using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Qua
{
    public class QuaFile
    {
        // <--- #General --->
        /// <summary>
        /// Keeps track of whether or not the data in the QuaFile object is valid.
        /// </summary>
        public bool IsValidQua;

        /// <summary>
        ///  The name of the audio file (.ogg)
        /// </summary>
        public string AudioFile;

        /// <summary>
        /// The amount of time before the audio starts.
        /// </summary>
        public int AudioLeadIn;

        /// <summary>
        /// The offset of the song which will be used to play a preview during song select
        /// </summary>
        public int SongPreviewTime;

        /// <summary>
        /// The file name of the song background.
        /// </summary>
        public string BackgroundFile;
        // <--- #General End --->
        
        // <--- #Metadata --->
        /// <summary>
        /// The title of the song.
        /// </summary>
        public string Title;

        /// <summary>
        /// The unicode title of the song.
        /// </summary>
        public string TitleUnicode;

        /// <summary>
        /// The artist of the song.
        /// </summary>
        public string Artist;

        /// <summary>
        /// The unicode artist of the song.
        /// </summary>
        public string ArtistUnicode;

        /// <summary>
        /// The source of the song (Album, Mixtape, etc.)
        /// </summary>
        public string Source;

        /// <summary>
        /// Specific tags for the song (Used when searching)
        /// </summary>
        public string Tags;

        /// <summary>
        /// The creator of the beatmap
        /// </summary>
        public string Creator;

        /// <summary>
        /// The name of the difficulty for the beatmap
        /// </summary>
        public string DifficultyName;

        /// <summary>
        /// The map's unique identifier if it was uploaded.
        /// </summary>
        public int MapID;

        /// <summary>
        /// The mapset's unique identifier if it was uploaded.
        /// </summary>
        public int MapSetID;

        /// <summary>
        /// A user defined description of the map.
        /// </summary>
        public string Description;
        // <--- #Metadata End -->
        
        // <--- #Difficulty -->
        /// <summary>
        /// The amount of hitpoints drain on the map.
        /// </summary>
        public float HPDrain;

        /// <summary>
        /// The difficulty of accuracy for the map.
        /// </summary>
        public float AccuracyStrain;
        // <--- #Difficulty End -->
        
        /// <summary>
        /// All of the map's timing sections.
        /// </summary>
        public List<TimingPoint> TimingPoints;
       
        /// <summary>
        /// The points in the map where the SV changes.
        /// </summary>
        public List<SliderVelocity> SliderVelocities;

        /// <summary>
        /// The physical hit objects in the map.
        /// </summary>
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

