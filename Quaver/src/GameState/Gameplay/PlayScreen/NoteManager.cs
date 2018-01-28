using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.Audio;
using Quaver.Config;
using Quaver.GameState;
using Quaver.GameState.States;
using Quaver.Utility;
using Quaver.Graphics;
using Quaver.Graphics.Sprite;
using Quaver.Logging;
using Quaver.Modifiers;
using Quaver.API.Maps;

namespace Quaver.GameState.Gameplay.PlayScreen
{
    /// <summary>
    /// This class manages anything relating to rendering the HitObjects. Note: This class does not do any timing/input calculation besides note removal after missing and late release.
    /// </summary>
    internal class NoteManager : IHelper
    {
        //SV
        internal ulong[] SvCalc { get; set; }
        internal List<TimingObject> SvQueue { get; set; }

        //Measure Bars
        private MeasureBarManager MeasureBarManager { get; set; }

        //HitObjects
        private int[] BeatSnaps { get; } = new int[8] { 48, 24, 16, 12, 8, 6, 4, 3 };
        internal float HitPositionOffset { get; set; }
        internal List<HitObject> HitObjectPool { get; set; }
        internal List<HitObject> HitObjectDead { get; set; }
        internal List<HitObject> HitObjectHold { get; set; }
        internal int HitObjectPoolSize { get; } = 255;
        internal uint RemoveTimeAfterMiss;
        internal Boundary Boundary;

        //Track
        internal int CurrentSvIndex { get; set; }
        internal ulong TrackPosition { get; set; }
        internal double CurrentSongTime { get; set; }

        //Events
        internal event EventHandler PressMissed;
        internal event EventHandler ReleaseSkipped;
        internal event EventHandler ReleaseMissed;

