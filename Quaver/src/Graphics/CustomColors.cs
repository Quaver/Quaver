using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Quaver.Graphics
{
    public struct CustomColors
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

        public static readonly Color JudgeMarv = new Color(90, 255, 255, 1);
        public static readonly Color JudgePerf = new Color(255, 255, 0, 1);
        public static readonly Color JudgeGreat = new Color(0, 255, 0, 1);
        public static readonly Color JudgeGood = new Color(0, 168, 255, 1);
        public static readonly Color JudgeOkay = new Color(255, 0, 255, 1);
        public static readonly Color JudgeMiss = new Color(255, 0, 0, 1);

        public static readonly Color[] JudgeColors = new Color[6] { JudgeMarv, JudgePerf, JudgeGreat, JudgeGood, JudgeOkay, JudgeMiss };
    }
}
