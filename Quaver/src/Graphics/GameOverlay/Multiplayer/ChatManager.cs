using Microsoft.Xna.Framework;
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;
using Quaver.Graphics.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Logging;
using Quaver.Net;
using Quaver.Net.Packets;
using Quaver.Net.Packets.Types;
using Quaver.Net.Structures;

namespace Quaver.Graphics.GameOverlay.Multiplayer
{
    class ChatManager : IGameOverlayComponent
    {
        private Boundary Boundary { get; set; }

        private Sprite.Sprite Sprite { get; set; }

        private TextInputField ChatInputField { get; set; }

        //chat lines todo: use proper class for chatting
        private int ChatSize { get; } = 10;

        private string[] ChatValues { get; set; }

        private TextBoxSprite[] ChatTextBoxes { get; set; }

        public void Initialize()
        {
            // Create main boundary
            Boundary = new Boundary();

            // Create background dimmer
            Sprite = new Graphics.Sprite.Sprite()
            {
                Size = new UDim2(0, 0, 1, 1),
                Alignment = Alignment.MidCenter,
                Alpha = 0.8f,
                Tint = Color.Black,
                Parent = Boundary
            };

            // Create input box
            ChatInputField = new TextInputField(new Vector2(400, 30), "Type something", OnChatSubmit)
            {
                PosY = -200,
                Alignment = Alignment.BotLeft,
                Parent = Boundary
            };

            //create chat. todo: this is temporary
            ChatValues = new string[ChatSize];
            ChatTextBoxes = new TextBoxSprite[ChatSize];
            for (var i = 0; i < ChatSize; i++)
            {
                ChatTextBoxes[i] = new TextBoxSprite()
                {
                    Size = new UDim2(400, 30),
                    Position = new UDim2(10, -(230 + (i * 30))),
                    Alignment = Alignment.BotLeft,
                    TextAlignment = Alignment.MidLeft,
                    TextColor = Color.LightGreen,
                    Parent = Boundary
                };
            }
        }

        public void RecalculateWindow()
        {
            Boundary.Size = new UDim2(GameBase.WindowRectangle.Width, GameBase.WindowRectangle.Height);
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

        private void NewChatLine(object sender, EventArgs e)
        {
            for (var i = ChatSize -1; i > 0; i++)
            {
                ChatValues[i] = ChatValues[i - 1];
                ChatTextBoxes[i].Text = ChatValues[i];
            }
            ChatValues[0] = e.ToString(); //todo: chat event args
            ChatTextBoxes[0].Text = ChatValues[0];
        }

        /// <summary>
        ///     The callback function that will be called when the input has been submitted
        /// </summary>
        private static void OnChatSubmit(string text)
        {
            Logger.LogInfo($"Chat Message Sent: {text}", LogType.Runtime);
            new ChatMessagePacket(true, new ChatMessage {Channel = "#quaver", Text = text, Sender = FlamingoClient.Self.Username}).Send();
        }
    }
}
