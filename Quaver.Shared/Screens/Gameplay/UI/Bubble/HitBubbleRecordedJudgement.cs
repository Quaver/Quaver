using System;

namespace Quaver.Shared.Screens.Gameplay.UI.Bubble;

[Flags]
public enum HitBubbleRecordedJudgement
{
    Marv = 1 << 0,
    Perf = 1 << 1,
    Great = 1 << 2,
    Good = 1 << 3,
    Okay = 1 << 4,
    Miss = 1 << 5,
    Default = NoMarv,
    NoMarv =  Perf | Great | Good | Okay | Miss,
    All = Marv | NoMarv
}