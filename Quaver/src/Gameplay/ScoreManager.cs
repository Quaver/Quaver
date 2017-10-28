using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Gameplay
{
    /// <summary>
    /// THIS CLASS IS IMPORTANT. This is where all the scoring will be calculated.
    /// This class will be updated in the future in such a way that it is near impossible to be manipulated with.
    /// </summary>
    internal class ScoreManager
    {
        //Hit Judge tracking
        public static int ScoreMarvelous { get; set; }
        public static int ScorePerfect { get; set; }
        public static int ScoreGreat { get; set; }
        public static int ScoreGood { get; set; }
        public static int ScoreBad { get; set; }
        public static int ScoreMiss { get; set; }

        //Hit Window
        internal static float[] HitWindow { get; } = new float[5] { 16, 43, 76, 106, 130 };

    }
}