        //CONFIG (temp)
        internal float ScrollSpeed { get; set; }
        internal bool DownScroll { get; set; }
        internal float LaneSize { get; set; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="qua"></param>
        public NoteManager(Qua qua)
        {
            //todo: qua stuff here
        }

        /// <summary>
        /// Initalize any HitObject related content. 
        /// </summary>
        public void Initialize(IGameState state)
        {
            var qua = GameBase.SelectedBeatmap.Qua; //todo: remove
            MeasureBarManager = new MeasureBarManager();
            MeasureBarManager.Initialize(state);

            // Modifiers
            RemoveTimeAfterMiss = (uint)(1000 * GameBase.GameClock);

            // Get Hit Position
            CurrentSvIndex = 0;
            
            //Initialize Track
            TrackPosition = GetCurrentTrackPosition();
            //TrackPosition = (ulong)(-GameplayReferences.PlayStartDelayed + 10000); //10000ms added since curSVPos is a ulong. -2000 offset is the wait time before song starts

            // Initialize Boundary
            Boundary = new Boundary()
            {
                Size = new UDim2(GameplayReferences.PlayfieldSize, 0, 0, 1),
                Alignment = Alignment.TopCenter
            };

            // Initialize Timing Bars
            // todo: Initialize from MeasureBarManager
            for (var i = 0; i < GameBase.SelectedBeatmap.Qua.TimingPoints.Count; i++)
            {
                var startTime = GameBase.SelectedBeatmap.Qua.TimingPoints[i].StartTime;
                var endTime = 0f;
                var curTime = startTime;
                var bpmInterval = 4000 * 60 / GameBase.SelectedBeatmap.Qua.TimingPoints[i].Bpm;

                if (i + 1 < GameBase.SelectedBeatmap.Qua.TimingPoints.Count)
                    endTime = GameBase.SelectedBeatmap.Qua.TimingPoints[i + 1].StartTime;
                else
                    endTime = GameBase.SelectedBeatmap.SongLength;

                while (curTime < endTime - 1)
                {
                    var newBar = new BarObject();
                    newBar.OffsetFromReceptor = SvOffsetFromTime(curTime, GetSvIndex(curTime));
                    MeasureBarManager.BarObjectQueue.Add(newBar);
                    curTime += bpmInterval;
                }
            }

            //todo: remove this. temp
            MeasureBarManager.BarObjectActive = MeasureBarManager.BarObjectQueue;
            for (var i = 0; i < MeasureBarManager.BarObjectActive.Count; i++)
            {
                MeasureBarManager.BarObjectActive[i].Initialize(Boundary, 1, 0);
            }

            // Initialize HitObjects
            HitObjectPool = new List<HitObject>();
            HitObjectDead = new List<HitObject>();
            HitObjectHold = new List<HitObject>();
            for (var i = 0; i < qua.HitObjects.Count; i++)
            {
                HitObject newObject = new HitObject()
                {
                    StartTime = qua.HitObjects[i].StartTime,
                    EndTime = qua.HitObjects[i].EndTime,
                    IsLongNote = qua.HitObjects[i].EndTime > 0,
                    KeyLane = qua.HitObjects[i].Lane,
                    HitObjectSize = LaneSize, //column size 7k
                    HitObjectPosition = new Vector2(GameplayReferences.ReceptorXPosition[qua.HitObjects[i].Lane - 1], 0),
                };

                // Calculate Y-Offset From Receptor
                newObject.OffsetFromReceptor = SvOffsetFromTime(newObject.StartTime, GetSvIndex(newObject.StartTime));
                newObject.HitObjectPositionY = newObject.OffsetFromReceptor + HitPositionOffset;

                // Set Snap Color of Object
                // Right now this method only changes the tint of the hitobject, but hopefully we can come up with something better
                if (GameBase.LoadedSkin.ColourObjectsBySnapDistance)
                {
                    var ti = GetBpmIndex(newObject.StartTime);
                    newObject.SnapIndex = GetSnapIndex(newObject.StartTime - qua.TimingPoints[ti].StartTime, qua.TimingPoints[ti].Bpm, newObject.KeyLane);
                }

                // If the object is a long note
                if (newObject.IsLongNote)
                {
                    //Set LN Variables
                    newObject.LnOffsetFromReceptor = SvOffsetFromTime(newObject.EndTime, GetSvIndex(newObject.EndTime));
                    newObject.InitialLongNoteSize = (ulong)((newObject.LnOffsetFromReceptor - newObject.OffsetFromReceptor) * ScrollSpeed);
                    newObject.CurrentLongNoteSize = newObject.InitialLongNoteSize;
                }

                // Initialize Object and add it to HitObjectPool
                if (i < HitObjectPoolSize) newObject.Initialize(DownScroll, qua.HitObjects[i].EndTime > 0, Boundary, GameBase.SelectedBeatmap.Qua.Mode);
                    HitObjectPool.Add(newObject);
            }

            Logger.Log("Done loading HitObjects", LogColors.GameInfo);
        }

        /// <summary>
        /// Updates any HitObject related content.
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            int i;

            //Update the position of the track
            TrackPosition = GetCurrentTrackPosition();

            //Update Bars
            //todo: check if bars are enabled
            if (true)
            {
                MeasureBarManager.TrackPosition = TrackPosition;
                for (i = 0; i < MeasureBarManager.BarObjectActive.Count; i++)
                {
                    MeasureBarManager.BarObjectActive[i].BarSprite.PosY = PosFromOffset(MeasureBarManager.BarObjectActive[i].OffsetFromReceptor);
                }
                MeasureBarManager.Update(dt);
                //Console.WriteLine(MeasureBarManager.BarObjectActive[0].BarSprite.PositionY);
            }

            //Update Active HitObjects
            for (i=0; i < HitObjectPool.Count && i < HitObjectPoolSize; i++)
            {
                //Note is missed
                if (CurrentSongTime > HitObjectPool[i].StartTime + GameplayReferences.PressWindowLatest)
                {
                    //Invoke Miss Event
                    PressMissed?.Invoke(this, null);

                    //If HitObject is an LN, kill it and count it as another miss
                    if (HitObjectPool[i].IsLongNote)
                    {
                        KillNote(i);
                        ReleaseSkipped?.Invoke(this, null);
                    }

                    //If HitObject is a LongNote, Recycle it
                    else RecycleNote(i);
                    i--;
                }
                //Note is still active
                else
                {
                    // Set new hit object position with the current x, and a new y
                    HitObjectPool[i].HitObjectPositionY = PosFromOffset(HitObjectPool[i].OffsetFromReceptor);
                    HitObjectPool[i].Update(DownScroll);
                }
            }

            //Update Hold Objects
            for (i = 0; i < HitObjectHold.Count; i++)
            {
                //LN is missed
                if (CurrentSongTime > HitObjectHold[i].EndTime + GameplayReferences.ReleaseWindowLatest)
                {
                    //Invoke Miss Event
                    ReleaseMissed?.Invoke(this, null);

                    //Remove from LN Queue
                    HitObjectHold[i].Destroy();
                    HitObjectHold.RemoveAt(i);
                    i--;
                }
                //LN is active
                else
                {
                    //Set LN Size and Note Position
                    if (CurrentSongTime > HitObjectHold[i].StartTime)
                    {
                        HitObjectHold[i].CurrentLongNoteSize = (ulong) ((HitObjectHold[i].LnOffsetFromReceptor - TrackPosition) * ScrollSpeed);
                        HitObjectHold[i].HitObjectPositionY = HitPositionOffset;
                    }
                    else
                    {
                        HitObjectHold[i].CurrentLongNoteSize = HitObjectHold[i].InitialLongNoteSize;
                        HitObjectHold[i].HitObjectPositionY = PosFromOffset(HitObjectHold[i].OffsetFromReceptor);
                    }

                    //Update HitObject
                    HitObjectHold[i].Update(DownScroll);
                }
            }

            //Update Dead HitObjects
            for (i = 0; i < HitObjectDead.Count; i++)
            {
                if (CurrentSongTime > HitObjectDead[i].EndTime + RemoveTimeAfterMiss && CurrentSongTime > HitObjectDead[i].StartTime + RemoveTimeAfterMiss)
                {
                    HitObjectDead[i].Destroy();
                    HitObjectDead.RemoveAt(i);
                    i--;
                }
                else
                {
                    HitObjectDead[i].HitObjectPositionY = PosFromOffset(HitObjectDead[i].OffsetFromReceptor);
                    HitObjectDead[i].Update(DownScroll);
                }
            }

            //Update Boundary
            Boundary.Update(dt);
        }

