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
        private List<HitObject> _hitObjectPool;
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
            _trackPosition = (ulong)(-PlayStartDelayed + 10000f); //10000ms added since curSVPos is a long
            _currentSVIndex = 0;

            //Initialize HitObjects
            _hitObjectQueue = new List<HitObject>();
            for (i = 0; i < Qua.HitObjects.Count; i++)
            {
                HitObject newObject = new HitObject()
                {
                    ParentContainer = _PlayField,
                    StartTime = Qua.HitObjects[i].StartTime,
                    EndTime = Qua.HitObjects[i].EndTime,
                    isLongNote = Qua.HitObjects[i].EndTime > 0,
                    KeyLane = Qua.HitObjects[i].KeyLane,
                    HitObjectSize = _PlayFieldObjectSize,
                    HitObjectPosition = new Vector2(_ReceptorXPosition[Qua.HitObjects[i].KeyLane-1], Qua.HitObjects[i].StartTime * _ScrollSpeed)
                };

                //Calculate SV Index for object
                if (newObject.StartTime >= _svQueue[_svQueue.Count - 1].TargetTime)
                {
                    newObject.SvPosition = _svQueue.Count - 1;
                }
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
                //_HitObjectQueue[i].HitObjectPosition = new Vector2(_ReceptorXPosition[_HitObjectQueue[i].KeyLane - 1], (float)((_HitObjectQueue[i].StartTime - _CurrentSongTime) * _ScrollSpeed + _ReceptorYOffset));
                _hitObjectQueue[i].HitObjectY = PosFromOffset(_hitObjectQueue[i].OffsetFromReceptor); //(float)((_hitObjectQueue[i].StartTime - _currentSongTime) * _ScrollSpeed); //not synced
                _hitObjectQueue[i].UpdateObject();
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
            return (_mod_noSV) ? (ulong) timeToOffset : _svCalc[svIndex] + (ulong)(15000 + ((timeToOffset - _svQueue[svIndex].TargetTime) * _svQueue[svIndex].SvMultiplier)) - 5000;
        }

        private float PosFromOffset(ulong offsetToPos)
        {
            //if (_mod_pull) return (float)((2f * Math.Max(Math.Pow(posFromTime, 0.6f), 0)) + (Math.Min(offsetToPos - _currentSongTime, 0f) * _ScrollSpeed));
            return _ReceptorYOffset + (((float)(10000 + offsetToPos - _trackPosition) - 10000f) * _scrollNegativeFactor * _ScrollSpeed);
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
    }
}
