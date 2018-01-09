using System;
using System.Collections.Generic;
using Quaver.Audio;
using Quaver.Config;
using Quaver.GameState.States;
using Quaver.Modifiers;
using Quaver.API.Maps;

namespace Quaver.GameState.Gameplay.PlayScreen
{
    /// <summary>
    /// This class deals with any timing and SV related calculations
    /// </summary>
    internal class Timing : IHelper
    {
        //Audio Variables
        internal bool SongIsPlaying { get; set; }

        //Gameplay Variables
        private double ActualSongTime { get; set; }
        internal float PlayingEndOffset { get; set; }
        private List<TimingObject> TimingQueue { get; set; }

        //SV + Timing Point Variables
        //private List<TimingObject> svQueue, TimingQueue, _barQueue, _activeBars;
        //private GameObject[] _activeBarObjects;

        //Audio File Variables
        internal bool SongIsDone { get; set; }
        internal bool PlayingIsDone { get; set; }
        private float _averageBpm { get; set; } = 100;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="qua"></param>
        public Timing(Qua qua)
        {
            TimingQueue = new List<TimingObject>();
            for (var i = 0; i < qua.TimingPoints.Count; i++)
            {
                TimingObject newTO = new TimingObject
                {
                    TargetTime = qua.TimingPoints[i].StartTime,
                    BPM = qua.TimingPoints[i].Bpm
                };
                TimingQueue.Add(newTO);
            }

            //Calculate Average BPM
            CalculateAverageBpm();
        }

        /// <summary>
        ///     Initialize Timing Contents.
        /// </summary>
        public void Initialize(IGameState state)
        {
            //TODO: Timing Initializer
            SongIsPlaying = false;

            //Declare Other Values
            ActualSongTime = -GameplayReferences.PlayStartDelayed * GameBase.GameClock;
            //_activeBarObjects = new GameObject[maxNoteCount];

            //Add offset after the last note
            PlayingEndOffset = GameBase.SelectedBeatmap.SongLength + (SongManager.BassDelayOffset - Configuration.GlobalOffset + 1500) * GameBase.GameClock;

            //Create Timing bars
            //_barQueue = new List<TimingObject>();
            //time_CreateBars();
        }

        /// <summary>
        ///     Unloads any objects to save memory
        /// </summary>
        public void UnloadContent()
        {
            TimingQueue.Clear();
        }

        /// <summary>
        ///     Set the position of the current play time
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            //Calculate Time after Song Done
            if (SongIsDone)
            {
                ActualSongTime += dt * GameBase.GameClock;

                //If song is done and song time is over playingEndOffset, the play session is done
                if (ActualSongTime >= PlayingEndOffset) PlayingIsDone = true;
            }

            //Calculate Actual Song Time if song is not done
            else
            {
                //If the audio didn't even start yet
                if (ActualSongTime < 0) ActualSongTime += dt * GameBase.GameClock;
                else
                {
                    //If song time > 0 and audio hasnt played yet
                    if (!SongIsPlaying)
                    {
                        SongIsPlaying = true;
                        SongManager.Play();
                    }

                    //If song time  > song end
                    if (SongManager.Position >= GameBase.SelectedBeatmap.SongLength || ActualSongTime >= PlayingEndOffset)
                        SongIsDone = true;
                    //Calculate song pos from audio
                    else
                        ActualSongTime = (SongManager.Position + (ActualSongTime + (dt * GameBase.GameClock))) / 2f;
                }
            }
        }

        internal double GeCurrentSongTime()
        {
            //Add global offset to actual song time
            return ActualSongTime + (SongManager.BassDelayOffset - Configuration.GlobalOffset) * GameBase.GameClock;
        }

        internal ulong[] GetSVCalc(List<TimingObject> svQueue)
        {
            //Calculates SV for efficiency
            var svCalc = new ulong[svQueue.Count];
            svCalc[0] = 0;
            ulong svPosTime = 0;
            for (var i = 0; i < svQueue.Count; i++)
            {
                if (i + 1 < svQueue.Count)
                {
                    svPosTime += (ulong)((svQueue[i + 1].TargetTime - svQueue[i].TargetTime) * svQueue[i].SvMultiplier);
                    svCalc[i + 1] = svPosTime;
                }
                else break;
            }

            return svCalc;
        }

