﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.GameState.States;
using Quaver.Graphics;
using Quaver.Logging;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using Quaver.Audio;

namespace Quaver.GameState.Gameplay
{
    /// <summary>
    /// THIS CLASS IS IMPORTANT. This is where all the scoring will be calculated.
    /// This class will be updated in the future in such a way that it is near impossible to be manipulated with.
    /// </summary>
    internal class ScoreManager
    {
        //todo: document this crap
        //Hit Timing Variables
        internal float JudgeDifficulty { get; set; } = 10;

        //Hit Tracking (Judging/Scoring)
        internal int[] JudgePressSpread { get; set; }
        internal int[] JudgeReleaseSpread { get; set; }
        internal int JudgeCount { get; set; }

        //Hit Tracking (ms deviance)
        internal int TotalJudgeCount { get; set; }
        internal List<NoteRecord> MsDeviance { get; set; }
        internal double SongLength { get; set; }

        //Score tracking
        internal int Combo { get; set; }
        internal int ScoreTotal { get; set; }
        private int ScoreCount { get; set; }
        private int ScoreMax { get; set; }
        internal int MultiplierCount { get; set; }
        internal int MultiplierIndex { get; set; }
        internal int[] ScoreWeighting { get; } = new int[6] { 100, 100, 100, 100, 100, 100 };

        //Accuracy Reference Variables
        internal int[] HitWeighting { get; } = new int[6] { 100, 100, 50, 25, -75, -100 };
        internal float[] HitWindowPress { get; private set; }
        internal float[] HitWindowRelease { get; private set; }
        internal int[] GradePercentage { get; } = new int[8] { 60, 70, 80, 90, 95, 99, 100, 100 };
        internal Texture2D[] GradeImage { get; } = new Texture2D [9]{
            GameBase.LoadedSkin.GradeSmallF,
            GameBase.LoadedSkin.GradeSmallD,
            GameBase.LoadedSkin.GradeSmallC,
            GameBase.LoadedSkin.GradeSmallB,
            GameBase.LoadedSkin.GradeSmallA,
            GameBase.LoadedSkin.GradeSmallS,
            GameBase.LoadedSkin.GradeSmallSS,
            GameBase.LoadedSkin.GradeSmallX,
            GameBase.LoadedSkin.GradeSmallXX};

        //Accuracy Scoring
        internal double Accuracy { get; set; }
        internal double RelativeAcc { get; set; }

        /// <summary>
        /// This method is used to track and count scoring.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="offset"></param>
        internal void Count(int index, bool release = false, double? offset = null, double? songpos = null)
        {
            //Update Judge Spread
            if (release) JudgeReleaseSpread[index]++;
            else JudgePressSpread[index]++;
            JudgeCount++;

            //record ms deviance data
            if (offset != null && songpos != null)
            {
                NoteRecord noteData = new NoteRecord()
                {
                    Offset = (double)offset/HitWindowPress[4],
                    Position = (double)songpos,
                    Type = index
                };
                MsDeviance.Add(noteData);
            }

            //Add JudgeSpread to Accuracy
            Accuracy = 0;
            RelativeAcc = 0;
            for (var i=0; i<6; i++)
            {
                Accuracy += (JudgePressSpread[i] + JudgeReleaseSpread[i]) * HitWeighting[i];
                RelativeAcc += (JudgePressSpread[i] + JudgeReleaseSpread[i]) * HitWeighting[i];
            }
            RelativeAcc += (TotalJudgeCount - JudgeCount) * HitWeighting[5];

            //Average Accuracy
            Accuracy = Math.Max(Accuracy / (JudgeCount * 100), 0);
            RelativeAcc = Math.Max(RelativeAcc / (TotalJudgeCount * 100), -100);

            //Update Multiplier and Combo
            //If note is pressed properly, count combo and multplier
            if (index < 4)
            {
                Combo++;
                if (MultiplierCount < 150) MultiplierCount++;
                else MultiplierCount = 150; //idk... just to be safe
            }
            //If note is not pressed properly:
            else
            {
                //Update Multiplier
                //MultiplierCount -= 10;

                //todo: temp
                if (MultiplierCount < 150) MultiplierCount++;
                else MultiplierCount = 150; //idk... just to be safe

                if (MultiplierCount < 0) MultiplierCount = 0;

                //Update Combo
                Combo = 0;
            }

            //Update Multiplier index and score count
            MultiplierIndex = (int)Math.Floor(MultiplierCount/10f);
            ScoreCount += ScoreWeighting[index] + MultiplierIndex;

            //Update Score todo: actual score calculation
            ScoreTotal = (int)(1000000 * ((float)ScoreCount / ScoreMax));
            //Console.WriteLine("Score Count: " + ScoreCount + "     Max: " + ScoreMax + "    Note: "+JudgeCount+"/"+ncount);
        }

