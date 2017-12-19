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
        internal static ulong[] SvCalc { get; set; }

        //todo: temp variables for scoremanager
        internal static int Combo { get; set; }
        //internal static float HitWindowSize { get; set; } = 1; // = HitWindowSize[4] * 2 * GameBase.WindowYRatio;
        internal static float ScoreTotal { get; set; } = 0;
        internal static int GradeIndex { get; set; } = 0; // PlayScreen.GetRelativeGradeIndex()
        internal static float Accuracy { get; set; } = 0;
        internal static float AccScale { get; set; } // PlayScreen.RelativeAccGetScale()
        internal static float PressWindowLatest { get; set; } = 70; // HitWindowPress[4]
        internal static float ReleaseWindowLatest { get; set; } = 140; // HitWindowRelease[3]

        //todo: temp Timing variables
        internal static double CurrentSongTime { get; set; }
        internal static List<TimingObject> SvQueue { get; set; }
        internal static int PlayStartDelayed { get; set; }

        //todo: temp GameplayUI variables
        internal static bool NoteHolding { get; set; }

        //todo: temp Playfield variables
        internal static float PlayfieldSize { get; set; } //todo: remove
        internal static float PlayfieldObjectSize { get; set; } //todo: remove
        internal static float[] ReceptorXPosition { get; set; } //used more than once
        internal static float ReceptorYOffset { get; set; } //used more than once

    }
}
