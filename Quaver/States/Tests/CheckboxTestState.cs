using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Config;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Buttons.Selection;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Main;

namespace Quaver.States.Tests
{
    internal class CheckboxTestState : IGameState
    {
        /// <summary>
        ///     Test Screen
        /// </summary>
        public State CurrentState { get; set; } = State.Test;
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UpdateReady { get; set; }

        private Container Container { get; set; }

        /// <summary>
        ///     Navbar sprite
        /// </summary>
        private Nav Nav { get; set; }

        public void Initialize()
        {
            Container = new Container();
            Nav = Nav.CreateGlobalNavbar();
            Nav.Initialize(this);
            
            var ds4k = new Checkbox(ConfigManager.DownScroll4K, new Vector2(20, 20))
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
            };

            var ds4ktext = new SpriteText()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                PosX = ds4k.PosX + 120,
                Text = "Toggle DownScroll for 4K",
                Font = Fonts.Medium12
            };
            
            var ds7k = new Checkbox(ConfigManager.DownScroll7K, new Vector2(20, 20))
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                PosY = ds4k.PosY + 50
            };

            var ds7ktext = new SpriteText()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                PosX = ds4k.PosX + 120,
                PosY = ds7k.PosY,
                Text = "Toggle DownScroll for 7K",
                Font = Fonts.Medium12
            };
            
            UpdateReady = true;
        }

        public void UnloadContent()
        {
            Nav.UnloadContent();
            Container.Destroy();
        }

        public void Update(double dt)
        {
            Nav.Update(dt);
            Container.Update(dt);
        }

        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.DarkOliveGreen);
            GameBase.SpriteBatch.Begin();
            
            Nav.Draw();
            Container.Draw();
            
            GameBase.SpriteBatch.End();
        }
    }
}