using System;
using System.Collections.Generic;
using Quaver.Audio;
using Quaver.Config;
using Quaver.GameState.States;
using Quaver.Modifiers;
using Quaver.Maps;

namespace Quaver.GameState.Gameplay.PlayScreen
{
    /// <summary>
    /// This class deals with any timing and SV related calculations
    /// </summary>
    internal class Timing : IHelper
    {
        //Gameplay Constants
        internal const int PlayStartDelayed = 3000; //How long to pause the audio before playing. Max is 10000ms.

        //Audio Variables

        internal bool SongIsPlaying { get; set; }

        //Gameplay Variables
        private double ActualSongTime { get; set; }
        internal List<TimingObject> SvQueue { get; set; } //todo: remove
        private List<TimingObject> TimingQueue { get; set; }
        internal float PlayingEndOffset { get; set; }

        //SV + Timing Point Variables
        //private List<TimingObject> SvQueue, TimingQueue, _barQueue, _activeBars;
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
            //Create Timing Points + SVs on a list
            SvQueue = new List<TimingObject>();
            for (var i = 0; i < qua.SliderVelocities.Count; i++)
            {
                CreateSV(qua.SliderVelocities[i].StartTime, qua.SliderVelocities[i].Multiplier);
            }

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

            //Create SVs
            if (ModManager.Activated(ModIdentifier.NoSliderVelocity) == false && SvQueue.Count > 1)
            {
                ConvertTPtoSV();
                NormalizeSVs();
            }
            //If there's no SV, create a single SV Point
            else
            {
                CreateSV(0, 1f);
            }

            //Calculates SV for efficiency
            GameplayReferences.SvCalc = new ulong[SvQueue.Count];
            GameplayReferences.SvCalc[0] = 0;
            ulong svPosTime = 0;
            for (var i = 0; i < SvQueue.Count; i++)
            {
                if (i + 1 < SvQueue.Count)
                {
                    svPosTime += (ulong)((SvQueue[i + 1].TargetTime - SvQueue[i].TargetTime) * SvQueue[i].SvMultiplier);
                    GameplayReferences.SvCalc[i + 1] = svPosTime;
                }
                else break;
            }

            GameplayReferences.SvQueue = SvQueue; //todo: implement properly
        }

        /// <summary>
        ///     Initialize Timing Contents.
        /// </summary>
        public void Initialize(IGameState state)
        {
            //TODO: Timing Initializer
            SongIsPlaying = false;

            //Declare Other Values
            ActualSongTime = -PlayStartDelayed;
            //_activeBarObjects = new GameObject[maxNoteCount];

            //Add offset after the last note
            PlayingEndOffset = GameBase.SelectedBeatmap.SongLength + 1500 * GameBase.GameClock;

            //Create Timing bars
            //_barQueue = new List<TimingObject>();
            //time_CreateBars();
        }

        /// <summary>
        ///     Unloads any objects to save memory
        /// </summary>
        public void UnloadContent()
        {
            SvQueue.Clear();
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
                    if (SongManager.Position >= GameBase.SelectedBeatmap.SongLength)
                        SongIsDone = true;
                    //Calculate song pos from audio
                    else
                        ActualSongTime = (SongManager.Position + (ActualSongTime + dt * GameBase.GameClock)) / 2f;
                }
            }
        }

        public double GeCurrentSongTime()
        {
            //Add global offset to actual song time
            return ActualSongTime - Configuration.GlobalOffset + SongManager.BassDelayOffset;
        }

        public void Draw()
        {
            //do stuff
        }

        internal void time_DestroyBars()
        {
            //for (int i = 0; i < _activeBars.Count; i++) Destroy(_activeBars[i].TimingBar);
        }

        //Creates SV Points
        internal void CreateSV(float targetTime, float multiplier, int? index = null)
        {
            TimingObject newTp = new TimingObject();
            newTp.TargetTime = targetTime;
            newTp.SvMultiplier = multiplier;
            if (index != null) SvQueue.Insert((int)index, newTp);
            else SvQueue.Add(newTp);
        }

        /// <summary>
        ///     Convert Timing Point to SV
        /// </summary>
        internal void ConvertTPtoSV()
        {
            //Create and converts timing points to SV's
            var lastIndex = 0;
            foreach (TimingObject timeObject in TimingQueue)
            {
                if (timeObject.TargetTime < SvQueue[0].TargetTime)
                {
                    if (Math.Abs(timeObject.BPM- _averageBpm) <= 0.02)
                        CreateSV(timeObject.TargetTime, 1, 0);
                    else
                        CreateSV(timeObject.TargetTime, SvQueue[0].SvMultiplier, 0);
                }
                else if (timeObject.TargetTime > SvQueue[SvQueue.Count - 1].TargetTime)
                    CreateSV(timeObject.TargetTime, SvQueue[SvQueue.Count - 1].SvMultiplier);
                else
                {
                    for (var i = lastIndex; i < SvQueue.Count; i++)
                    {
                        if (i + 1 >= SvQueue.Count) //|| !(timeObject.TargetTime < SvQueue[i + 1].TargetTime))
                            continue;
                        if (Math.Abs(timeObject.TargetTime - SvQueue[i].TargetTime) > 1f)
                        {
                            CreateSV(timeObject.TargetTime, 1f, lastIndex);
                            lastIndex = i+1;
                        }
                        break;
                    }
                }
            }
            SvQueue.Sort((p1, p2) => p1.TargetTime.CompareTo(p2.TargetTime));
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

        //Normalizes SV's in between each BPM change interval
        internal void NormalizeSVs()
        {
            //Reference Variables + Sort
            var i = 0;
            var j = 0;
            var lastIndex = 0;

            //Normalize
            if (TimingQueue.Count >= 1)
            {
                for (i = 0; i < SvQueue.Count; i++)
                {
                    for (j = lastIndex; j < TimingQueue.Count; j++)
                    {
                        if (j + 1 < TimingQueue.Count)
                        {
                            if (SvQueue[i].TargetTime < TimingQueue[lastIndex + 1].TargetTime)
                            {
                                SvQueue[i].SvMultiplier =
                                    Math.Min(SvQueue[i].SvMultiplier * TimingQueue[lastIndex].BPM / _averageBpm, 128f);
                            }
                            else
                            {
                                SvQueue[i].SvMultiplier =
                                    Math.Min(SvQueue[i].SvMultiplier * TimingQueue[lastIndex].BPM / _averageBpm, 128f);
                                lastIndex = j;
                            }
                            break;
                        }
                        SvQueue[i].SvMultiplier =
                            Math.Min(SvQueue[i].SvMultiplier * TimingQueue[lastIndex].BPM / _averageBpm, 128f);
                    }
                }
            }
        }

        //Move Timing Bars
        internal void MoveTimingBars()
        {
            /*
            if (_config_timingBars && !mod_split)
            {
                if (_activeBars.Count > 0 && CurrentSongTime > _activeBars[0].StartTime + missTime)
                {
                    time_RemoveBar(_activeBars[0].TimingBar);
                    _activeBars.RemoveAt(0);
                }
                else
                {
                    for (int k = 0; k < _activeBars.Count; k++)
                    {
                        _activeBars[k].TimingBar.transform.localPosition = new Vector3(0f, PosFromSV(_activeBars[k].StartTime), 2f);
                    }
                }
            }
            */
        }
    }
}
