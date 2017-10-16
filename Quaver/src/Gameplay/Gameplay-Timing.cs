using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.GameState;
using Quaver.Utility;
using Quaver.Graphics;
using Quaver.Main;

namespace Quaver.Gameplay
{
    internal partial class StatePlayScreen : GameStateBase
    {
        private float _CurrentSongTime;

        /// <summary>
        /// Initialize Timing Contents.
        /// </summary>
        internal void InitializeTiming()
        {
            //TODO: Timing Initializer
            _CurrentSongTime = 0;
        }

        /// <summary>
        /// Calculates the Timing/SV of the current song position
        /// </summary>
        internal void UpdateTiming(double dt)
        {
            _CurrentSongTime = (float)_GameAudio.GetAudioPosition()*1000f;
        }
    }
}
