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
        private HitObject[] _testHitObject = new HitObject[4];
        private Boundary _testBoundary;

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
            _testBoundary = new Boundary();
            for (int i = 0; i < 4; i++)
            {
                _testHitObject[i] = new HitObject();
                _testHitObject[i]._HoldBodySprite.Parent = _testBoundary;
                _testHitObject[i]._HoldEndSprite.Parent = _testBoundary;
                _testHitObject[i]._HitBodySprite.Parent = _testBoundary;
            }    
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
            _testBoundary.Draw();
        }
    }
}
