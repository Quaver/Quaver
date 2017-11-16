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

        public static readonly Color JudgeMarvOpaque = new Color(90, 255, 255, 1);
        public static readonly Color JudgePerfOpaque = new Color(255, 255, 0, 1);
        public static readonly Color JudgeGreatOpaque = new Color(0, 255, 0, 1);
        public static readonly Color JudgeGoodOpaque = new Color(0, 168, 255, 1);
        public static readonly Color JudgeOkayOpaque = new Color(255, 0, 255, 1);
        public static readonly Color JudgeMissOpaque = new Color(255, 0, 0, 1);

        public static readonly Color JudgeMarvTransparent = new Color(90, 255, 255, 0.4f);
        public static readonly Color JudgePerfTransparent = new Color(255, 255, 0, 0.4f);
        public static readonly Color JudgeGreatTransparent = new Color(0, 255, 0, 0.4f);
        public static readonly Color JudgeGoodTransparent = new Color(0, 168, 255, 0.4f);
        public static readonly Color JudgeOkayTransparent = new Color(255, 0, 255, 0.4f);
        public static readonly Color JudgeMissTransparent = new Color(255, 0, 0, 0.4f);

        public static readonly Color[] JudgeColorsOpaque = new Color[6] { JudgeMarvOpaque, JudgePerfOpaque, JudgeGreatOpaque, JudgeGoodOpaque, JudgeOkayOpaque, JudgeMissOpaque };
        public static readonly Color[] JudgeColorsTransparent = new Color[6] { JudgeMarvTransparent, JudgePerfTransparent, JudgeGreatTransparent, JudgeGoodTransparent, JudgeOkayTransparent, JudgeMissTransparent };

        public static readonly Color TransparentBlack = new Color(0, 0, 0, 0.7f);
    }
}
