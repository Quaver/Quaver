using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Gameplay;

namespace Quaver.States.Gameplay.Mania.Components.Scoring
{
    /// <summary>
    ///     THIS CLASS IS IMPORTANT. This is where all the scoring will be calculated.  
    /// </summary>
    internal class ManiaScoreManager
    {
        /// <summary>
        ///     TODO: Document this. Not sure why scroll speed is here?
        /// </summary>
        internal int ScrollSpeed { get; set; } = 0;

        /// <summary>
        ///     The total amount of pauses for this score
        /// </summary>
        internal int TotalPauses;

        /// <summary>
        ///     The number of presses for each judge
        /// </summary>
        internal int[] JudgePressSpread { get; private set; }

        /// <summary>
        ///     The number of releases for each judge
        /// </summary>
        internal int[] JudgeReleaseSpread { get; private set; }

        /// <summary>
        ///     TODO: Document this.
        /// </summary>
        internal int JudgeCount { get; set; }

        /// <summary>
        ///     TODO: Document this.
        /// </summary>
        internal int TotalJudgeCount { get; set; }

        /// <summary>
        ///     TODO: Document this.
        /// </summary>
        internal double PlayTimeTotal { get; set; }

        /// <summary>
        ///     TODO: Document this.
        /// </summary>
        internal List<ManiaGameplayData> NoteDevianceData { get; private set; }

        /// <summary>
        ///     TODO: Document this.
        /// </summary>
        internal List<ManiaGameplayData> AccuracyData { get; private set; }

        /// <summary>
        ///     TODO: Document this.
        /// </summary>
        internal List<ManiaGameplayData> HealthData { get; private set; }

        /// <summary>
        ///     The current combo during gameplay
        /// </summary>
        internal int Combo { get; set; }

        /// <summary>
        ///     The max combo the player has gotten
        /// </summary>
        internal int MaxCombo { get; set; }
        
        /// <summary>
        ///     TODO: Document this.
        /// </summary>
        internal int ScoreTotal { get; set; }

        /// <summary>
        ///     TODO: Document this.
        /// </summary>
        private int ScoreCount { get; set; }

        /// <summary>
        ///     TODO: Document this.
        /// </summary>
        private int ScoreMax { get; set; }

        /// <summary>
        ///     TODO: Document this.
        /// </summary>
        internal int MultiplierCount { get; set; }

        /// <summary>
        ///     TODO Document this.
        /// </summary>
        internal int MultiplierIndex { get; set; }

        /// <summary>
        ///     Keeps track of if this is a currently failed score
        /// </summary>
        internal bool Failed { get; private set; }

        /// <summary>
        ///     The current user's health
        /// </summary>
        internal double Health { get; private set; }

        /// <summary>
        ///     The current accuracy the user has.
        /// </summary>
        internal double Accuracy { get; set; }

        /// <summary>
        ///     TODO: Document this.
        /// </summary>
        internal double RelativeAcc { get; set; }

        /// <summary>
        ///     The weighting in accuracy each judge gives.
        ///     Note: The order in which they are defined is important, and it is from best to worst
        /// </summary>
        internal int[] HitWeighting { get; } = new int[6]
        {
            JudgeAccuracyWeighting.Marv,
            JudgeAccuracyWeighting.Perf,
            JudgeAccuracyWeighting.Great,
            JudgeAccuracyWeighting.Good,
            JudgeAccuracyWeighting.Okay,
            JudgeAccuracyWeighting.Miss
        };


        /// <summary>
        ///     The weighting in score each judge gives
        ///     Note: The order in which they are defined is important, and it is from best to worst
        /// </summary>
        internal int[] ScoreWeighting { get; } = new int[6]
        {
            JudgeScoreWeighting.Marv,
            JudgeScoreWeighting.Perf,
            JudgeScoreWeighting.Great,
            JudgeScoreWeighting.Good,
            JudgeScoreWeighting.Okay,
            JudgeScoreWeighting.Miss
        };

        /// <summary>
        ///     The weighting in health each judge gives
        ///     Note: The order in which they are defined is important, and it is from best to worst
        /// </summary>
        internal double[] HealthWeighting { get; } = new double[6]
        {
            JudgeHealthWeighting.Marv,
            JudgeHealthWeighting.Perf,
            JudgeHealthWeighting.Great,
            JudgeHealthWeighting.Good,
            JudgeHealthWeighting.Okay,
            JudgeHealthWeighting.Miss
        };

        /// <summary>
        ///     The HitWindows for Gameplay when pressing
        ///     Note: The order in which they are defined is important, and it is from best to worst
        /// </summary>
        internal float[] HitWindowPress { get; } = { JudgeWindow.Marv, JudgeWindow.Perf, JudgeWindow.Great, JudgeWindow.Good, JudgeWindow.Okay };

        /// <summary>
        ///     The HinWindows for gameplay when release keys for LNs
        ///     Note: The order in which they are defined is important, and it is from best to worst
        /// </summary>
        internal float[] HitWindowRelease { get; } =
        {
            JudgeWindow.Marv * 1.5f,
            JudgeWindow.Perf * 1.7f,
            JudgeWindow.Great * 1.8f,
            JudgeWindow.Good * 2.0f,
            JudgeWindow.Okay 
        };