        internal List<TimingObject> GetSVQueue(Qua qua)
        {
            //Create Timing Points + SVs on a list
            var svQueue = new List<TimingObject>();

            //Create SVs
            if (ModManager.Activated(ModIdentifier.NoSliderVelocity) == false)
            {
                //Todo: Implement SV change
                CreateSV(svQueue, 0, 1f);
                //foreach (var sv in qua.SliderVelocities)
                //    CreateSV(svQueue, sv.StartTime, sv.Multiplier);

                if (svQueue.Count >= 1)
                {
                    //ConvertTPtoSV(svQueue);
                    //NormalizeSVs(svQueue);
                }
                else CreateSV(svQueue, 0, 1f);
            }
            //If there's no SV, create a single SV Point
            else
            {
                CreateSV(svQueue, 0, 1f);
            }

            return svQueue;
        }

        //Creates SV Points
        private void CreateSV(List<TimingObject> svQueue, float targetTime, float multiplier, int? index = null)
        {
            TimingObject newTp = new TimingObject();
            newTp.TargetTime = targetTime;
            newTp.SvMultiplier = multiplier;
            if (index != null) svQueue.Insert((int)index, newTp);
            else svQueue.Add(newTp);
        }

        /// <summary>
        ///     Convert Timing Point to SV
        /// </summary>
        internal void ConvertTPtoSV(List<TimingObject> svQueue)
        {
            //Create and converts timing points to SV's
            int lastIndex = 0;
            int i = 0;
            int j = 0;
            List<TimingObject> bpmObjects = new List<TimingObject>();

            for (i = 0; i < TimingQueue.Count; i++)
            {
                for (j = 0; j < svQueue.Count; j++)
                {
                    if (TimingQueue[i].TargetTime > svQueue[j].TargetTime)
                    {
                        lastIndex = Math.Max(j - 1, 0);
                        break;
                    }
                }
                if (TimingQueue[i].TargetTime < svQueue[0].TargetTime - 1)
                    CreateSV(bpmObjects, TimingQueue[i].TargetTime, 1);

                else if (Math.Abs(TimingQueue[i].TargetTime - svQueue[lastIndex].TargetTime) > 1)
                    CreateSV(bpmObjects, TimingQueue[i].TargetTime, svQueue[lastIndex].SvMultiplier); //svQueue[lastIndex].SvMultiplier
            }
            foreach (var ob in bpmObjects) svQueue.Add(ob);
            svQueue.Sort((p1, p2) => p1.TargetTime.CompareTo(p2.TargetTime));
        }

        //Normalizes SV's in between each BPM change interval
        internal void NormalizeSVs(List<TimingObject> svQueue)
        {
            //Reference Variables + Sort
            var i = 0;
            var j = 0;
            var lastIndex = 0;

            //Normalize
            if (TimingQueue.Count == 0) return;

            for (i = 0; i < svQueue.Count; i++)
            {
                for (j = 0; j < TimingQueue.Count; j++)
                {
                    if (svQueue[i].TargetTime > TimingQueue[j].TargetTime - 1)
                    {
                        lastIndex = Math.Max(j - 1, 0);
                        break;
                    }
                }
                svQueue[i].SvMultiplier = svQueue[i].SvMultiplier * TimingQueue[lastIndex].BPM / _averageBpm;
            }
        }

        /// <summary>
        /// Calculate Average BPM of map
        /// </summary>
        internal void CalculateAverageBpm()
        {
            //TODO: Make this calculate consistancy based average bpm instead of timing point that is longest
            //AverageBpm Reference Variables
            double longestBpmTime = 0;
            var avergeBpmIndex = 0;
            var i = 0;

            //Calculate Average BPM
            if (TimingQueue.Count > 1)
            {
                for (i = 0; i < TimingQueue.Count; i++)
                {
                    if (i + 1 < TimingQueue.Count)
                    {
                        if (TimingQueue[i + 1].TargetTime - TimingQueue[i].TargetTime > longestBpmTime)
                        {
                            avergeBpmIndex = i;
                            longestBpmTime = TimingQueue[i + 1].TargetTime - TimingQueue[i].TargetTime;
                        }
                    }
                    else if (i + 1 == TimingQueue.Count)
                    {
                        if (GameBase.SelectedBeatmap.SongLength - TimingQueue[i].TargetTime > longestBpmTime)
                        {
                            avergeBpmIndex = i;
                            longestBpmTime = GameBase.SelectedBeatmap.SongLength - TimingQueue[i].TargetTime;
                        }
                    }
                }
                _averageBpm = TimingQueue[avergeBpmIndex].BPM;
            }
            else _averageBpm = TimingQueue[0].BPM;
        }

        public void Draw()
        {
            //do stuff
        }
    }
}