        private int ncount = 0; //todo: remove

        /// <summary>
        ///     Clear and Initialize Scoring related variables
        /// </summary>
        /// <param name="Count"> Total amount of hitobjects + releases</param>
        internal void Initialize(int count, float od)
        {
            ncount = count; //todo: remove

            Accuracy = 0;
            RelativeAcc = -200;
            Combo = 0;
            MultiplierCount = 0;
            MultiplierIndex = 0;
            ScoreTotal = 0;
            JudgeCount = 0;
            JudgeReleaseSpread = new int[6];
            JudgePressSpread = new int[6];
            MsDeviance = new List<NoteRecord>();
            TotalJudgeCount = count;
            JudgeDifficulty = od;
            SongLength = SongManager.Length / GameBase.GameClock;

            //Create Difficulty Curve for od
            var curve = (float)Math.Pow(od+1, -0.325) * GameBase.GameClock;
            HitWindowPress = new float[5] { 20 * GameBase.GameClock, 88 * curve, 122 * curve, 148 * curve, 214 * curve };
            HitWindowRelease = new float[4] { 30 * GameBase.GameClock, HitWindowPress[1]*1.35f, HitWindowPress[2] * 1.35f, HitWindowPress[3] * 1.35f };

            //count max score
            ScoreMax = 0;
            if (count < 150)
            {
                for (var i = 1; i < count; i++)
                    ScoreMax += 100 + (int)Math.Floor(i / 10f);
            }
            else
                ScoreMax = 16065 + (count - 150) * 115;

            Console.WriteLine("Max Score: " + ScoreMax);
        }

        /// <summary>
        ///     Convert RelativeAcc to display how far until next grade on graph scale.
        /// </summary>
        internal float RelativeAccGetScale()
        {
            var index = GetAccGradeIndex();

            //Console.WriteLine(RelativeAcc*100);
            if (index > 0)
            {
                //Console.WriteLine(index +": "+(((float)(100 * RelativeAcc) - Grade[index]) / (float)(Grade[index] - Grade[index - 1])));
                return ((float)(100 * RelativeAcc) - GradePercentage[index]) / (GradePercentage[index] - GradePercentage[index - 1]);
            }
            else if (index == 0)
            {
                //Console.WriteLine(index + ": " + (((float)(100 * RelativeAcc) - Grade[0]) / (float)(Grade[1] - Grade[0])));
                return ((float)(100 * RelativeAcc) - GradePercentage[0]) / (GradePercentage[1] - GradePercentage[0]);
            }
            else
            {
                //Console.WriteLine("0: "+((float)(100 * Math.Max(RelativeAcc, 0)) / (float)Grade[0]));
                return (float)(100 * Math.Max(RelativeAcc+1, 0)) / (GradePercentage[0] + 100);
            }
        }

        /// <summary>
        ///     Get the index for the relative acc.
        /// </summary>
        /// <returns></returns>
        internal int GetAccGradeIndex()
        {
            var index = -1;
            for (var i = 0; i < 8; i++)
            {
                if (RelativeAcc * 100 >= GradePercentage[i]) index = i;
                else break;
            }
            return index;
        }
    }
}
