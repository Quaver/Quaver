using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Audio;
using Quaver.Config;
using Quaver.GameState;
using Quaver.Logging;
using Quaver.Main;
using Quaver.Modifiers;

namespace Quaver.States.Gameplay.Mania.Components.Timing
{
    /// <summary>
    /// This class deals with any timing and SV related calculations
    /// </summary>
    internal class ManiaTiming : IGameStateComponent
    {
        //Audio Variables
        internal bool SongIsPlaying { get; set; }
        internal const int SONG_SKIP_OFFSET = 3000;
        internal const int SONG_END_OFFSET = 1500;

        //Gameplay Variables
        internal double ActualSongTime { get; set; }
        internal float PlayingEndOffset { get; set; }
        private List<ManiaTimingObject> TimingQueue { get; set; }

        //SV + ManiaTiming Point Variables
        //private List<ManiaTimingObject> svQueue, TimingQueue, _barQueue, _activeBars;
        //private GameObject[] _activeBarObjects;

        //Audio File Variables
        internal bool SongIsDone { get; set; }
        internal bool PlayingIsDone { get; set; }
        private float _averageBpm { get; set; } = 100;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="qua"></param>
        public ManiaTiming(Qua qua)
        {
            TimingQueue = new List<ManiaTimingObject>();
            for (var i = 0; i < qua.TimingPoints.Count; i++)
            {
                ManiaTimingObject newTO = new ManiaTimingObject
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
        ///     Initialize ManiaTiming Contents.
        /// </summary>
        public void Initialize(IGameState state)
        {
            //TODO: ManiaTiming Initializer
            SongIsPlaying = false;

            //Declare Other Values. 
            // Game starts 3 seconds before song
            ActualSongTime = -SONG_SKIP_OFFSET * GameBase.AudioEngine.PlaybackRate;
            //_activeBarObjects = new GameObject[maxNoteCount];

            //Add offset after the last note
            PlayingEndOffset = Qua.FindSongLength(GameBase.SelectedMap.Qua) + (AudioEngine.BassDelayOffset - ConfigManager.GlobalAudioOffset.Value + SONG_END_OFFSET) * GameBase.AudioEngine.PlaybackRate;

            //Create ManiaTiming bars
            //_barQueue = new List<ManiaTimingObject>();
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
                ActualSongTime += dt * GameBase.AudioEngine.PlaybackRate;

                //If song is done and song time is over playingEndOffset, the play session is done
                if (ActualSongTime >= PlayingEndOffset) PlayingIsDone = true;
                return;
            }

            //If the audio didn't even start yet, it will calculate actual song time with delta time.
            if (ActualSongTime < 0)
            {
                ActualSongTime += dt * GameBase.AudioEngine.PlaybackRate;
                return;
            }

            //Play audio if song time is >= 0
            if (!SongIsPlaying)
            {
                SongIsPlaying = true;

                try
                {
                    GameBase.AudioEngine.Play();
                }
                catch (AudioEngineException e)
                {
                    Logger.LogWarning("Audio file could not be played, as it probably doesn't exist!", LogType.Runtime);
                }
            }

            //If song time  > song end, song is done.
            if (GameBase.AudioEngine.Position + 1 > GameBase.AudioEngine.Length || ActualSongTime >= PlayingEndOffset)
            {
                SongIsDone = true;
            }

            //Calculate song pos from audio
            ActualSongTime = (GameBase.AudioEngine.Position + (ActualSongTime + (dt * GameBase.AudioEngine.PlaybackRate))) / 2;
        }

        internal double GetCurrentSongTime()
        {
            //Add global offset to actual song time
            return ActualSongTime + (AudioEngine.BassDelayOffset - ConfigManager.GlobalAudioOffset.Value) * GameBase.AudioEngine.PlaybackRate;
        }

        internal ulong[] GetSVCalc(List<ManiaTimingObject> svQueue)
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

        internal List<ManiaTimingObject> GetSVQueue(Qua qua)
        {
            //Create ManiaTiming Points + SVs on a list
            var svQueue = new List<ManiaTimingObject>();

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
        private void CreateSV(List<ManiaTimingObject> svQueue, float targetTime, float multiplier, int? index = null)
        {
            ManiaTimingObject newTp = new ManiaTimingObject();
            newTp.TargetTime = targetTime;
            newTp.SvMultiplier = multiplier;
            if (index != null) svQueue.Insert((int)index, newTp);
            else svQueue.Add(newTp);
        }

        /// <summary>
        ///     Convert ManiaTiming Point to SV
        /// </summary>
        internal void ConvertTPtoSV(List<ManiaTimingObject> svQueue)
        {
            //Create and converts timing points to SV's
            int lastIndex = 0;
            int i = 0;
            int j = 0;
            List<ManiaTimingObject> bpmObjects = new List<ManiaTimingObject>();

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
        internal void NormalizeSVs(List<ManiaTimingObject> svQueue)
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
                        if (GameBase.SelectedMap.SongLength - TimingQueue[i].TargetTime > longestBpmTime)
                        {
                            avergeBpmIndex = i;
                            longestBpmTime = GameBase.SelectedMap.SongLength - TimingQueue[i].TargetTime;
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