        /// <summary>
        ///     Draws whatever has to be rendered.
        /// </summary>
        public void Draw()
        {
            if (true) MeasureBarManager.Draw();
            Boundary.Draw();
        }
        /// <summary>
        ///     Unloads content after the game is done.
        /// </summary>
        public void UnloadContent()
        {
            Boundary.Destroy();
            HitObjectHold.Clear();
            HitObjectDead.Clear();
            HitObjectPool.Clear();
        }

        /// <summary>
        ///     Returns color of note beatsnap
        /// </summary>
        /// <param name="timeToOffset"></param>
        /// <param name="bpm"></param>
        /// <returns></returns>
        internal int GetSnapIndex(float offset, float bpm, int lane)
        {
            // Add 2ms offset buffer space to offset and get beat length
            var pos = offset + 2;
            var beatlength = 60000 / bpm;

            // subtract pos until it's less than beat length. multiple loops for efficiency
            while (pos >= beatlength * 65536) pos -= beatlength * 65536;
            while (pos >= beatlength * 4096) pos -= beatlength * 4096;
            while (pos >= beatlength * 256) pos -= beatlength * 256;
            while (pos >= beatlength * 16) pos -= beatlength * 16;
            while (pos >= beatlength) pos -= beatlength;

            // Calculate Note's snap index
            var index = (int)(Math.Floor(48 * pos / beatlength));

            // Return Color of snap index
            for (var i=0; i< 8; i++)
            {
                if (index % BeatSnaps[i] == 0)
                {
                    return i;
                }
            }

            // If it's not snapped to 1/16 or less, return 1/48 snap color
            return 8;
        }

        /// <summary>
        /// Calculates the position of the hitobject from given time
        /// </summary>
        /// <param name="timeToPos"></param>
        /// <returns></returns>
        internal ulong SvOffsetFromTime(float timeToOffset, int svIndex)
        {
            //If NoSV mod is enabled, return ms offset, else return sv offset calculation
            return (ModManager.Activated(ModIdentifier.NoSliderVelocity)) ? (ulong) timeToOffset : SvCalc[svIndex] + (ulong)(15000 + ((timeToOffset - SvQueue[svIndex].TargetTime) * SvQueue[svIndex].SvMultiplier)) - 5000;
        }

        /// <summary>
        /// Calculates the position of the note from offset.
        /// </summary>
        /// <param name="offsetToPos"></param>
        /// <returns></returns>
        internal float PosFromOffset(ulong offsetToPos)
        {
            //if (_mod_pull) return (float)((2f * Math.Max(Math.Pow(posFromTime, 0.6f), 0)) + (Math.Min(offsetToPos - CurrentSongTime, 0f) * _ScrollSpeed));
            return DownScroll
                ? HitPositionOffset + (((10000 + offsetToPos - TrackPosition) - 10000f) * -ScrollSpeed)
                : HitPositionOffset + (((10000 + offsetToPos - TrackPosition) - 10000f) * ScrollSpeed);
        }

