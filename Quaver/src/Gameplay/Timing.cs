﻿using System;
using System.Collections.Generic;
using Quaver.Config;

using Quaver.Modifiers;
using Quaver.QuaFile;

namespace Quaver.Gameplay
{
    /// <summary>
    /// This class deals with any timing and SV related calculations
    /// </summary>
    internal static class Timing
    {
        //Gameplay Constants
        internal const int PlayStartDelayed = 3000; //How long to pause the audio before playing. Max is 10000ms.

        //Audio Variables
        private static double GameAudioLength { get; set; }
        private static double SongEndOffset { get; set; }
        private static bool SongIsPlaying { get; set; }

        //Gameplay Variables
        private static double ActualSongTime { get; set; }
        internal static double CurrentSongTime { get; set; }
        internal static List<TimingObject> SvQueue { get; set; }
        private static List<TimingObject> TimingQueue { get; set; }

        //SV + Timing Point Variables
        //private List<TimingObject> SvQueue, TimingQueue, _barQueue, _activeBars;
        //private GameObject[] _activeBarObjects;

        //Audio File Variables
        private static bool _songIsDone { get; set; }
        private static float _averageBpm { get; set; } = 100;

        /// <summary>
        ///     Initialize Timing Contents.
        /// </summary>
        internal static void Initialize(Qua Qua)
        {
            //TODO: Timing Initializer
            GameAudioLength = GameBase.SelectedBeatmap.Song.GetAudioLength();
            SongEndOffset = 0;
            SongIsPlaying = false;

            //Reference Variables
            var i = 0;

            //Declare Other Values
            CurrentSongTime = -PlayStartDelayed;
            ActualSongTime = -PlayStartDelayed;
            //_activeBarObjects = new GameObject[maxNoteCount];

            //Create Timing Points + SVs on a list

            SvQueue = new List<TimingObject>();
            
            for (i = 0; i < Qua.SliderVelocities.Count; i++)
            {
                CreateSV(Qua.SliderVelocities[i].StartTime, Qua.SliderVelocities[i].Multiplier);
            }

            TimingQueue = new List<TimingObject>();
            for (i = 0; i < Qua.TimingPoints.Count; i++)
            {
                TimingObject newTO = new TimingObject
                {
                    TargetTime = Qua.TimingPoints[i].StartTime,
                    BPM = Qua.TimingPoints[i].Bpm
                };
                TimingQueue.Add(newTO);
            }

            //Calculate Average BPM
            CalculateAverageBpm();

            //Create SVs
            if (ModManager.Activated(ModIdentifier.NoSliderVelocity) == false && SvQueue.Count > 1)
            {
                ConvertTPtoSV();
                //NormalizeSVs();
            }
            //If there's no SV, create a single SV Point
            else
            {
                CreateSV(0,1f);
            }

            //Calculates SV for efficiency
            NoteRendering.SvCalc = new ulong[SvQueue.Count];
            NoteRendering.SvCalc[0] = 0;
            ulong svPosTime = 0;
            for (i = 0; i < SvQueue.Count; i++)
            {
                if (i + 1 < SvQueue.Count)
                {
                    svPosTime += (ulong)((SvQueue[i + 1].TargetTime - SvQueue[i].TargetTime) * SvQueue[i].SvMultiplier);
                    NoteRendering.SvCalc[i + 1] = svPosTime;
                }
                else break;
            }

            //Create Timing bars
            //_barQueue = new List<TimingObject>();
            //time_CreateBars();
        }

        /// <summary>
        ///     Unloads any objects to save memory
        /// </summary>
        internal static void UnloadContent()
        {
            SvQueue.Clear();
            TimingQueue.Clear();
        }

        /// <summary>
        ///     Set the position of the current play time
        /// </summary>
        /// <param name="dt"></param>
        internal static void Update(double dt)
        {
            //Calculate Time after Song Done
            if (_songIsDone)
            {
                SongEndOffset += dt;
                ActualSongTime = GameAudioLength + SongEndOffset;
            }
            //Calculate Actual Song Time
            else
            {
                if (ActualSongTime < 0) ActualSongTime += dt;
                else
                {
                    if (!SongIsPlaying)
                    {
                        SongIsPlaying = true;
                        GameBase.SelectedBeatmap.Song.Play(0, GameBase.GameClock);
                    }
                    ActualSongTime = (GameBase.SelectedBeatmap.Song.GetAudioPosition() + (ActualSongTime + dt)) / 2f;
                }
            }
            CurrentSongTime = ActualSongTime - Configuration.GlobalOffset;
        }

