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
        internal static string[] JudgeNames { get; } = new string[6] { "Marvelous", "Perfect", "Great", "Good", "Okay", "Miss" };

        //todo: temp variables for scoremanager
        //internal static float PressWindowLatest { get; set; } // HitWindowPress[4]
        //internal static float ReleaseWindowLatest { get; set; } // HitWindowRelease[3]

        //todo: temp Playfield variables
        //internal static float PlayfieldSize { get; set; } //todo: remove
        //internal static float PlayfieldLaneSize { get; set; } //todo: remove
        internal static float[] ReceptorXPosition { get; set; } //used more than once
        //internal static float ReceptorYOffset { get; set; } //used more than once
    }
}
