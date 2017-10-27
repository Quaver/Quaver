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
    /// This class manages anything relating to rendering the HitObjects. Note: This class does not do any timing/input calculation besides note removal after missing.
    /// </summary>
    internal class NoteRendering
    {
        //HitObjects
        internal static List<HitObject> HitObjectPool { get; set; }
        internal static List<HitObject> HitObjectDead { get; set; }
        internal static List<HitObject> HitObjectHold { get; set; }
        internal const int HitObjectPoolSize = 256;
        internal const uint RemoveTimeAfterMiss = 1000;

        //Track
        internal static ulong[] SvCalc { get; set; } //Stores SV position data for efficiency
        internal static int CurrentSVIndex { get; set; }
        internal static ulong TrackPosition { get; set; }

        //CONFIG (temp)
        private static float ScrollNegativeFactor { get; set; } = 1f;
        private static float ScrollSpeed { get; set; } = Configuration.ScrollSpeed / 20f;

        /// <summary>
        /// Initalize any HitObject related content. 
        /// </summary>
        internal static void InitializeNotes(Qua Qua)
        {
            //Initialize Track
            int i;
            TrackPosition = (ulong)(-Timing.PlayStartDelayed + 10000f); //10000ms added since curSVPos is a ulong
            CurrentSVIndex = 0;

            //Initialize HitObjects
            HitObjectPool = new List<HitObject>();
            HitObjectDead = new List<HitObject>();
            HitObjectHold = new List<HitObject>();
            for (i = 0; i < Qua.HitObjects.Count; i++)
            {
                HitObject newObject = new HitObject()
                {
                    ParentContainer = Playfield.PlayfieldBoundary,
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
            LogTracker.AddLogger("noteRemoved",Color.Red);
        }

        /// <summary>
        /// Updates any HitObject related content.
        /// </summary>
        /// <param name="dt"></param>
        internal static void UpdateNotes(double dt)
        {
            int i;

            //Update the position of the track
            GetCurrentTrackPosition();

            //Update Active HitObjects
            for (i=0; i < HitObjectPool.Count && i < HitObjectPoolSize; i++)
            {
                if (Timing.CurrentSongTime > HitObjectPool[i].StartTime + NoteManager.HitTiming[4])
                {
                    LogTracker.UpdateLogger("noteRemoved", "last note removed: index #"+i+ " total remain: "+(HitObjectPool.Count-HitObjectPoolSize));

                    //If HitObject is an LN, kill it
                    if (HitObjectPool[i].IsLongNote) KillNote(i);

                    //If HitObject is a LongNote, Recycle it
                    else RecycleNote(i);
                    i--;
                }
                else
                {
                    // Set new hit object position with the current x, and a new y
                    HitObjectPool[i].HitObjectPosition = new Vector2(HitObjectPool[i].HitObjectPosition.X, PosFromOffset(HitObjectPool[i].OffsetFromReceptor));
                    HitObjectPool[i].UpdateObject();
                }
            }

            //Update Hold Objects
            for (i = 0; i < HitObjectHold.Count; i++)
            {
                if (Timing.CurrentSongTime > HitObjectHold[i].EndTime + NoteManager.HitTiming[4])
                {
                    //Remove from LN Queue
                    HitObjectHold[i].Destroy();
                    HitObjectHold.RemoveAt(i);
                    i--;
                }
                else
                {
                    //Set LN Size
                    if (Timing.CurrentSongTime > HitObjectHold[i].StartTime)
                        HitObjectHold[i].CurrentLongNoteSize =(ulong) ((HitObjectHold[i].LnOffsetFromReceptor - TrackPosition) * ScrollSpeed);
                    else
                        HitObjectHold[i].CurrentLongNoteSize = HitObjectHold[i].InitialLongNoteSize;

                    //Update Position and Object
                    HitObjectHold[i].HitObjectPosition = new Vector2(HitObjectHold[i].HitObjectPosition.X,(float) Playfield.ReceptorYOffset);
                    HitObjectHold[i].UpdateObject();
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
                    HitObjectDead[i].HitObjectPosition = new Vector2(HitObjectDead[i].HitObjectPosition.X,PosFromOffset(HitObjectDead[i].OffsetFromReceptor));
                    HitObjectDead[i].UpdateObject();
                }
            }
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
                CurrentSVIndex = Timing.SvQueue.Count - 1;
            }
            else if (CurrentSVIndex < Timing.SvQueue.Count - 2)
            {
                for (int j = CurrentSVIndex; j < Timing.SvQueue.Count - 1; j++)
                {
                    if (Timing.CurrentSongTime > Timing.SvQueue[CurrentSVIndex + 1].TargetTime) CurrentSVIndex++;
                    else break;
                }
            }
            TrackPosition = SvCalc[CurrentSVIndex] + (ulong)(((float)(Timing.CurrentSongTime - (Timing.SvQueue[CurrentSVIndex].TargetTime)) * Timing.SvQueue[CurrentSVIndex].SvMultiplier) + 10000);
        }

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
        internal static void KillHold(int index)
        {
            //Kill the object and add it to the HitObjectDead pool
            HitObjectHold[index].Kill();
            HitObjectDead.Add(HitObjectHold[index]);

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
