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


        /// <summary>
        /// Initalize any HitObject related content. 
        /// </summary>
        internal void InitializeNotes()
        {
            int i;

            Console.WriteLine("[STATE_GAMEPLAY/NoteRendering]: TOTAL HITOBJECTS: " + _qua.HitObjects.Count);
            _hitObjectQueue = new List<HitObject>();
            for (i = 0; i < _qua.HitObjects.Count && i < 200; i++)
            {
                HitObject newObject = new HitObject()
                {
                    ParentContainer = _PlayField,
                    StartTime = _qua.HitObjects[i].StartTime,
                    EndTime = _qua.HitObjects[i].EndTime,
                    isLongNote = _qua.HitObjects[i].EndTime > 0,
                    KeyLane = _qua.HitObjects[i].KeyLane,
                    HitObjectSize = _PlayFieldObjectSize,
                    HitObjectPosition = new Vector2(_ReceptorXPosition[_qua.HitObjects[i].KeyLane-1], _qua.HitObjects[i].StartTime * _ScrollSpeed)
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

                //Initialize Object and add it to HitObjectQueue
                newObject.Initialize();
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
            int i;
            for (i=0; i< _hitObjectQueue.Count; i++)
            {
                //_HitObjectQueue[i].HitObjectPosition = new Vector2(_ReceptorXPosition[_HitObjectQueue[i].KeyLane - 1], (float)((_HitObjectQueue[i].StartTime - _CurrentSongTime) * _ScrollSpeed + _ReceptorYOffset));
                _hitObjectQueue[i].HitObjectY = PosFromSv(_hitObjectQueue[i].StartTime, _hitObjectQueue[i].SvPosition); //(float)((_hitObjectQueue[i].StartTime - _currentSongTime) * _ScrollSpeed); //not synced
                _hitObjectQueue[i].UpdateObject();
            }
        }

        /// <summary>
        /// Calculates the position from SV
        /// </summary>
        /// <param name="timeToPos"></param>
        /// <returns></returns>
        private float PosFromSv(double timeToPos, int svIndex)
        {
            //This variable returns the position from the given time
            float posFromTime = 0;

            //If NoSV mod is enabled
            if (_mod_noSV) posFromTime = (float)((timeToPos - _currentSongTime) * _ScrollSpeed);
            //If SVs are enabled
            {
                ulong svTimeToPos = _svCalc[svIndex] + (ulong)(15000 + ((timeToPos - _svQueue[svIndex].TargetTime) * _svQueue[svIndex].SvMultiplier));
                posFromTime = (svTimeToPos - _curSVPos - 5000f) * _ScrollSpeed;
            }

            if (_mod_pull)
                posFromTime = (float)((2f * Math.Max(Math.Pow(posFromTime, 0.6f), 0)) + (Math.Min(timeToPos - _currentSongTime, 0f) * _ScrollSpeed));

            return (posFromTime * _scrollNegativeFactor) + _ReceptorYOffset;
        }
    }
}
