using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Metadata {
    // Is the beatmap's structure valid?
    public bool valid;

    // Song Title, Subtitle, & Artist
    public string title;
    public string subtitle;
    public string artist;

    // The file paths for the beatmap images
    public string bannerPath;
    public string backgroundPath;

    // Path for the MP3
    public string musicPath;

    // The offset that the song starts at compared to the hit objects
    public float offset;

    // The start and length of the sample that is played when selecting a song (PreviewTime in osu!)
    public float sampleStart;
    public float sampleLength;

    // The BPM the song is played at
    public float bpm;

    // The note data for each difficulty
    public NoteData beginner;
    public bool beginnerExists;

    public NoteData easy;
    public bool easyExists;

    public NoteData medium;
    public bool mediumExists;

    public NoteData hard;
    public bool hardExists;

    public NoteData challenge;
    public bool challengeExists;
}
