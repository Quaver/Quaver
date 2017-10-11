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
    /// <summary>
    /// This is the GameState when the player is actively playing.
    /// </summary>
    internal partial class StatePlayScreen : GameStateBase
    {
        public StatePlayScreen()
        {
            //Important to assign a state to this class.
            CurrentState = State.PlayScreen;
        }

        //TEST
        private HitObject _testHitObject;

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void Initialize()
        {
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void LoadContent()
        {
            _testHitObject = new HitObject();
                /*
            _curNote = new Sprite(GraphicsDevice);
            _curNote.Image = content.Load<Texture2D>("TestImages/note_hitObject");
            _curNote.Alignment = Alignment.MidCenter;
            _curNote.Size = Vector2.One * 64;*/          
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            GameStateManager.Instance.UnloadContent();
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void Update(GameTime gameTime)
        {
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.Black);
            GameBase.SpriteBatch.Begin();
            //End
            _testHitObject.Draw();
            GameBase.SpriteBatch.End();
        }
    }
}
