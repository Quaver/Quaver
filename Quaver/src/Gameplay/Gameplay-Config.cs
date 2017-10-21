using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

using Quaver.Config;
using Quaver.GameState;
using Quaver.Utility;
using Quaver.Graphics;
using Quaver.Main;

namespace Quaver.Gameplay
{
    internal partial class StatePlayScreen : IGameState
    {
        //Config Values
        private int _config_scrollSpeed = 20;
        private bool _config_timingBars;
        private bool _config_upScroll;
        private int _config_offset;
        private const bool _config_EnableNpsGraph = true;
        private const bool _config_EnableMAdisplay = true;

        //Config
        private float _ScrollSpeed;
        private float _scrollNegativeFactor = 1;

        private void InitializeConfig()
        {
            //ScrollSpeed/ScrollDirection
            _ScrollSpeed = _config_scrollSpeed / 20f; //TODO: Set a curve for scroll speed config
            _scrollNegativeFactor = 1;
        }
    }
}
