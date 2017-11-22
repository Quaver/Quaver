using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Audio;
using Quaver.Config;
using Quaver.GameState;
using Quaver.GameState.States;
using Quaver.Utility;
using Quaver.Graphics;
using Quaver.Graphics.Sprite;
using Quaver.Logging;

using Quaver.Modifiers;
using Quaver.QuaFile;

namespace Quaver.Gameplay.GameplayRendering
{
    /// <summary>
    /// This class manages anything relating to rendering the HitObjects. Note: This class does not do any timing/input calculation besides note removal after missing and late release.
    /// </summary>
    internal class NoteRendering : IGameplayRendering
    {
        public PlayScreenState PlayScreen { get; set; }

        //HitObjects
        internal List<HitObject> HitObjectPool { get; set; }
        internal List<HitObject> HitObjectDead { get; set; }
        internal List<HitObject> HitObjectHold { get; set; }
        internal int HitObjectPoolSize { get; } = 200;
        internal const uint RemoveTimeAfterMiss = 1000;
        internal Boundary Boundary;

        //Track
        internal ulong[] SvCalc { get; set; } //Stores SV position data for efficiency
        internal int CurrentSvIndex { get; set; }
        internal ulong TrackPosition { get; set; }

        //CONFIG (temp)
        private float ScrollNegativeFactor { get; set; }
        private float ScrollSpeed { get; set; }

        /// <summary>
        /// Initalize any HitObject related content. 
        /// </summary>
        public void Initialize(PlayScreenState playScreen)
        {
            PlayScreen = playScreen;

            // Do config stuff
            ScrollNegativeFactor = Config.Configuration.DownScroll ? -1 : 1;
            ScrollSpeed = Configuration.ScrollSpeed / (20f * GameBase.GameClock); //todo: balance curve

            //Initialize Track
            int i;
            TrackPosition = (ulong)(-Timing.PlayStartDelayed + 10000f); //10000ms added since curSVPos is a ulong
            CurrentSvIndex = 0;

            //Initialize Boundary
            Boundary = new Boundary()
            {
                Size = new Vector2(PlayScreen.Playfield.PlayfieldSize, GameBase.Window.Z),
                Alignment = Alignment.TopCenter
            };

            //Initialize HitObjects
            HitObjectPool = new List<HitObject>();
            HitObjectDead = new List<HitObject>();
            HitObjectHold = new List<HitObject>();
            for (i = 0; i < GameBase.SelectedBeatmap.Qua.HitObjects.Count; i++)
            {
                HitObject newObject = new HitObject()
                {
                    ParentContainer = Boundary,
                    StartTime = GameBase.SelectedBeatmap.Qua.HitObjects[i].StartTime,
                    EndTime = GameBase.SelectedBeatmap.Qua.HitObjects[i].EndTime,
                    IsLongNote = GameBase.SelectedBeatmap.Qua.HitObjects[i].EndTime > 0,
                    KeyLane = GameBase.SelectedBeatmap.Qua.HitObjects[i].KeyLane,
                    HitObjectSize = PlayScreen.Playfield.PlayfieldObjectSize,
                    HitObjectPosition = new Vector2(PlayScreen.Playfield.ReceptorXPosition[GameBase.SelectedBeatmap.Qua.HitObjects[i].KeyLane-1], GameBase.SelectedBeatmap.Qua.HitObjects[i].StartTime * ScrollSpeed),
                };

                //Calculate SV Index for hit object
                newObject.SvIndex = GetSvIndex(newObject.StartTime);

                //Calculate Y-Offset From Receptor
                newObject.OffsetFromReceptor = SvOffsetFromTime(newObject.StartTime, newObject.SvIndex);

                //If the object is a long note
                if (newObject.IsLongNote)
                {
                    //Set LN Variables
                    newObject.LnOffsetFromReceptor = SvOffsetFromTime(newObject.EndTime, GetSvIndex(newObject.EndTime));
                    newObject.InitialLongNoteSize = (ulong)((newObject.LnOffsetFromReceptor - newObject.OffsetFromReceptor)*ScrollSpeed);
                    newObject.CurrentLongNoteSize = newObject.InitialLongNoteSize;
                }

                //Initialize Object and add it to HitObjectPool
                if (i < HitObjectPoolSize) newObject.Initialize(Config.Configuration.DownScroll, GameBase.SelectedBeatmap.Qua.HitObjects[i].EndTime > 0);
                HitObjectPool.Add(newObject);
            }

            Logger.Log("Done loading HitObjects", Color.AntiqueWhite);
        }