        /// <summary>
        /// Calculate track position
        /// </summary>
        internal ulong GetCurrentTrackPosition()
        {
            if (CurrentSongTime >= SvQueue[SvQueue.Count - 1].TargetTime)
            {
                CurrentSvIndex = SvQueue.Count - 1;
            }
            else if (CurrentSvIndex < SvQueue.Count - 2)
            {
                for (int j = CurrentSvIndex; j < SvQueue.Count - 1; j++)
                {
                    if (CurrentSongTime > SvQueue[CurrentSvIndex + 1].TargetTime) CurrentSvIndex++;
                    else break;
                }
            }
            return SvCalc[CurrentSvIndex] + (ulong)(((float)(CurrentSongTime - (SvQueue[CurrentSvIndex].TargetTime)) * SvQueue[CurrentSvIndex].SvMultiplier) + 10000);
        }

        /// <summary>
        /// Gets the Index of the SV timing point from the SV Queue List
        /// </summary>
        /// <param name="indexTime"></param>
        /// <returns></returns>
        internal int GetSvIndex(float indexTime)
        {
            int newIndex = 0;
            if (indexTime >= SvQueue[SvQueue.Count - 1].TargetTime) newIndex = SvQueue.Count - 1;
            else
            {
                for (int j = 0; j < SvQueue.Count - 1; j++)
                {
                    if (indexTime < SvQueue[j + 1].TargetTime)
                    {
                        newIndex = j;
                        break;
                    }
                }
            }

            return newIndex;
        }

        /// <summary>
        /// Gets the Index of the Bpm timing point from the Bpm Queue List
        /// </summary>
        /// <param name="indexTime"></param>
        /// <returns></returns>
        internal int GetBpmIndex(float indexTime)
        {
            var tp = GameBase.SelectedBeatmap.Qua.TimingPoints;
            int newIndex = 0;
            if (indexTime >= tp[tp.Count - 1].StartTime) newIndex = tp.Count - 1;
            else
            {
                for (int j = 0; j < tp.Count - 1; j++)
                {
                    if (indexTime < tp[j + 1].StartTime)
                    {
                        newIndex = j;
                        break;
                    }
                }
            }

            return newIndex;
        }

        /// <summary>
        /// TODO: add description
        /// </summary>
        /// <param name="index"></param>
        internal void KillNote(int index)
        {
            //Kill HitObject (AKA when you miss an LN)
            HitObjectPool[index].Kill();
            HitObjectDead.Add(HitObjectPool[index]);

            //Remove old HitObject
            HitObjectPool.RemoveAt(index);

            //Initialize the new HitObject (create the hit object sprites)
            CreateNote();
        }

        /// <summary>
        /// Kills an object from the HitObjectHold Pool
        /// </summary>
        /// <param name="index"></param>
        internal void KillHold(int index, bool destroy = false)
        {
            //Update the object's position and size
            HitObjectHold[index].StartTime = (float)CurrentSongTime;
            HitObjectHold[index].OffsetFromReceptor = SvOffsetFromTime(HitObjectHold[index].StartTime, GetSvIndex(HitObjectHold[index].StartTime));

            //Kill the object and add it to the HitObjectDead pool
            if (destroy) HitObjectHold[index].Destroy();
            else
            {
                HitObjectHold[index].Kill();
                HitObjectDead.Add(HitObjectHold[index]);
            }

            //Remove from the hold pool
            HitObjectHold.RemoveAt(index);
        }

        /// <summary>
        /// TODO: add description
        /// </summary>
        /// <param name="index"></param>
        internal void HoldNote(int index)
        {
            //Move to LN Hold Pool
            HitObjectHold.Add(HitObjectPool[index]);

            //Remove old HitObject
            HitObjectPool.RemoveAt(index);

            //Initialize the new HitObject (create the hit object sprites)
            CreateNote();
        }

        /// <summary>
        /// TODO: add description
        /// </summary>
        /// <param name="index"></param>
        internal void RecycleNote(int index)
        {
            //Remove old HitObject
            HitObjectPool[index].Destroy();
            HitObjectPool.RemoveAt(index);

            //Initialize the new HitObject (create the hit object sprites)
            CreateNote();
        }

        internal void CreateNote()
        {
            if (HitObjectPool.Count >= HitObjectPoolSize) HitObjectPool[HitObjectPoolSize - 1].Initialize(DownScroll, HitObjectPool[HitObjectPoolSize - 1].EndTime > 0, Boundary, GameBase.SelectedBeatmap.Qua.Mode);
        }
    }
}
