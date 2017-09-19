
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Qua;

namespace Quaver.Difficulty
{
    public static class DifficultyCalculator
    {
        public static Difficulty CalculateDifficulty(List<HitObject> ObjectList)
        {
            //NPS Values
            float[] avgNps = new float[5]; //Pos 0 =total average
            float[] meanNps = new float[5];
            float[] npsConsistancy = new float[5];
            int[][] npsInterval = new int[5][];
            List<int>[] startTimes = new List<int>[4];
            List<float>[] jackStrain = new List<float>[4];
            List<float>[] handStrain = new List<float>[4];
            int npsSize = (int)(Mathf.Ceil(Mathf.Max((float)ObjectList[ObjectList.Count - 1].StartTime, (float)ObjectList[ObjectList.Count - 1].EndTime) / 1000f));
            int npsTolerance = 0;

            //Temp Referencing Values
            int curTime = 0;
            int notePos = 0;
            int i = 0;

            //Other Referencing Values
            float consistancy;
            float stamina; //High Consistancy * BPM
            float tech; //Inconsistant Keylane Nps
            float jack; //Consistant Keylane Nps
            float speed; //(NPS Keylane Mean + Nps Keylane Average) / 2
            float control; //(lnCount + ChordCount)/TotalHitObjects /AverageNPS
            int lnCount = 0;
            int chordCount = 0;

            //Sets NpsInterval Initial Size
            for (i = 0; i < 5; i++)
            {
                npsInterval[i] = new int[npsSize];
                if (i < 4)
                {
                    startTimes[i] = new List<int>();
                    jackStrain[i] = new List<float>();
                    handStrain[i] = new List<float>();
                }
            }

            //Calculates NPS + Sets npsInterval Array
            while (curTime < npsSize)
            {
                if (notePos < ObjectList.Count)
                {
                    if (curTime * 1000 < ObjectList[notePos].StartTime)
                    {
                        curTime++;
                    }
                    else
                    {
                        npsInterval[0][curTime] += 1;
                        npsInterval[ObjectList[notePos].KeyLane][curTime] += 1;
                        startTimes[ObjectList[notePos].KeyLane - 1].Add(ObjectList[notePos].StartTime);

                        //If LNs are larger than 220ms, adds to LN count
                        if (ObjectList[notePos].EndTime > ObjectList[notePos].StartTime + 220) lnCount++;

                        //If current note is a chord, adds to chord count
                        if (notePos - 1 >= 0)
                        {
                            if (ObjectList[notePos].StartTime == ObjectList[notePos - 1].StartTime)
                            {
                                chordCount++;
                            }
                        }
                        else if (notePos + 1 < npsSize)
                        {
                            if (ObjectList[notePos].StartTime == ObjectList[notePos + 1].StartTime)
                            {
                                chordCount++;
                            }
                        }
                    }
                }
                else
                {
                    break;
                }
                notePos++;
            }

            //Calculates Average NPS
            curTime = 0;
            for (i = 0; i < 5; i++)
            {
                curTime = 0;
                for (notePos = 0; notePos < npsSize; notePos++)
                {
                    if (npsInterval[i][notePos] > 0)
                    {
                        avgNps[i] += npsInterval[i][notePos];
                        curTime++;
                    }
                }
                avgNps[i] = avgNps[i] / curTime;
            }

            //Sets the NPS Tolerance (Any value below this will be ignored when calculating mean NPS.)
            npsTolerance = (int)(avgNps[0] / 5f);

            //Calculates Mean NPS of Keylane
            List<int> MeanNpsInterval;
            for (i = 0; i < 5; i++)
            {
                MeanNpsInterval = new List<int>();
                curTime = 0;
                for (notePos = 0; notePos < npsSize; notePos++)
                {
                    if (npsInterval[i][notePos] >= npsTolerance)
                    {
                        MeanNpsInterval.Add(npsInterval[i][notePos]);
                        curTime++;
                    }
                }
                MeanNpsInterval.Sort(delegate (int p1, int p2) { return p1.CompareTo(p2); });
                meanNps[i] = MeanNpsInterval[(int)(curTime / 2f)];
            }

            //Calculates NPS Consistancy
            float sd;
            for (i = 0; i < 5; i++)
            {
                sd = 0;
                curTime = 0;
                for (notePos = 0; notePos < npsSize; notePos++)
                {
                    if (npsInterval[i][notePos] >= npsTolerance)
                    {
                        //Normalize SD to 100NPS
                        sd += Mathf.Pow((npsInterval[i][notePos] - meanNps[i]) * (100f / Mathf.Max(avgNps[0], 1)), 2);
                        curTime++;
                    }
                }
                sd = Mathf.Sqrt(sd / curTime);
                npsConsistancy[i] = sd;
            }

            //Calculate Stamina Strain
            stamina = Mathf.Pow(1f / Mathf.Max(npsConsistancy[0], 0.1f), 1 / 10f) * Mathf.Pow(avgNps[0] / 10f, 0.94f) * 20f;

            //Calculate Jack Strain
            //jack = Mathf.Sqrt(Mathf.Pow(npsConsistancy[1]*npsConsistancy[2]*npsConsistancy[3]*npsConsistancy[4]/100f, 2) / 4f);
            //jack = 10f * Mathf.Pow(Mathf.Max(avgNps[0] * 10f / Mathf.Max(jack, 1f), 0.1f), 0.6f);

            //Calculate Speed Strain
            //speed = 6f*Mathf.Sqrt((avgNps[0]*1.2f + meanNps[0])/2f);

            //Calculate Tech Strain
            //tech = 15f * Mathf.Pow((npsConsistancy[1] + npsConsistancy[2] + npsConsistancy[3] + npsConsistancy[4]) / 40f,0.4f);
            //tech = 15f * Mathf.Pow(Mathf.Max(tech/100f,0.1f), 0.35f);

            //Calculate Control Strain
            //control = Mathf.Pow((npsConsistancy[1] + npsConsistancy[2] + npsConsistancy[3] + npsConsistancy[4]) / 200f , 0.75f) * Mathf.Pow((Mathf.Max(lnCount*1.2f+chordCount*0.8f,1)/ ObjectList.Count),0.55f) * Mathf.Pow(meanNps[0] / 10f, 0.7f) * 60f;

            //LN Strain * Chord Strain - consistancy

            //Declare Difficulty Values
            Difficulty newDifficulty = new Difficulty();

            newDifficulty.AverageNPS = avgNps[0];
            newDifficulty.npsInterval = npsInterval[0];

            //newDifficulty.StarDifficulty = (control+stamina)/2f;
            //newDifficulty.ControlStrain = control;
            newDifficulty.StaminaStrain = stamina;
            //newDifficulty.SpeedStrain = speed;
            //newDifficulty.TechStrain = tech;

            return newDifficulty;
        }
    }
}