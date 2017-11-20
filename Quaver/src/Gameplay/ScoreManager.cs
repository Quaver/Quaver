﻿using System;
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
        //Hit Timing Variables
        internal string[] JudgeNames { get; } = new string[6] { "MARV", "PERF", "GREAT", "GOOD", "OKAY", "MISS" };

        //Hit Tracking (Judging/Scoring)
        internal int[] JudgePressSpread { get; set; }
        internal int[] JudgeReleaseSpread { get; set; }
        internal int JudgeCount { get; set; }

        //Hit Tracking (ms deviance)
        internal int TotalJudgeCount { get; set; }
        internal List<double> MsDeviance { get; set; }

        //Score tracking
        internal int ConsistancyMultiplier { get; set; }
        internal int Combo { get; set; }
        internal int Score { get; set; }

        //Accuracy Variables
        internal int[] HitWeighting { get; } = new int[6] { 100, 100, 50, 25, -75, -100 };
        internal float[] HitWindow { get; } = new float[5] { 20, 44, 76, 106, 130 };
        internal int[] Grade { get; } = new int[8] { 60, 70, 80, 90, 95, 99, 100, 100 };

        //Accuracy Scoring
        internal double Accuracy { get; set; }
        internal double RelativeAcc { get; set; }

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

            //Add JudgeSpread to Accuracy
            Accuracy = 0;
            RelativeAcc = 0;
            for (var i=0; i<6; i++)
            {
                Accuracy += (JudgePressSpread[i] + JudgeReleaseSpread[i]) * HitWeighting[i];
                if (i < 5) RelativeAcc += (JudgePressSpread[i] + JudgeReleaseSpread[i]) * HitWeighting[i];
            }
            RelativeAcc += (TotalJudgeCount - JudgeCount) * HitWeighting[5];

            //Average Accuracy
            Accuracy = Math.Max(Accuracy / (JudgeCount * 100), 0);
            RelativeAcc = Math.Max(RelativeAcc / (TotalJudgeCount * 100), -100);

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
        }

        /// <summary>
        ///     Clear and Initialize Scoring related variables
        /// </summary>
        /// <param name="Count"> Total amount of hitobjects + releases</param>
        internal void Initialize(int Count)
        {
            Accuracy = 0;
            RelativeAcc = 0;
            ConsistancyMultiplier = 0;
            Combo = 0;
            Score = 0;
            JudgeCount = 0;
            JudgeReleaseSpread = new int[6];
            JudgePressSpread = new int[6];
            MsDeviance = new List<double>();
            TotalJudgeCount = Count;
        }

        /// <summary>
        ///     Convert RelativeAcc to display how far until next grade on graph scale.
        /// </summary>
        internal float RelativeAccGetScale()
        {
            var index = GetRelativeAccIndex();

            //Console.WriteLine(RelativeAcc*100);
            if (index > 0)
            {
                //Console.WriteLine(index +": "+(((float)(100 * RelativeAcc) - Grade[index]) / (float)(Grade[index] - Grade[index - 1])));
                return ((float)(100 * RelativeAcc) - Grade[index])/(float)(Grade[index] - Grade[index - 1]);
            }
            else if (index == 0)
            {
                //Console.WriteLine(index + ": " + (((float)(100 * RelativeAcc) - Grade[0]) / (float)(Grade[1] - Grade[0])));
                return ((float)(100 * RelativeAcc) - Grade[0]) / (float)(Grade[1] - Grade[0]);
            }
            else
            {
                //Console.WriteLine("0: "+((float)(100 * Math.Max(RelativeAcc, 0)) / (float)Grade[0]));
                return (float)(100 * Math.Max(RelativeAcc, 0))/(float)Grade[0];
            }
        }

        /// <summary>
        ///     Get the index for the relative acc.
        /// </summary>
        /// <returns></returns>
        internal int GetRelativeAccIndex()
        {
            var index = -1;
            for (var i = 0; i < 8; i++)
            {
                if (RelativeAcc * 100 >= Grade[i]) index = i;
                else break;
            }
            return index;
        }
    }
}
