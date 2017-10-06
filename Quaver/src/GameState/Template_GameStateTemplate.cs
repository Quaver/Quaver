using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Quaver.src.GameState
{
    internal partial class State_Template : GameStateBase
    {
        public State_Template(GraphicsDevice graphicsDevice) :base(graphicsDevice)
        {
            //Important to set the current state of this GameState
            CurrentState = State.MainMenu;
        }
        public override void Initialize()
        {
        }

        public override void LoadContent(ContentManager content)
        {
        }

        public override void UnloadContent()
        {
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
        }
        
    }
}