        /// <summary>
        /// Updates any HitObject related content.
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            int i;

            //Update the position of the track
            GetCurrentTrackPosition();

            //Update Active HitObjects
            for (i=0; i < HitObjectPool.Count && i < HitObjectPoolSize; i++)
            {
                //Note is not pressed (Missed)
                if (PlayScreen.Timing.CurrentSongTime > HitObjectPool[i].StartTime + PlayScreen.ScoreManager.HitWindowPress[4])
                {
                    //Track note miss with ScoreManager
                    PlayScreen.ScoreManager.Count(5);
                    PlayScreen.GameplayUI.UpdateAccuracyBox(5);
                    PlayScreen.Playfield.UpdateJudge(5);

                    //If HitObject is an LN, kill it
                    if (HitObjectPool[i].IsLongNote)
                    {
                        KillNote(i);
                        PlayScreen.ScoreManager.Count(5, true);
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
                    HitObjectPool[i].Update(Config.Configuration.DownScroll);
                }
            }

            //Update Hold Objects
            if (HitObjectHold.Count == 0) PlayScreen.GameplayUI.NoteHolding = false;
            else PlayScreen.GameplayUI.NoteHolding = true;

            for (i = 0; i < HitObjectHold.Count; i++)
            {
                //LN is missed
                if (PlayScreen.Timing.CurrentSongTime > HitObjectHold[i].EndTime + PlayScreen.ScoreManager.HitWindowPress[4])
                {
                    //Track LN late release with ScoreManager
                    PlayScreen.ScoreManager.Count(5,true);

                    //Remove from LN Queue
                    HitObjectHold[i].Destroy();
                    HitObjectHold.RemoveAt(i);
                    i--;
                }
                //LN is active
                else
                {
                    //Set LN Size and Note Position
                    if (PlayScreen.Timing.CurrentSongTime > HitObjectHold[i].StartTime)
                    {
                        HitObjectHold[i].CurrentLongNoteSize = (ulong) ((HitObjectHold[i].LnOffsetFromReceptor - TrackPosition) * ScrollSpeed);
                        HitObjectHold[i].HitObjectPositionY = PlayScreen.Playfield.ReceptorYOffset;
                    }
                    else
                    {
                        HitObjectHold[i].CurrentLongNoteSize = HitObjectHold[i].InitialLongNoteSize;
                        HitObjectHold[i].HitObjectPositionY = PosFromOffset(HitObjectHold[i].OffsetFromReceptor);
                    }

                    //Update HitObject
                    HitObjectHold[i].Update(Config.Configuration.DownScroll);
                }
            }

            //Update Dead HitObjects
            for (i = 0; i < HitObjectDead.Count; i++)
            {
                if (PlayScreen.Timing.CurrentSongTime > HitObjectDead[i].EndTime + RemoveTimeAfterMiss && PlayScreen.Timing.CurrentSongTime > HitObjectDead[i].StartTime + RemoveTimeAfterMiss)
                {
                    HitObjectDead[i].Destroy();
                    HitObjectDead.RemoveAt(i);
                    i--;
                }
                else
                {
                    HitObjectDead[i].HitObjectPositionY = PosFromOffset(HitObjectDead[i].OffsetFromReceptor);
                    HitObjectDead[i].Update(Config.Configuration.DownScroll);
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
        /// Calculates the position of the hitobject from given time
        /// </summary>
        /// <param name="timeToPos"></param>
        /// <returns></returns>
        internal ulong SvOffsetFromTime(float timeToOffset, int svIndex)
        {
            //If NoSV mod is enabled, return ms offset, else return sv offset calculation
            return (ModManager.Activated(ModIdentifier.NoSliderVelocity)) ? (ulong) timeToOffset : SvCalc[svIndex] + (ulong)(15000 + ((timeToOffset - PlayScreen.Timing.SvQueue[svIndex].TargetTime) * PlayScreen.Timing.SvQueue[svIndex].SvMultiplier)) - 5000;
        }

        /// <summary>
        /// Calculates the position of the note from offset.
        /// </summary>
        /// <param name="offsetToPos"></param>
        /// <returns></returns>
        internal float PosFromOffset(ulong offsetToPos)
        {
            //if (_mod_pull) return (float)((2f * Math.Max(Math.Pow(posFromTime, 0.6f), 0)) + (Math.Min(offsetToPos - CurrentSongTime, 0f) * _ScrollSpeed));
            return PlayScreen.Playfield.ReceptorYOffset + (((float)(10000 + offsetToPos - TrackPosition) - 10000f) * ScrollNegativeFactor * ScrollSpeed);
        }

        /// <summary>
        /// Calculate track position
        /// </summary>
        internal void GetCurrentTrackPosition()
        {
            if (PlayScreen.Timing.CurrentSongTime >= PlayScreen.Timing.SvQueue[PlayScreen.Timing.SvQueue.Count - 1].TargetTime)
            {
                CurrentSvIndex = PlayScreen.Timing.SvQueue.Count - 1;
            }
            else if (CurrentSvIndex < PlayScreen.Timing.SvQueue.Count - 2)
            {
                for (int j = CurrentSvIndex; j < PlayScreen.Timing.SvQueue.Count - 1; j++)
                {
                    if (PlayScreen.Timing.CurrentSongTime > PlayScreen.Timing.SvQueue[CurrentSvIndex + 1].TargetTime) CurrentSvIndex++;
                    else break;
                }
            }
            TrackPosition = SvCalc[CurrentSvIndex] + (ulong)(((float)(PlayScreen.Timing.CurrentSongTime - (PlayScreen.Timing.SvQueue[CurrentSvIndex].TargetTime)) * PlayScreen.Timing.SvQueue[CurrentSvIndex].SvMultiplier) + 10000);
        }

        /// <summary>
        /// Gets the Index of the SV timing point from the SV Queue List
        /// </summary>
        /// <param name="indexTime"></param>
        /// <returns></returns>
        internal int GetSvIndex(float indexTime)
        {
            int newIndex = 0;
            if (indexTime >= PlayScreen.Timing.SvQueue[PlayScreen.Timing.SvQueue.Count - 1].TargetTime) newIndex = PlayScreen.Timing.SvQueue.Count - 1;
            else
            {
                for (int j = 0; j < PlayScreen.Timing.SvQueue.Count - 1; j++)
                {
                    if (indexTime < PlayScreen.Timing.SvQueue[j + 1].TargetTime)
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
            HitObjectHold[index].StartTime = (float)PlayScreen.Timing.CurrentSongTime;
            HitObjectHold[index].SvIndex = GetSvIndex(HitObjectHold[index].StartTime);
            HitObjectHold[index].OffsetFromReceptor = SvOffsetFromTime(HitObjectHold[index].StartTime, HitObjectHold[index].SvIndex);

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
            if (HitObjectPool.Count >= HitObjectPoolSize) HitObjectPool[HitObjectPoolSize - 1].Initialize(Config.Configuration.DownScroll, HitObjectPool[HitObjectPoolSize - 1].EndTime > 0);
        }
    }
}
