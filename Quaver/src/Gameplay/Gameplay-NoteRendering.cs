using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Quaver.GameState;
using Quaver.Utility;
using Quaver.Graphics;
using Quaver.Main;

namespace Quaver.Gameplay
{
    internal partial class StatePlayScreen
    {
        //HitObjects
        private List<HitObject> _hitObjectQueue;
        private const int HitObjectPoolSize = 200;

        //Track
        private ulong[] _svCalc; //Stores SV position data for efficiency
        private int _currentSVIndex;
        private ulong _trackPosition;

        /// <summary>
        /// Initalize any HitObject related content. 
        /// </summary>
        internal void InitializeNotes()
        {
            //Initialize Track
            int i;
            _trackPosition = (ulong)(-PlayStartDelayed + 10000f); //10000ms added since curSVPos is a ulong
            _currentSVIndex = 0;

            //Initialize HitObjects
            _hitObjectQueue = new List<HitObject>();
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
                if (newObject.StartTime >= _svQueue[_svQueue.Count - 1].TargetTime) newObject.SvPosition = _svQueue.Count - 1;
                else
                {
                    for (int j = 0; j < _svQueue.Count - 1; j++)
                    {
                        if (newObject.StartTime < _svQueue[j + 1].TargetTime)
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
                    if (newObject.EndTime >= _svQueue[_svQueue.Count - 1].TargetTime) curIndex = _svQueue.Count - 1;
                    else
                    {
                        for (int j = 0; j < _svQueue.Count - 1; j++)
                        {
                            if (newObject.EndTime < _svQueue[j + 1].TargetTime)
                            {
                                curIndex = j;
                                break;
                            }
                        }
                    }

                    newObject.InitialLongNoteSize = SvOffsetFromTime(newObject.EndTime, curIndex) - newObject.OffsetFromReceptor;
                    newObject.CurrentLongNoteSize = newObject.InitialLongNoteSize;
                }

                //Initialize Object and add it to HitObjectQueue
                if (i < HitObjectPoolSize) newObject.Initialize();
                _hitObjectQueue.Add(newObject);
            }
            Console.WriteLine("[STATE_GAMEPLAY/NoteRendering]: Done Loading Hitobjects.");
        }

        /// <summary>
        /// Updates any HitObject related content.
        /// </summary>
        /// <param name="dt"></param>
        internal void UpdateNotes(double dt)
        {
            //Update the position of the track
            GetCurrentTrackPosition();

            int i;
            for (i=0; i< _hitObjectQueue.Count && i < HitObjectPoolSize; i++)
            {
                if (_currentSongTime > _hitObjectQueue[i].StartTime && _currentSongTime > _hitObjectQueue[i].EndTime) //TODO: Add miss ms timing later
                {
                    //Recycle Note
                    RecycleNote(i);
                    i--;
                }
                else
                {
                    // Set new hit object position with the current x, and a new y
                    _hitObjectQueue[i].HitObjectPosition = new Vector2(_hitObjectQueue[i].HitObjectPosition.X, PosFromOffset(_hitObjectQueue[i].OffsetFromReceptor));
                    _hitObjectQueue[i].UpdateObject();
                }
            }
        }

        /// <summary>
        /// Calculates the position of the hitobject from given time
        /// </summary>
        /// <param name="timeToPos"></param>
        /// <returns></returns>
        private ulong SvOffsetFromTime(float timeToOffset, int svIndex)
        {
            //If NoSV mod is enabled, return ms offset, else return sv offset calculation
            return (ModNoSv) ? (ulong) timeToOffset : _svCalc[svIndex] + (ulong)(15000 + ((timeToOffset - _svQueue[svIndex].TargetTime) * _svQueue[svIndex].SvMultiplier)) - 5000;
        }

        private float PosFromOffset(ulong offsetToPos)
        {
            //if (_mod_pull) return (float)((2f * Math.Max(Math.Pow(posFromTime, 0.6f), 0)) + (Math.Min(offsetToPos - _currentSongTime, 0f) * _ScrollSpeed));
            return Playfield.ReceptorYOffset + (((float)(10000 + offsetToPos - _trackPosition) - 10000f) * ScrollNegativeFactor * ScrollSpeed);
        }

        /// <summary>
        /// Calculate track position
        /// </summary>
        private void GetCurrentTrackPosition()
        {
            if (_currentSongTime >= _svQueue[_svQueue.Count - 1].TargetTime)
            {
                _currentSVIndex = _svQueue.Count - 1;
            }
            else if (_currentSVIndex < _svQueue.Count - 2)
            {
                for (int j = _currentSVIndex; j < _svQueue.Count - 1; j++)
                {
                    if (_currentSongTime > _svQueue[_currentSVIndex + 1].TargetTime) _currentSVIndex++;
                    else break;
                }
            }
            _trackPosition = _svCalc[_currentSVIndex] + (ulong)(((float)((_currentSongTime) - (_svQueue[_currentSVIndex].TargetTime)) * _svQueue[_currentSVIndex].SvMultiplier) + 10000);
        }

        /// <summary>
        /// TODO: add description
        /// </summary>
        /// <param name="index"></param>
        private void RecycleNote(int index)
        {
            _hitObjectQueue[index].Destroy();
            _hitObjectQueue.RemoveAt(index);

            //Initialize the HitObject (create the hit object sprites) if there are any inactive HitObjects
            if (_hitObjectQueue.Count > HitObjectPoolSize)
                _hitObjectQueue[HitObjectPoolSize-1].Initialize();
        }


    }
}
