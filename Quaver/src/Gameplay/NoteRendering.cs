using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Quaver.Config;
using Quaver.GameState;
using Quaver.Utility;
using Quaver.Graphics;
using Quaver.Logging;
using Quaver.Main;
using Quaver.Modifiers;
using Quaver.QuaFile;

namespace Quaver.Gameplay
{
    /// <summary>
    /// This class manages anything relating to rendering the HitObjects. Note: This class does not do any timing/input calculation besides note removal after missing and late release.
    /// </summary>
    internal class NoteRendering
    {
        //HitObjects
        internal static List<HitObject> HitObjectPool { get; set; }
        internal static List<HitObject> HitObjectDead { get; set; }
        internal static List<HitObject> HitObjectHold { get; set; }
        internal const int HitObjectPoolSize = 256;
        internal const uint RemoveTimeAfterMiss = 1000;
        internal static Boundary Boundary;

        //Track
        internal static ulong[] SvCalc { get; set; } //Stores SV position data for efficiency
        internal static int CurrentSvIndex { get; set; }
        internal static ulong TrackPosition { get; set; }

        //CONFIG (temp)
        private static float ScrollNegativeFactor { get; set; } = 1f;
        private static float ScrollSpeed { get; set; } = Configuration.ScrollSpeed / 20f; //TODO: Add scroll speed curve

