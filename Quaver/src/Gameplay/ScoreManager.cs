using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Logging;

namespace Quaver.Gameplay
{
    /// <summary>
    /// THIS CLASS IS IMPORTANT. This is where all the scoring will be calculated.
    /// This class will be updated in the future in such a way that it is near impossible to be manipulated with.
    /// </summary>
    internal class ScoreManager
    {
        //Hit Tracking (Judging/Scoring)
        public static int[] JudgeSpread { get; set; } = new int[6];
        public static int JudgeCount { get; set; }

        //Hit Tracking (ms deviance)
        public static List<double> MsDeviance { get; set; } = new List<double>();

        //Score tracking
        public static int ConsistancyMultiplier { get; set; }
        public static int Combo { get; set; }
        public static int Score { get; set; }

        //Accuracy
        public static float Accuracy { get; set; }

        //Hit Window
        internal static float[] HitWindow { get; } = new float[5] { 16, 43, 76, 106, 130 };

        /// <summary>
        /// This method is used to track and count scoring.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="offset"></param>
        public static void Count(int index, bool release = false, double? offset=null)
        {
            //Update Judge Spread
            JudgeSpread[index]++;
            JudgeCount++;

            //Update ms-deviance
            if (offset != null)
            {
                //Todo: ms deviance graphing/ displaying
                MsDeviance.Add((double)offset);
            }

            //Get new accuracy
            Accuracy = (JudgeSpread[0] + JudgeSpread[1] + JudgeSpread[2] / 1.5f + JudgeSpread[3] / 2f + JudgeSpread[4] / 4f) /JudgeCount;

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
            if (index < 5)
            LogManager.UpdateLogger(NoteManager.TimingNames[index], NoteManager.TimingNames[index]+": "+ JudgeSpread[index]);

            //Display stuff
            Playfield.UpdateJudge(index);

        }

    }
}
