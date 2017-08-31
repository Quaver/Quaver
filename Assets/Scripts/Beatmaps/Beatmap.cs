using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Beatmap {

    public bool IsValid;
    public string OsuFileFormat;

    // [General]
    public string AudioFilename;
    public int AudioLeadIn;
    public int PreviewTime;
    public int Countdown;
    public string SampleSet;
    public float StackLeniency;
    public int Mode;
    public int LetterboxInBreaks;
    public int SpecialStyle;
    public int WidescreenStoryboard;

    // [Editor]
    public string Bookmarks;
    public float DistanceSpacing;
    public int BeatDivisor;
    public int GridSize;
    public float TimelineZoom;

    // [Metadata]
    public string Title;
    public string TitleUnicode;
    public string Artist;
    public string ArtistUnicode;
    public string Creator;
    public string Version;
    public string Source;
    public string Tags;
    public int BeatmapID;
    public int BeatmapSetID;

    // [Difficulty]
    public float HPDrainRate;
    public int KeyCount;
    public float OverallDifficulty;
    public float ApproachRate;
    public float SliderMultiplier;
    public float SliderTickRate;

    // [Events]
    public string Background;

    // [TimingPoints]
    List<TimingPoint> TimingPoints;

    // [HitObjects]
    List<HitObject> HitObjects;
}
