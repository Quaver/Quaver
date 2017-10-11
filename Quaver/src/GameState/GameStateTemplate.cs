using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Quaver.GameState
{
    internal class GameStateTemplate : GameStateBase
    {
        public GameStateTemplate()
        {
            //Important to assign a state to this class.
            CurrentState = State.MainMenu;
        }

        public override void Initialize()
        {
        }

        public override void LoadContent()
        {
        }

        public override void UnloadContent()
        {
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw()
        {
        }      
    }
}
