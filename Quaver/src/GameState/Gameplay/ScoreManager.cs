using System;
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

        //Hit Tracking (ms deviance) and other data
        internal int TotalJudgeCount { get; set; }
        internal double SongLength { get; set; }
        internal List<NoteRecord> MsDevianceData { get; private set; }
        internal List<AccuracyRecord> AccuracyData { get; private set; }
        internal List<HealthRecord> HealthData { get; private set; }

        //Score tracking
        internal int Combo { get; set; }
        internal int ScoreTotal { get; set; }
        private int ScoreCount { get; set; }
        private int ScoreMax { get; set; }
        internal int MultiplierCount { get; set; }
        internal int MultiplierIndex { get; set; }
        internal int[] ScoreWeighting { get; } = new int[6] { 100, 50, 25, 10, 5, 0 };

        //Health tracking
        internal bool Failed { get; private set; }
        internal double Health { get; private set; }
        internal double[] HealthWeighting { get; } = new double[6] { 0.5, 0.4, 0.1, -2, -2.5, -3 };

        //Accuracy Reference Variables
        internal int[] HitWeighting { get; } = new int[6] { 100, 100, 50, -50, -100, 0 };
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
            JudgeCount++;
            if (release) JudgeReleaseSpread[index]++;
            else JudgePressSpread[index]++;

            //Add JudgeSpread to Accuracy
            Accuracy = 0;
            RelativeAcc = 0;
            RelativeAcc += (TotalJudgeCount - JudgeCount) * HitWeighting[5];
            for (var i=0; i<6; i++)
            {
                Accuracy += (JudgePressSpread[i] + JudgeReleaseSpread[i]) * HitWeighting[i];
                RelativeAcc += (JudgePressSpread[i] + JudgeReleaseSpread[i]) * HitWeighting[i];
            }

            //Average Accuracy
            Accuracy = Math.Max(Accuracy / (JudgeCount * 100), 0);
            RelativeAcc = Math.Max(RelativeAcc / (TotalJudgeCount * 100), -100);

            //Update Multiplier and Combo
            //If note is pressed properly, count combo and multplier
            if (index < 4)
            {
                //Update Multiplier
                if (index == 3)
                {
                    MultiplierCount -= 10;
                    if (MultiplierCount < 0) MultiplierCount = 0;
                }
                else
                {
                    if (MultiplierCount < 150) MultiplierCount++;
                    else MultiplierCount = 150; //idk... just to be safe
                }

                //Update Combo
                Combo++;
            }
            //If note is not pressed properly:
            else
            {
                //Update Multiplier
                MultiplierCount -= 20;
                if (MultiplierCount < 0) MultiplierCount = 0;

                //Update Combo
                Combo = 0;
            }

            //Update Multiplier index and score count
            MultiplierIndex = (int)Math.Floor(MultiplierCount/10f);
            ScoreCount += ScoreWeighting[index] + (MultiplierIndex * 10);

            //Update Health
            Health += HealthWeighting[index];
            if (Health <= 0)
            {
                Failed = true;
                Health = 0;
            }
            else if (Health > 100) Health = 100;

            //Record Data
            if (songpos != null)
            {
                //Note ms deviance data
                if (offset != null)
                {
                    NoteRecord noteData = new NoteRecord()
                    {
                        Offset = (double)offset / HitWindowPress[4],
                        Position = (double)songpos,
                        Type = index
                    };
                    MsDevianceData.Add(noteData);
                }

                //Acc Data
                AccuracyRecord accData = new AccuracyRecord()
                {
                    Accuracy = Accuracy,
                    Position = (double)songpos,
                    Type = GetAccGradeIndex(Accuracy) + 1
                };
                AccuracyData.Add(accData);

                //Health Data
                HealthRecord healthData = new HealthRecord()
                {
                    Health = Health / 100f,
                    Position = (double)songpos
                };
                HealthData.Add(healthData);
            }

            //Update Score todo: actual score calculation
            ScoreTotal = (int)(1000000 * ((float)ScoreCount / ScoreMax));
            //Logger.Log("Score Count: " + ScoreCount + "     Max: " + ScoreMax + "    Note: "+JudgeCount+"/"+ncount, LogColors.GameInfo);
        }

        /// <summary>
        ///     Clear and Initialize Scoring related variables
        /// </summary>
        /// <param name="Count"> Total amount of hitobjects + releases</param>
        internal void Initialize(int count)
        {
            Accuracy = 0;
            RelativeAcc = -200;
            Combo = 0;
            MultiplierCount = 0;
            MultiplierIndex = 0;
            ScoreTotal = 0;
            JudgeCount = 0;
            Health = 100;
            JudgeReleaseSpread = new int[6];
            JudgePressSpread = new int[6];
            MsDevianceData = new List<NoteRecord>();
            AccuracyData = new List<AccuracyRecord>();
            HealthData = new List<HealthRecord>();
            TotalJudgeCount = count;
            SongLength = SongManager.Length / GameBase.GameClock;

            //Create Difficulty Curve for od
            //var curve = (float)Math.Pow(od+1, -0.325) * GameBase.GameClock;
            //HitWindowPress = new float[5] { 20 * GameBase.GameClock, 88 * curve, 122 * curve, 148 * curve, 214 * curve };
            //HitWindowRelease = new float[4] { 30 * GameBase.GameClock, HitWindowPress[1]*1.35f, HitWindowPress[2] * 1.35f, HitWindowPress[3] * 1.35f };

            //Update Hit Window
            //This is similar to stepmania J4
            HitWindowPress = new float[5] { 18, 45, 80, 100, 200 };
            HitWindowRelease = new float[4] { HitWindowPress[0] * 1.5f, HitWindowPress[1] * 1.5f, HitWindowPress[2] * 1.5f, HitWindowPress[3] * 1.5f };

            for (int i = 0; i < 4; i++)
            {
                HitWindowPress[i] = HitWindowPress[i] * GameBase.GameClock;
                HitWindowRelease[i] = HitWindowRelease[i] * GameBase.GameClock;
            }
            HitWindowPress[4] = HitWindowPress[4] * GameBase.GameClock;

            // Calculate max score
            if (count < 150)
            {
                ScoreMax = 0;
                for (var i = 1; i < count+1; i++)
                    ScoreMax += 100 + 10*(int)Math.Floor(i / 10f);
            }
            else
                ScoreMax = 25650 + (count - 149) * 250;
        }

        /// <summary>
        ///     This is used to unload data after it has been read in score screen to save space.
        /// </summary>
        internal void UnloadData()
        {
            HealthData = null;
            AccuracyData = null;
            MsDevianceData = null;
        }

        /// <summary>
        ///     Convert RelativeAcc to display how far until next grade on graph scale.
        /// </summary>
        internal float GetRelativeAccScale()
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
        internal int GetAccGradeIndex(double acc = -1)
        {
            if (acc < 0) acc = RelativeAcc;

            var index = -1;
            for (var i = 0; i < 7; i++)
            {
                if (acc * 100 >= GradePercentage[i]) index = i;
                else break;
            }
            return index;
        }
    }
}
