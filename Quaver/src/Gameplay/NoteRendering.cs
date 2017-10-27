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
    /// This class manages anything relating to rendering the HitObjects
    /// </summary>
    internal class NoteRendering
    {
        //HitObjects
        internal static List<HitObject> HitObjectPool { get; set; }
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
                if (newObject.StartTime >= Timing.SvQueue[Timing.SvQueue.Count - 1].TargetTime) newObject.SvPosition = Timing.SvQueue.Count - 1;
                else
                {
                    for (int j = 0; j < Timing.SvQueue.Count - 1; j++)
                    {
                        if (newObject.StartTime < Timing.SvQueue[j + 1].TargetTime)
                        {
                            newObject.SvPosition = j;
                            break;
                        }
                    }
                }

                //Calculate Y-Offset From Receptor
                newObject.OffsetFromReceptor = SvOffsetFromTime(newObject.StartTime, newObject.SvPosition);

                //If the object is a long note
                if (newObject.IsLongNote)
                {
                    int curIndex = 0;
                    if (newObject.EndTime >= Timing.SvQueue[Timing.SvQueue.Count - 1].TargetTime) curIndex = Timing.SvQueue.Count - 1;
                    else
                    {
                        for (int j = 0; j < Timing.SvQueue.Count - 1; j++)
                        {
                            if (newObject.EndTime < Timing.SvQueue[j + 1].TargetTime)
                            {
                                curIndex = j;
                                break;
                            }
                        }
                    }

                    newObject.InitialLongNoteSize = (ulong)((SvOffsetFromTime(newObject.EndTime, curIndex) - newObject.OffsetFromReceptor)*ScrollSpeed);
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
            //Update the position of the track
            GetCurrentTrackPosition();

            int i;
            for (i=0; i < HitObjectPool.Count && i < HitObjectPoolSize; i++)
            {
                if (Timing.CurrentSongTime > HitObjectPool[i].StartTime + RemoveTimeAfterMiss && Timing.CurrentSongTime > HitObjectPool[i].EndTime + RemoveTimeAfterMiss) //TODO: Add miss ms Timing later
                {
                    LogTracker.UpdateLogger("noteRemoved", "last note removed: index #"+i+ " total remain: "+(HitObjectPool.Count-HitObjectPoolSize));
                    //Recycle Note
                    RecycleNote(i);
                    i--;
                }
                else
                {
                    // Set new hit object position with the current x, and a new y
                    HitObjectPool[i].HitObjectPosition = new Vector2(HitObjectPool[i].HitObjectPosition.X, PosFromOffset(HitObjectPool[i].OffsetFromReceptor));
                    HitObjectPool[i].UpdateObject();
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
