using System;

namespace osu.Shared
{
    public enum GameMode : byte
    {
        Standard = 0,
        Taiko = 1,
        CatchTheBeat = 2,
        Mania = 3
    }

    public enum Ranking : byte
    {
        XH, //silver SS
        SH, //silver S
        X,  //SS
        S,
        A,
        B,
        C,
        D,
        F,  //Failed
        N   //None
    }

    [Flags]
    public enum PlayerRank : byte
    {
        None       = 0b0000_0000,   //Not logged in
        Default    = 0b0000_0001,   //Standard, logged in user
        Bat        = 0b0000_0010,   //Beatmap Approval Team
        Supporter  = 0b0000_0100,
        Friend     = 0b0000_1000,   //unused
        SuperMod   = 0b0001_0000,   //peppy, blue name in chat
        Tournament = 0b0010_0000,   //allowed to host tournaments
    }

    public enum SubmissionStatus : byte
    {
        Unknown = 0,
        NotSubmitted = 1,
        Pending = 2,        //both pending and graveyarded
        EditableCutoff = 3, //not used anymore
        Ranked = 4,
        Approved = 5,
        Qualified = 6,
        Loved = 7
    }

    [Flags]
    public enum Mods
    {
        None        = 0,
        NoFail      = 1 << 0,
        Easy        = 1 << 1,
        TouchDevice = 1 << 2,   //previously NoVideo
        Hidden      = 1 << 3,
        HardRock    = 1 << 4,
        SuddenDeath = 1 << 5,
        DoubleTime  = 1 << 6,
        Relax       = 1 << 7,
        HalfTime    = 1 << 8,
        Nightcore   = 1 << 9,
        Flashlight  = 1 << 10,
        Autoplay    = 1 << 11,
        SpunOut     = 1 << 12,
        Relax2      = 1 << 13,  //AutoPilot
        Perfect     = 1 << 14,
        Key4        = 1 << 15,
        Key5        = 1 << 16,
        Key6        = 1 << 17,
        Key7        = 1 << 18,
        Key8        = 1 << 19,
        FadeIn      = 1 << 20,
        Random      = 1 << 21,
        Cinema      = 1 << 22,
        Target      = 1 << 23,
        Key9        = 1 << 24,
        KeyCoop     = 1 << 25,
        Key1        = 1 << 26,
        Key3        = 1 << 27,
        Key2        = 1 << 28,
        ScoreV2     = 1 << 29,
    }
}
