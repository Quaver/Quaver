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
        //Config
        private float _ScrollSpeed = 1.4f;

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
                _hitObjectQueue[i].HitObjectY = (float)((_hitObjectQueue[i].StartTime - _currentSongTime) * _ScrollSpeed); //not synced
                _hitObjectQueue[i].UpdateObject();
            }
        }

        /// <summary>
        /// Calculates the position from SV
        /// </summary>
        /// <param name="TimeToPos"></param>
        /// <returns></returns>
        private double PosFromSV(double TimeToPos)
        {
            double PosFromTime = 0;
            
            /*
            float returnVal;
            if (Math.Abs(timePosition - _currentSongTime) >= 0.01)
            {
                if (!_mod_noSV)
                {
                    ulong svPosTime = 0;
                    int curPos = 0;
                    if (timePosition >= _svQueue[_svQueue.Count - 1].StartTime)
                    {
                        curPos = _svQueue.Count - 1;
                    }
                    else
                    {
                        for (int i = 0; i < _svQueue.Count - 1; i++)
                        {
                            if (timePosition < _svQueue[i + 1].StartTime)
                            {
                                curPos = i;
                                break;
                            }
                        }
                    }
                    svPosTime = _svCalc[curPos] + (ulong)(15000 + ((timePosition - _svQueue[curPos].StartTime) * _svQueue[curPos].Multiplier));
                    //10000ms added for negative, since svPos is a ulong

                    returnVal = (float)(svPosTime - _curSVPos - 5000f) / 1000f * (float)_config_scrollSpeed * (1 / _songAudio.pitch);
                }
                else returnVal = (timePosition - _currentSongTime) / 1000f * (float)_config_scrollSpeed * (1 / _songAudio.pitch);
            }
            else returnVal = 0;

            if (_mod_pull) returnVal = (2f * Mathf.Max(Mathf.Pow(returnVal, 0.6f), 0)) + (Mathf.Min(timePosition - _currentSongTime, 0f) / 1000f * (float)_config_scrollSpeed * (1 / _songAudio.pitch));

            return (returnVal * _scrollNegativeFactor) + _receptorYPos;
            */
            return PosFromTime;
        }
    }
}
