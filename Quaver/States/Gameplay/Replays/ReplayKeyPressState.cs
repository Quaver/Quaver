using System;

namespace Quaver.States.Gameplay.Replays
{
    [Flags]
    public enum ReplayKeyPressState
    {
        K1 = 1 << 0,
        K2 = 1 << 1, 
        K3 = 1 << 2, 
        K4 = 1 << 3,
        K5 = 1 << 4,
        K6 = 1 << 5,
        K7 = 1 << 6,
    }
}