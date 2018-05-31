using System;
using Microsoft.Xna.Framework;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.Sprites;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.States.Enums;

namespace Quaver.States.Tests
{
    public class AnimatableSpriteTestScreen : IGameState
    {
        public State CurrentState { get; set; }
        public bool UpdateReady { get; set; }
        internal Container Container { get; set; }
        internal AnimatableSprite TestSprite { get; set; }
        internal Sprite Test { get; set; }

        /// <summary>
        ///     Navbar sprite
        /// </summary>
        private Nav Nav { get; set; }

        public void Initialize()
        {
            Container = new Container();
            TestSprite = new AnimatableSprite(GameBase.QuaverUserInterface.TestSpritesheet)
            {
                Parent = Container,
                Alignment = Alignment.MidRight,
                Size = new UDim2D(64, 128)
            };
            
            TestSprite.StartLoop(LoopDirection.Backward, 24);

            Test = new Sprite()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = new UDim2D(200, 200),
                Image = GameBase.QuaverUserInterface.JudgementOverlay,
            };
            
            
            Nav = new Nav();
            Nav.Initialize(this);
            UpdateReady = true;
        }

        public void UnloadContent()
        {
            Container.Destroy();
            Nav.UnloadContent();
        }

        public void Update(double dt)
        {
            Container.Update(dt);
            Nav.Update(dt);
        }

        public void Draw()
        {
            GameBase.SpriteBatch.Begin();
            Container.Draw();
            Nav.Draw();
            GameBase.SpriteBatch.End();
        }
    }
}