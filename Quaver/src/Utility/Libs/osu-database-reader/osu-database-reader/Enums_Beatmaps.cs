namespace osu_database_reader
{
    public enum CurveType
    {
        Linear,
        Catmull,
        Bezier,
        Perfect
    }

    public enum BeatmapSection
    {
        _EndOfFile,
        General,
        Editor,
        Metadata,
        Difficulty,
        Events,
        TimingPoints,
        Colours,
        HitObjects,
    }
}