        /// <summary>
        /// Initalize any HitObject related content. 
        /// </summary>
        internal static void Initialize(Qua Qua)
        {
            //Initialize Track
            int i;
            TrackPosition = (ulong)(-Timing.PlayStartDelayed + 10000f); //10000ms added since curSVPos is a ulong
            CurrentSvIndex = 0;

            //Initialize Boundary
            Boundary = new Boundary()
            {
                Size = new Vector2(Playfield.PlayfieldSize, GameBase.Window.Height),
                Alignment = Alignment.TopCenter
            };

            //Initialize HitObjects
            HitObjectPool = new List<HitObject>();
            HitObjectDead = new List<HitObject>();
            HitObjectHold = new List<HitObject>();
            for (i = 0; i < Qua.HitObjects.Count; i++)
            {
                HitObject newObject = new HitObject()
                {
                    ParentContainer = Boundary,
                    StartTime = Qua.HitObjects[i].StartTime,
                    EndTime = Qua.HitObjects[i].EndTime,
                    IsLongNote = Qua.HitObjects[i].EndTime > 0,
                    KeyLane = Qua.HitObjects[i].KeyLane,
                    HitObjectSize = Playfield.PlayfieldObjectSize,
                    HitObjectPosition = new Vector2(Playfield.ReceptorXPosition[Qua.HitObjects[i].KeyLane-1], Qua.HitObjects[i].StartTime * ScrollSpeed),
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
                if (i < HitObjectPoolSize) newObject.Initialize();
                HitObjectPool.Add(newObject);
            }
            Console.WriteLine("[STATE_GAMEPLAY/NoteRendering]: Done Loading Hitobjects.");
        }

        /// <summary>
        /// Updates any HitObject related content.
        /// </summary>
        /// <param name="dt"></param>
        internal static void Update(double dt)
        {
            int i;

            //Update the position of the track
            GetCurrentTrackPosition();

            //Update Active HitObjects
            for (i=0; i < HitObjectPool.Count && i < HitObjectPoolSize; i++)
            {
                //Note is not pressed (Missed)
                if (Timing.CurrentSongTime > HitObjectPool[i].StartTime + ScoreManager.HitWindow[4])
                {
                    //Track note miss with ScoreManager
                    LogManager.QuickLog("NOTE INDEX: MISSED NOTE " + (HitObjectPool[i].KeyLane - 1), Color.IndianRed, 0.5f);
                    ScoreManager.Count(5);

                    //If HitObject is an LN, kill it
                    if (HitObjectPool[i].IsLongNote) KillNote(i);

                    //If HitObject is a LongNote, Recycle it
                    else RecycleNote(i);
                    i--;
                }
                //Note is still active
                else
                {
                    // Set new hit object position with the current x, and a new y
                    HitObjectPool[i].HitObjectPositionY = PosFromOffset(HitObjectPool[i].OffsetFromReceptor);
                    HitObjectPool[i].Update();
                }
            }

            //Update Hold Objects
            for (i = 0; i < HitObjectHold.Count; i++)
            {
                //LN is missed
                if (Timing.CurrentSongTime > HitObjectHold[i].EndTime + ScoreManager.HitWindow[4])
                {
                    //Track LN late release with ScoreManager
                    LogManager.QuickLog("NOTE INDEX: LATE RELEASE " + (HitObjectHold[i].KeyLane - 1), Color.DarkRed, 0.5f);
                    ScoreManager.Count(5);

                    //Remove from LN Queue
                    HitObjectHold[i].Destroy();
                    HitObjectHold.RemoveAt(i);
                    i--;
                }
                //LN is active
                else
                {
                    //Set LN Size and Note Position
                    if (Timing.CurrentSongTime > HitObjectHold[i].StartTime)
                    {
                        HitObjectHold[i].CurrentLongNoteSize = (ulong) ((HitObjectHold[i].LnOffsetFromReceptor - TrackPosition) * ScrollSpeed);
                        HitObjectHold[i].HitObjectPositionY = Playfield.ReceptorYOffset;
                    }
                    else
                    {
                        HitObjectHold[i].CurrentLongNoteSize = HitObjectHold[i].InitialLongNoteSize;
                        HitObjectHold[i].HitObjectPositionY = PosFromOffset(HitObjectHold[i].OffsetFromReceptor);
                    }

                    //Update HitObject
                    HitObjectHold[i].Update();
                }
            }

            //Update Dead HitObjects
            for (i = 0; i < HitObjectDead.Count; i++)
            {
                if (Timing.CurrentSongTime > HitObjectDead[i].EndTime + RemoveTimeAfterMiss)
                {
                    HitObjectDead[i].Destroy();
                    HitObjectDead.RemoveAt(i);
                    i--;
                }
                else
                {
                    HitObjectDead[i].HitObjectPositionY = PosFromOffset(HitObjectDead[i].OffsetFromReceptor);
                    HitObjectDead[i].Update();
                }
            }

            //Update Boundary
            Boundary.Update(dt);
        }

        internal static void UnloadContent()
        {
            //foreach (var ho in HitObjectPool) ho.Destroy();
            //foreach (var ho in HitObjectHold) ho.Destroy();
            //foreach (var ho in HitObjectDead) ho.Destroy();
            HitObjectPool = null;
            HitObjectHold = null;
            HitObjectDead = null;
            Boundary.Destroy();
            //Boundary = null;
        }

        /// <summary>
        /// Calculates the position of the hitobject from given time
        /// </summary>
        /// <param name="timeToPos"></param>
        /// <returns></returns>
        internal static ulong SvOffsetFromTime(float timeToOffset, int svIndex)
        {
            //If NoSV mod is enabled, return ms offset, else return sv offset calculation
            return (ModManager.Activated(ModIdentifier.NoSliderVelocity)) ? (ulong) timeToOffset : SvCalc[svIndex] + (ulong)(15000 + ((timeToOffset - Timing.SvQueue[svIndex].TargetTime) * Timing.SvQueue[svIndex].SvMultiplier)) - 5000;
        }

        /// <summary>
        /// Calculates the position of the note from offset.
        /// </summary>
        /// <param name="offsetToPos"></param>
        /// <returns></returns>
        internal static float PosFromOffset(ulong offsetToPos)
        {
            //if (_mod_pull) return (float)((2f * Math.Max(Math.Pow(posFromTime, 0.6f), 0)) + (Math.Min(offsetToPos - CurrentSongTime, 0f) * _ScrollSpeed));
            return Playfield.ReceptorYOffset + (((float)(10000 + offsetToPos - TrackPosition) - 10000f) * ScrollNegativeFactor * ScrollSpeed);
        }

        /// <summary>
        /// Calculate track position
        /// </summary>
        internal static void GetCurrentTrackPosition()
        {
            if (Timing.CurrentSongTime >= Timing.SvQueue[Timing.SvQueue.Count - 1].TargetTime)
            {
                CurrentSvIndex = Timing.SvQueue.Count - 1;
            }
            else if (CurrentSvIndex < Timing.SvQueue.Count - 2)
            {
                for (int j = CurrentSvIndex; j < Timing.SvQueue.Count - 1; j++)
                {
                    if (Timing.CurrentSongTime > Timing.SvQueue[CurrentSvIndex + 1].TargetTime) CurrentSvIndex++;
                    else break;
                }
            }
            TrackPosition = SvCalc[CurrentSvIndex] + (ulong)(((float)(Timing.CurrentSongTime - (Timing.SvQueue[CurrentSvIndex].TargetTime)) * Timing.SvQueue[CurrentSvIndex].SvMultiplier) + 10000);
        }

        /// <summary>
        /// Gets the Index of the SV timing point from the SV Queue List
        /// </summary>
        /// <param name="indexTime"></param>
        /// <returns></returns>
        internal static int GetSvIndex(float indexTime)
        {
            int newIndex = 0;
            if (indexTime >= Timing.SvQueue[Timing.SvQueue.Count - 1].TargetTime) newIndex = Timing.SvQueue.Count - 1;
            else
            {
                for (int j = 0; j < Timing.SvQueue.Count - 1; j++)
                {
                    if (indexTime < Timing.SvQueue[j + 1].TargetTime)
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
        internal static void KillNote(int index)
        {
            //Kill HitObject (AKA when you miss an LN)
            HitObjectPool[index].Kill();
            HitObjectDead.Add(HitObjectPool[index]);

            //Remove old HitObject
            HitObjectPool.RemoveAt(index);

            //Initialize the new HitObject (create the hit object sprites)
            if (HitObjectPool.Count >= HitObjectPoolSize) HitObjectPool[HitObjectPoolSize - 1].Initialize();
        }

        /// <summary>
        /// Kills an object from the HitObjectHold Pool
        /// </summary>
        /// <param name="index"></param>
        internal static void KillHold(int index, bool destroy = false)
        {
            //Update the object's position and size
            HitObjectHold[index].StartTime = (float)Timing.CurrentSongTime;
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
        internal static void HoldNote(int index)
        {
            //Move to LN Hold Pool
            HitObjectHold.Add(HitObjectPool[index]);

            //Remove old HitObject
            HitObjectPool.RemoveAt(index);

            //Initialize the new HitObject (create the hit object sprites)
            if (HitObjectPool.Count >= HitObjectPoolSize) HitObjectPool[HitObjectPoolSize - 1].Initialize();
        }

        /// <summary>
        /// TODO: add description
        /// </summary>
        /// <param name="index"></param>
        internal static void RecycleNote(int index)
        {
            //Remove old HitObject
            HitObjectPool[index].Destroy();
            HitObjectPool.RemoveAt(index);

            //Initialize the new HitObject (create the hit object sprites)
            if (HitObjectPool.Count >= HitObjectPoolSize) HitObjectPool[HitObjectPoolSize - 1].Initialize();
        }
    }
}
