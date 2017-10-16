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
        private int _testNoteSize = 67;

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
            _testBoundary.Size = new Vector2(_testNoteSize*4, GameBase.WindowSize.Y);
            _testBoundary.Alignment = Alignment.TopCenter;
            _testBoundary.UpdateRect();
            for (int i = 0; i < 4; i++)
            {
                _testHitObject[i] = new HitObject();

                _testHitObject[i]._HoldBodySprite = new Sprite()
                {
                    Image = GameBase.LoadedSkin.NoteHoldBody,
                    Size = Vector2.One * _testNoteSize,
                    Position = new Vector2(i* _testNoteSize, _testNoteSize/2f),
                    Alignment = Alignment.TopLeft,
                    Parent = _testBoundary
                };

                _testHitObject[i]._HoldEndSprite = new Sprite()
                {
                    Image = GameBase.LoadedSkin.NoteHoldEnd,
                    Size = Vector2.One * _testNoteSize,
                    Position = new Vector2(i * _testNoteSize, _testNoteSize),
                    Alignment = Alignment.TopLeft,
                    Parent = _testBoundary
                };

                _testHitObject[i]._HitBodySprite = new Sprite()
                {
                    Image = GameBase.LoadedSkin.NoteHitObject1,
                    Position = new Vector2(i * _testNoteSize, 0),
                    Size = Vector2.One * _testNoteSize,
                    Alignment = Alignment.TopLeft,
                    Parent = _testBoundary
                };

                _testHitObject[i]._HoldBodySprite.UpdateRect();
                _testHitObject[i]._HoldEndSprite.UpdateRect();
                _testHitObject[i]._HitBodySprite.UpdateRect();
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
