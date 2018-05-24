using Microsoft.Xna.Framework;
using Quaver.Graphics.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Main;

namespace Quaver.Graphics.GameOverlay
{
    class MenuOverlay : IGameOverlayComponent
    {
        private Container Container { get; set; }

        public void Draw()
        {
            Container.Draw();
        }

        public void Initialize()
        {
            Container = new Container();

            // bottom bar
            var bot = new Sprites.Sprite()
            {
                Size = new UDim2D(0, 80, 1, 0),
                Alignment = Alignment.BotLeft,
                Tint = new Color(0, 4, 16),
                Parent = Container
            };

            // top bar
            var top = new Sprites.Sprite()
            {
                Size = new UDim2D(0, 30, 1, 0),
                Alignment = Alignment.TopLeft,
                Tint = new Color(0, 4, 16),
                Parent = Container
            };

            // todoL: add actual content later
            // temp
            var pixel = new Sprites.Sprite()
            {
                Size = new UDim2D(0, 1, 1, 0),
                Alignment = Alignment.BotLeft,
                Tint = Color.DeepSkyBlue,
                Parent = top
            };

            pixel = new Sprites.Sprite()
            {
                Size = new UDim2D(0, 1, 1, 0),
                Alignment = Alignment.TopLeft,
                Tint = Color.DeepSkyBlue,
                Parent = bot
            };

            // place holder bottom
            var placeholder = new QuaverSpriteText()
            {
                Position = new UDim2D(5, 5),
                Size = new UDim2D(400, 40),
                Text = "Put player info / player stats / game client related stuff here",
                TextColor = Color.White,
                TextBoxStyle = TextBoxStyle.OverflowSingleLine,
                TextAlignment = Alignment.MidLeft,
                Parent = bot
            };

            // place holder top
            placeholder = new QuaverSpriteText()
            {
                Position = new UDim2D(5, 5),
                Size = new UDim2D(400, 20),
                Text = "Put menu title e.g: 'MAIN MENU' / overlay toggle / buttons / search and stuff here",
                TextColor = Color.White,
                TextBoxStyle = TextBoxStyle.OverflowSingleLine,
                TextAlignment = Alignment.MidLeft,
                Parent = top
            };
        }

        public void RecalculateWindow()
        {
            Container.Size = new UDim2D(GameBase.WindowRectangle.Width, GameBase.WindowRectangle.Height);
        }

        public void UnloadContent()
        {
            Container.Destroy();
        }

        public void Update(double dt)
        {
            Container.Update(dt);
        }
    }
}
