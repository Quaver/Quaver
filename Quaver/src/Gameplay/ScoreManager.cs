using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.GameState.States;
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
        internal PlayScreenState PlayScreen { get; set; }
        //Hit Timing Variables
        internal string[] JudgeNames { get; } = new string[6] { "MARV", "PERF", "GREAT", "GOOD", "OKAY", "MISS" };

        //Hit Tracking (Judging/Scoring)
        internal int[] JudgePressSpread { get; set; }
        internal int[] JudgeReleaseSpread { get; set; }
        internal int JudgeCount { get; set; }

        //Hit Tracking (ms deviance)
        internal List<double> MsDeviance { get; set; }

        //Score tracking
        internal int ConsistancyMultiplier { get; set; }
        internal int Combo { get; set; }
        internal int Score { get; set; }

        //Accuracy
        internal double Accuracy { get; set; }
        internal int[] HitWeighting { get; } = new int[6] { 100, 100, 50, 25, -100, -200};

        //Hit Window
        internal float[] HitWindow { get; } = new float[5] { 20, 44, 76, 106, 130 }; //todo: create OD curve

        /// <summary>
        /// This method is used to track and count scoring.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="offset"></param>
        internal void Count(int index, bool release = false, double? offset=null)
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

            Accuracy = 0;
            for (var i=0; i<6; i++)
            {
                Accuracy += (JudgePressSpread[i] + JudgeReleaseSpread[i]) * HitWeighting[i];
            }

            Accuracy /= (JudgeCount * 100);

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
            PlayScreen.GameplayUI.UpdateAccuracyBox(index);

            //Display stuff
            PlayScreen.Playfield.UpdateJudge(index, release, offset);

        }

        /// <summary>
        ///     Clear and Initialize Scoring related variables
        /// </summary>
        internal void Initialize(PlayScreenState playScreen)
        {
            PlayScreen = playScreen;
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
