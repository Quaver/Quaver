using System;

namespace osu.Shared
{
    [Flags]
    public enum HitObjectType : byte
    {
        Normal      = 0b0000_0001,
        Slider      = 0b0000_0010,
        NewCombo    = 0b0000_0100,
        Spinner     = 0b0000_1000,
        ColourHax   = 0b0111_0000,  //color "RGB" mask
        Hold        = 0b1000_0000,
    }

    [Flags]
    public enum HitSound : byte
    {
        None    = 0b0000,
        Normal  = 0b0001,
        Whistle = 0b0010,
        Finish  = 0b0100,
        Clap    = 0b1000,
}
}
