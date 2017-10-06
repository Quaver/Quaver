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
    internal partial class State_Gameplay : GameStateBase
    {

        //TEST
        private Texture2D _TestImage;

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

            //TEST
            _TestImage = content.Load<Texture2D>("TestImages/arpiapic");
        }

        public override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            GameStateManager.Instance.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            // Draw sprites here
            spriteBatch.Draw(_TestImage, new Rectangle(0, 0, 400, 400), Color.White);

            //End
            spriteBatch.End();
        }

    }
}
