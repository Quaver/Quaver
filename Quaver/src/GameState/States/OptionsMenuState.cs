using Quaver.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState.States
{
    class OptionsMenuState : IGameState
    {
        public State CurrentState { get; set; }
        public bool UpdateReady { get; set; }

        public void Draw()
        {
            //throw new NotImplementedException();
        }

        public void Initialize()
        {
            //throw new NotImplementedException();
            Logger.Log("Options Menu Button Clicked", LogColors.GameImportant, 5);
            UpdateReady = true;
        }

        public void UnloadContent()
        {
            //throw new NotImplementedException();
        }

        public void Update(double dt)
        {
            //throw new NotImplementedException();
        }
    }
}