        internal static void time_DestroyBars()
        {
            //for (int i = 0; i < _activeBars.Count; i++) Destroy(_activeBars[i].TimingBar);
        }

        //Creates timing bars (used to measure 16 beats)
        internal static void time_CreateBars()
        {
            /*
            int i = 0;
            if (!mod_split && _config_timingBars)
            {
                float curBarTime = 0;
                for (i = 0; i < TimingQueue.Count; i++)
                {
                    curBarTime = TimingQueue[i].StartTime;

                    if (_barQueue.Count > 0 && _barQueue[0].StartTime + 2 > curBarTime) _barQueue.RemoveAt(0);
                    curBarTime += 1000f * 4f * 60f / (TimingQueue[i].BPM);
                    TimingObject curTiming;

                    if (i + 1 < TimingQueue.Count)
                    {
                        while (curBarTime < TimingQueue[i + 1].StartTime)
                        {
                            curTiming = new TimingObject();
                            curTiming.StartTime = (int)(curBarTime);
                            _barQueue.Add(curTiming);
                            curBarTime += 1000f * 4f * 60f / (TimingQueue[i].BPM);
                        }
                    }
                    else
                    {
                        while (curBarTime < _songAudio.clip.length * 1000f)
                        {
                            curTiming = new TimingObject();
                            curTiming.StartTime = (int)(curBarTime);
                            _barQueue.Add(curTiming);
                            curBarTime += 1000f * 4f * 60f / (TimingQueue[i].BPM);
                        }
                    }
                }

                //Create all bars in music
                List<TimingObject> tempBars = new List<TimingObject>();
                for (i = 0; i < _barQueue.Count; i++)
                {
                    TimingObject hoo = new TimingObject();

                    hoo.StartTime = _barQueue[i].StartTime;
                    tempBars.Add(hoo);
                }
                _barQueue = new List<TimingObject>(tempBars);

                //Create starting bars
                _activeBars = new List<TimingObject>();
                for (i = 0; i < maxNoteCount; i++)
                {
                    if (_barQueue.Count > 0) _activeBarObjects[i] = time_InstantiateBar(null);
                    else break;
                }

            }
            */
        }

        //Creates a bar object
        /*
        private GameObject time_InstantiateBar(GameObject curBar)
        {

            if (curBar == null)
            {
                curBar = Instantiate(timingBar, hitContainer.transform);
                curBar.transform.localScale = new Vector3(((float)(skin_columnSize + skin_bgMaskBufferSize + skin_noteBufferSpacing) / config_PixelUnitSize) * 4f * (config_PixelUnitSize / (float)curBar.transform.GetComponent<SpriteRenderer>().sprite.rect.size.x),
                    (config_PixelUnitSize / (float)curBar.transform.GetComponent<SpriteRenderer>().sprite.rect.size.y) * ((float)skin_timingBarPixelSize / config_PixelUnitSize)
                    , 1f);
            }
            TimingObject newTimeOb = new TimingObject();
            newTimeOb.StartTime = _barQueue[0].StartTime;
            newTimeOb.TimingBar = curBar;

            curBar.transform.localPosition = new Vector3(0f, PosFromSV(newTimeOb.StartTime), 2f);

            _activeBars.Add(newTimeOb);
            _barQueue.RemoveAt(0);
            return curBar;
        }
        */

        //Recycles bar objects from object pool or destroys them
        /*
        private void time_RemoveBar(GameObject curBar)
        {
            if (_barQueue.Count > 0) time_InstantiateBar(curBar);
            else Destroy(curBar);
        }*/

        //Creates SV Points
        internal static void CreateSV(float targetTime, float multiplier, int? index = null)
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
        internal static void ConvertTPtoSV()
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
        internal static void CalculateAverageBpm()
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
                        if (GameAudioLength - TimingQueue[i].TargetTime > longestBpmTime)
                        {
                            avergeBpmIndex = i;
                            longestBpmTime = GameAudioLength - TimingQueue[i].TargetTime;
                        }
                    }
                }
                _averageBpm = TimingQueue[avergeBpmIndex].BPM;
            }
            else _averageBpm = TimingQueue[0].BPM;
        }


        //Normalizes SV's in between each BPM change interval
        internal static void NormalizeSVs()
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
                                Console.WriteLine(SvQueue[i].TargetTime);
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
        internal static void MoveTimingBars()
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