using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Graphics;
using Quaver.Logging;

namespace Quaver.Gameplay
{
    /// <summary>
    /// THIS CLASS IS IMPORTANT. This is where all the scoring will be calculated.
    /// This class will be updated in the future in such a way that it is near impossible to be manipulated with.
    /// </summary>
    internal class ScoreManager
    {
        //Hit Timing Variables
        internal static string[] JudgeNames { get; } = new string[6] { "MARV", "PERF", "GREAT", "GOOD", "OKAY", "MISS" };

        //Hit Tracking (Judging/Scoring)
        public static int[] JudgePressSpread { get; set; }
        public static int[] JudgeReleaseSpread { get; set; }
        public static int JudgeCount { get; set; }

        //Hit Tracking (ms deviance)
        public static List<double> MsDeviance { get; set; }

        //Score tracking
        public static int ConsistancyMultiplier { get; set; }
        public static int Combo { get; set; }
        public static int Score { get; set; }

        //Accuracy
        public static float Accuracy { get; set; }

        //Hit Window
        internal static float[] HitWindow { get; } = new float[5] { 20, 44, 76, 106, 130 }; //todo: create OD curve

        /// <summary>
        /// This method is used to track and count scoring.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="offset"></param>
        public static void Count(int index, bool release = false, double? offset=null)
        {
            //Update Judge Spread
            if (release) JudgeReleaseSpread[index]++;
            else JudgePressSpread[index]++;
            JudgeCount++;

            //Update ms-deviance
            if (offset != null)
            {
                //Todo: ms deviance graphing/ displaying
                MsDeviance.Add((double)offset);
            }

            Accuracy = (JudgePressSpread[0] + JudgePressSpread[1] + JudgePressSpread[2] / 1.5f +
                        JudgePressSpread[3] / 2f + JudgePressSpread[4] / 4f);
            Accuracy += (JudgeReleaseSpread[0] + JudgeReleaseSpread[1] + JudgeReleaseSpread[2] / 1.5f +
                         JudgeReleaseSpread[3] / 2f + JudgeReleaseSpread[4] / 4f);
            Accuracy /= JudgeCount;

            //todo: actual score calculation
            Score = (int)(1000000f * JudgeCount / 20000f);

            //Update ConsistancyMultiplier and Combo
            if (index < 4)
            {
                Combo++;
                if (index < 2) ConsistancyMultiplier++;
                if (ConsistancyMultiplier > 200) ConsistancyMultiplier = 200;
            }
            else if (index >= 4)
            {
                Combo = 0;
                ConsistancyMultiplier -= 10;
                if (ConsistancyMultiplier < 0) ConsistancyMultiplier = 0;
            }

            //Update Score

            //log scores
            GameplayUI.UpdateAccuracyBox(index);

            //Display stuff
            Playfield.UpdateJudge(index, release, offset);

        }

        /// <summary>
        ///     Clear and Initialize Scoring related variables
        /// </summary>
        public static void Initialize()
        {
            Accuracy = 0;
            ConsistancyMultiplier = 0;
            Combo = 0;
            Score = 0;
            JudgeCount = 0;
            JudgeReleaseSpread = new int[6];
            JudgePressSpread = new int[6];
            MsDeviance = new List<double>();
        }
    }
}
