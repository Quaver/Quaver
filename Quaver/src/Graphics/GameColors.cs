using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Quaver.Graphics
{
    public struct GameColors
    {
        /// <summary>
        /// Swan's favorite color; #db88c2. This color is used for the developers of the game.
        /// </summary>
        public static readonly Color NameTagAdmin = new Color(219, 136, 194, 1);

        /// <summary>
        /// Nametag color for Community Managers, Map Nominators, ect.
        /// </summary>
        public static readonly Color NameTagModerator = new Color(59, 233, 106, 1);

        /// <summary>
        /// Nametag color for Quaver Supporters/Donators.
        /// </summary>
        public static readonly Color NameTagSupporter = new Color(76, 146, 211, 1);

        /// <summary>
        /// Nametag color for regular users.
        /// </summary>
        public static readonly Color NameTagRegular = new Color(76, 146, 211, 1);

        //todo: documentation.
        //Judge Colors
        public static readonly Color JudgeMarv = new Color(255, 255, 200, 1);
        public static readonly Color JudgePerf = new Color(255, 255, 0, 1);
        public static readonly Color JudgeGreat = new Color(0, 255, 0, 1);
        public static readonly Color JudgeGood = new Color(0, 168, 255, 1);
        public static readonly Color JudgeOkay = new Color(255, 0, 255, 1);
        public static readonly Color JudgeMiss = new Color(255, 0, 0, 1);
        public static readonly Color[] JudgeColors = new Color[6] { JudgeMarv, JudgePerf, JudgeGreat, JudgeGood, JudgeOkay, JudgeMiss };

        //todo: documentation.
        //Grade Colors
        public static readonly Color GradeSSSS = new Color(255, 255, 255, 1);
        public static readonly Color GradeSSS = new Color(255, 255, 255, 1);
        public static readonly Color GradeSS = new Color(255, 255, 125, 1);
        public static readonly Color GradeS = new Color(255, 255, 0, 1);
        public static readonly Color GradeA = new Color(0, 255, 0, 1);
        public static readonly Color GradeB = new Color(0, 30, 255, 1);
        public static readonly Color GradeC = new Color(255, 0, 255, 1);
        public static readonly Color GradeD = new Color(255, 70, 0, 1);
        public static readonly Color GradeF = new Color(255, 0, 0, 1);
        public static readonly Color[] GradeColors = new Color[9] { GradeF, GradeD, GradeC, GradeB, GradeA, GradeS, GradeSS, GradeSSS, GradeSSSS };

        //todo: documentation.
        //Snap Colors
        public static readonly Color Snap1 = Color.Red;
        public static readonly Color Snap2 = Color.Blue;
        public static readonly Color Snap3 = Color.Purple;
        public static readonly Color Snap4 = Color.Yellow;
        public static readonly Color Snap6 = Color.Magenta;
        public static readonly Color Snap8 = Color.Orange;
        public static readonly Color Snap12 = Color.Cyan;
        public static readonly Color Snap16 = Color.Green;
        public static readonly Color Snap48 = Color.White;
        public static readonly Color[] SnapColors = new Color[9] { Snap1, Snap2, Snap3, Snap4, Snap6, Snap8, Snap12, Snap16, Snap48 };

        //todo: documentation
        //Logging and Other Game Colors
        public static readonly Color GameError = Color.Red;
        public static readonly Color GameWarning = Color.Yellow;
        public static readonly Color GameSuccess = Color.Green;
        public static readonly Color GameInfo = Color.LightGreen;
        public static readonly Color GameImportant = Color.LightCyan;
    }
}
