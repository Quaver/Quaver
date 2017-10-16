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
    internal partial class StatePlayScreen : GameStateBase
    {
        //Config
        private float _ScrollSpeed = 1.4f;

        //HitObjects
        private List<HitObject> _HitObjectQueue;

        /// <summary>
        /// Initalize any HitObject related content. 
        /// </summary>
        internal void InitializeNotes()
        {
            int i;

            Console.WriteLine("[STATE_GAMEPLAY/NoteRendering]: TOTAL HITOBJECTS: " + _Qua.HitObjects.Count);
            _HitObjectQueue = new List<HitObject>();
            for (i = 0; i < _Qua.HitObjects.Count && i < 200; i++)
            {
                HitObject newObject = new HitObject()
                {
                    ParentContainer = _PlayField,
                    StartTime = _Qua.HitObjects[i].StartTime,
                    EndTime = _Qua.HitObjects[i].EndTime,
                    isLongNote = _Qua.HitObjects[i].EndTime > 0,
                    KeyLane = _Qua.HitObjects[i].KeyLane,
                    HitObjectSize = _PlayFieldObjectSize,
                    HitObjectPosition = new Vector2(_ReceptorXPosition[_Qua.HitObjects[i].KeyLane-1], _Qua.HitObjects[i].StartTime * _ScrollSpeed)
                };
                newObject.Initialize();
                _HitObjectQueue.Add(newObject);
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
            for (i=0; i< _HitObjectQueue.Count; i++)
            {
                //_HitObjectQueue[i].HitObjectPosition = new Vector2(_ReceptorXPosition[_HitObjectQueue[i].KeyLane - 1], (float)((_HitObjectQueue[i].StartTime - _CurrentSongTime) * _ScrollSpeed + _ReceptorYOffset));
                _HitObjectQueue[i].HitObjectY = (float)((_HitObjectQueue[i].StartTime - _CurrentSongTime - 100) * _ScrollSpeed); //not synced
                _HitObjectQueue[i].UpdateObject();
            }
        }
    }
}
