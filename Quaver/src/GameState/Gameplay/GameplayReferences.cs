using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState.Gameplay
{
    /// <summary>
    ///     This class holds any reference variables for the gameplay state.
    ///     Most of these variables will be gone later when a better flow is implemented.
    /// </summary>
    internal static class GameplayReferences
    {
        internal static string[] JudgeNames { get; } = new string[6] { "MARV", "PERF", "GREAT", "GOOD", "OKAY", "MISS" };
        internal static Texture2D[] GradeImages { get; } = new Texture2D[9] {GameBase.LoadedSkin.GradeSmallF, GameBase.LoadedSkin.GradeSmallD, GameBase.LoadedSkin.GradeSmallC, GameBase.LoadedSkin.GradeSmallB,
                                                                        GameBase.LoadedSkin.GradeSmallA, GameBase.LoadedSkin.GradeSmallS, GameBase.LoadedSkin.GradeSmallSS, GameBase.LoadedSkin.GradeSmallX, GameBase.LoadedSkin.GradeSmallXX};
        //todo: temp note rendering reference variables
        //internal static ulong[] SvCalc { get; set; }

        //todo: temp variables for scoremanager
        internal static float PressWindowLatest { get; set; } // HitWindowPress[4]
        internal static float ReleaseWindowLatest { get; set; } // HitWindowRelease[3]

        //todo: temp Timing variables
        //internal static List<TimingObject> SvQueue { get; set; }
        internal static uint PlayStartDelayed { get; } = 3000;

        //todo: temp GameplayUI variables
        //internal static bool NoteHolding { get; set; }

        //todo: temp Playfield variables
        internal static float PlayfieldSize { get; set; } //todo: remove
        //internal static float PlayfieldLaneSize { get; set; } //todo: remove
        internal static float[] ReceptorXPosition { get; set; } //used more than once
        internal static float ReceptorYOffset { get; set; } //used more than once

    }
}
