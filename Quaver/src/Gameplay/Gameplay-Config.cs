using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

using Quaver.Config;
using Quaver.Utility;
using Quaver.Graphics;
using Quaver.Main;

namespace Quaver.Gameplay
{
    internal partial class StatePlayScreen
    {
        //Config Values
        private int _config_scrollSpeed = 20;
        private bool _config_timingBars;
        private bool _config_upScroll;
        private int _config_offset;
        private const bool _config_EnableNpsGraph = true;
        private const bool _config_EnableMAdisplay = true;
        private Keys[] _config_PlayKey = new Keys[4];

        //Config
        private float _ScrollSpeed;
        private float _scrollNegativeFactor = 1;

        private void InitializeConfig()
        {
            //Mania Keys
            _config_PlayKey[0] = Configuration.KeyMania1;
            _config_PlayKey[1] = Configuration.KeyMania2;
            _config_PlayKey[2] = Configuration.KeyMania3;
            _config_PlayKey[3] = Configuration.KeyMania4;

            //ScrollSpeed/ScrollDirection
            _ScrollSpeed = _config_scrollSpeed / 20f; //TODO: Set a curve for scroll speed config
            _scrollNegativeFactor = 1;
        }
    }
}
