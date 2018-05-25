using Quaver.GameState;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
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
                Alignment = Alignment.MidCenter,
                Size = new UDim2D(128, 128)
            };
            
            TestSprite.StartLoop(LoopDirection.Forward, 60);
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