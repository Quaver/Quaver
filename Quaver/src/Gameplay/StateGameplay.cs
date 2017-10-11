﻿using System;
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

namespace Quaver.Gameplay
{
    /// <summary>
    /// This is the GameState when the player is actively playing.
    /// </summary>
    internal partial class StateGameplay : GameStateBase
    {

        public StateGameplay(GraphicsDevice graphicsDevice) :base(graphicsDevice)
        {
            //Important to assign a state to this class.
            CurrentState = State.TestScreen;
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void Initialize()
        {
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public override void LoadContent(ContentManager content)
        {

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
        public override void Draw(SpriteBatch spriteBatch)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            //End
            spriteBatch.End();
        }
    }
}
