using Microsoft.Xna.Framework;
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Graphics.GameOverlay.Chat
{
    class ChatManager : IGameOverlayComponent
    {
        private Boundary Boundary { get; set; }

        private Sprite.Sprite Sprite { get; set; }

        private TextInputField ChatInputField { get; set; }

        public void Initialize()
        {
            // Create main boundary
            Boundary = new Boundary();

            // Create background dimmer
            Sprite = new Graphics.Sprite.Sprite()
            {
                Size = new UDim2(0, 0, 1, 1),
                Alignment = Alignment.MidCenter,
                Alpha = 0.7f,
                Tint = Color.Black,
                Parent = Boundary
            };

            // Create input box
            ChatInputField = new TextInputField(new Vector2(400, 30), "Type something")
            {
                PosY = -200,
                Alignment = Alignment.BotLeft,
                Parent = Boundary
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

        public void Draw()
        {
            Boundary.Draw();
        }
    }
}
