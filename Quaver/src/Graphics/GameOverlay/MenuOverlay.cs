using Microsoft.Xna.Framework;
using Quaver.Graphics.Sprite;
using Quaver.Graphics.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Graphics.GameOverlay
{
    class MenuOverlay : IGameOverlayComponent
    {
        private Boundary Boundary { get; set; }

        public void Draw()
        {
            Boundary.Draw();
        }

        public void Initialize()
        {
            Boundary = new Boundary();

            // bottom bar
            var bot = new Sprite.Sprite()
            {
                Size = new UDim2(0, 80, 1, 0),
                Alignment = Alignment.BotLeft,
                Tint = new Color(2, 0, 20),
                Parent = Boundary
            };

            // top bar
            var top = new Sprite.Sprite()
            {
                Size = new UDim2(0, 30, 1, 0),
                Alignment = Alignment.TopLeft,
                Tint = new Color(2, 0, 20),
                Parent = Boundary
            };

            // todoL: add actual content later
            // temp
            var pixel = new Sprite.Sprite()
            {
                Size = new UDim2(0, 1, 1, 0),
                Alignment = Alignment.BotLeft,
                Tint = Color.BlueViolet,
                Parent = top
            };

            pixel = new Sprite.Sprite()
            {
                Size = new UDim2(0, 1, 1, 0),
                Alignment = Alignment.TopLeft,
                Tint = Color.BlueViolet,
                Parent = bot
            };

            // place holder bottom
            var placeholder = new TextBoxSprite()
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
            placeholder = new TextBoxSprite()
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

        public void UnloadContent()
        {
            Boundary.Destroy();
        }

        public void Update(double dt)
        {
            Boundary.Update(dt);
        }
    }
}