        /// <summary>
        ///     The percentages for grades
        ///     Note: The order in which they are defined is important, and it is from best to worst
        /// </summary>
        internal int[] GradePercentage { get; } = new int[]
        {
            GradePercentages.D,
            GradePercentages.C,
            GradePercentages.B,
            GradePercentages.A,
            GradePercentages.S,
            GradePercentages.SS,
            GradePercentages.X, 
            GradePercentages.XX
        };

        /// <summary>
        ///     Textures for the grades
        /// </summary>
        internal Texture2D[] GradeImage { get; } = new Texture2D []
        {
            GameBase.LoadedSkin.GradeSmallF,
            GameBase.LoadedSkin.GradeSmallD,
            GameBase.LoadedSkin.GradeSmallC,
            GameBase.LoadedSkin.GradeSmallB,
            GameBase.LoadedSkin.GradeSmallA,
            GameBase.LoadedSkin.GradeSmallS,
            GameBase.LoadedSkin.GradeSmallSS,
            GameBase.LoadedSkin.GradeSmallX,
            GameBase.LoadedSkin.GradeSmallXX          
        };

        /// <summary>
        ///     This method is used to track and count scoring.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="offset"></param>
        internal void Count(int index, bool release = false, double? offset = null, double? songPos = null)
        {
            //Update Judge Spread
            JudgeCount++;
            if (release)
                JudgeReleaseSpread[index]++;
            else
                JudgePressSpread[index]++;

            //Add JudgeSpread to Accuracy
            Accuracy = 0;
            RelativeAcc = 0;
            RelativeAcc += (TotalJudgeCount - JudgeCount) * HitWeighting[5];
            for (var i=0; i<6; i++)
            {
                Accuracy += (JudgePressSpread[i] + JudgeReleaseSpread[i]) * HitWeighting[i];
                RelativeAcc += (JudgePressSpread[i] + JudgeReleaseSpread[i]) * HitWeighting[i];
            }

            //Update Average Accuracy
            Accuracy = Math.Max(Accuracy / (JudgeCount * 100), 0);
            RelativeAcc = Math.Max(RelativeAcc / (TotalJudgeCount * 100), -100);

            //If note is not pressed properly, update combo and multplier
            if (index < 5)
            {
                //Update Multiplier
                if (index == 3)
                {
                    MultiplierCount -= 10;
                    if (MultiplierCount < 0)
                        MultiplierCount = 0;
                }
                else
                {
                    if (MultiplierCount < 150)
                        MultiplierCount++;
                    else
                        MultiplierCount = 150; //idk... just to be safe
                }

                //Update Combo
                Combo++;

                if (Combo > MaxCombo)
                    MaxCombo = Combo;
            }
            //If player combo breaks, reset combo and punish multiplier
            else
            {
                //Update Multiplier
                MultiplierCount -= 20;
                if (MultiplierCount < 0) MultiplierCount = 0;

                //Update Combo
                Combo = 0;
            }

            //Update Health
            Health += HealthWeighting[index];
            if (Health <= 0)
            {
                Failed = true;
                Health = 0;
            }
            else if (Health > 100)
                Health = 100;

            //Update Multiplier index and score count
            MultiplierIndex = (int)Math.Floor(MultiplierCount/10f);
            ScoreCount += ScoreWeighting[index] + (MultiplierIndex * 10);

            //Record Data
            if (songPos != null)
            {
                //Note ms deviance data
                if (offset != null)
                NoteDevianceData.Add(new ManiaGameplayData()
                {
                    Value = index != 5 ? (double)offset : HitWindowPress[4] + 1,
                    Position = (double)songPos
                });

                //Acc Data
                AccuracyData.Add(new ManiaGameplayData()
                {
                    Value = Accuracy,
                    Position = (double)songPos
                });

                //Health Data
                HealthData.Add(new ManiaGameplayData()
                {
                    Value = Health / 100,
                    Position = (double)songPos
                });
            }

            //Update Score todo: actual score calculation
            ScoreTotal = (int)(1000000 * ((double)ScoreCount / ScoreMax));
        }

        /// <summary>
        ///     Clear and Initialize Scoring related variables
        /// </summary>
        /// <param name="Count"> Total amount of hitobjects + releases</param>
        /// <param name="count"></param>
        internal void Initialize(int count)
        {
            RelativeAcc = -200;
            Health = 100;
            JudgeReleaseSpread = new int[6];
            JudgePressSpread = new int[6];
            NoteDevianceData = new List<ManiaGameplayData>();
            AccuracyData = new List<ManiaGameplayData>();
            HealthData = new List<ManiaGameplayData>();
            TotalJudgeCount = count;

            //Create first value of data at point 0
            //Acc Data
            var accData = new ManiaGameplayData()
            {
                Value = 1,
                Position = 0
            };
            AccuracyData.Add(accData);

            //Health Data
            var healthData = new ManiaGameplayData()
            {
                Value = 1,
                Position = 0
            };
            HealthData.Add(healthData);


            for (int i = 0; i < 4; i++)
            {
                HitWindowPress[i] = HitWindowPress[i] * GameBase.AudioEngine.PlaybackRate;
                HitWindowRelease[i] = HitWindowRelease[i] * GameBase.AudioEngine.PlaybackRate;
            }
            HitWindowPress[4] = HitWindowPress[4] * GameBase.AudioEngine.PlaybackRate;

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
            NoteDevianceData = null;
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
