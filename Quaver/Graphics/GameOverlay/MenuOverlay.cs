using Microsoft.Xna.Framework;
using Quaver.Graphics.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;

namespace Quaver.Graphics.GameOverlay
{
    class MenuOverlay : IGameOverlayComponent
    {
        private QuaverContainer QuaverContainer { get; set; }

        public void Draw()
        {
            QuaverContainer.Draw();
        }

        public void Initialize()
        {
            QuaverContainer = new QuaverContainer();

            // bottom bar
            var bot = new Sprites.QuaverSprite()
            {
                Size = new UDim2(0, 80, 1, 0),
                Alignment = Alignment.BotLeft,
                Tint = new Color(0, 4, 16),
                Parent = QuaverContainer
            };

            // top bar
            var top = new Sprites.QuaverSprite()
            {
                Size = new UDim2(0, 30, 1, 0),
                Alignment = Alignment.TopLeft,
                Tint = new Color(0, 4, 16),
                Parent = QuaverContainer
            };

            // todoL: add actual content later
            // temp
            var pixel = new Sprites.QuaverSprite()
            {
                Size = new UDim2(0, 1, 1, 0),
                Alignment = Alignment.BotLeft,
                Tint = Color.DeepSkyBlue,
                Parent = top
            };

            pixel = new Sprites.QuaverSprite()
            {
                Size = new UDim2(0, 1, 1, 0),
                Alignment = Alignment.TopLeft,
                Tint = Color.DeepSkyBlue,
                Parent = bot
            };

            // place holder bottom
            var placeholder = new QuaverTextbox()
            {
                Position = new UDim2(5, 5),
                Size = new UDim2(400, 40),
                Text = "Put player info / player stats / game client related stuff here",
                TextColor = Color.White,
                TextBoxStyle = TextBoxStyle.OverflowSingleLine,
                TextAlignment = Alignment.MidLeft,
                Parent = bot
            };

            // place holder top
            placeholder = new QuaverTextbox()
            {
                Position = new UDim2(5, 5),
                Size = new UDim2(400, 20),
                Text = "Put menu title e.g: 'MAIN MENU' / overlay toggle / buttons / search and stuff here",
                TextColor = Color.White,
                TextBoxStyle = TextBoxStyle.OverflowSingleLine,
                TextAlignment = Alignment.MidLeft,
                Parent = top
            };
        }

        public void RecalculateWindow()
        {
            QuaverContainer.Size = new UDim2(GameBase.WindowRectangle.Width, GameBase.WindowRectangle.Height);
        }

        public void UnloadContent()
        {
            QuaverContainer.Destroy();
        }

        public void Update(double dt)
        {
            QuaverContainer.Update(dt);
        }
    }
}
