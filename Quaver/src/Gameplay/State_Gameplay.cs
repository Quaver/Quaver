using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.src.GameState;

namespace Quaver.src.Gameplay
{
    partial class State_Gameplay : GameStateBase
    {
        public State_Gameplay(GraphicsDevice graphicsDevice) :base(graphicsDevice)
        {
            //Important to set the current state of this GameState
            CurrentState = State.PlayScreen;
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
