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

namespace Quaver.Gameplay
{
    internal partial class StatePlayScreen : GameStateBase
    {
        //Input Variables
        private KeyboardState _KeyboardState;
        private bool[] _KeyDown = new bool[4];

        internal void CheckInput()
        {
            _KeyboardState = Keyboard.GetState();
            
            for (int i=0; i< 4; i++)
            {
                if (_KeyboardState.IsKeyDown(_config_PlayKey[i]))
                {
                    if (!_KeyDown[i])
                    {
                        _KeyDown[i] = true;

                        //ADD KEY PRESS LOGIC HERE
                        UpdateReceptor(i,true);
                    }
                }
                if (_KeyboardState.IsKeyUp(_config_PlayKey[i]))
                {
                    if (_KeyDown[i])
                    {
                        _KeyDown[i] = false;

                        //ADD KEY RELEASE LOGIC HERE
                        UpdateReceptor(i,false);
                    }
                }
            }

        }
    }
}
