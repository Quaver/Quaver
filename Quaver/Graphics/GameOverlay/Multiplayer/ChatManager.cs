using Microsoft.Xna.Framework;
using Quaver.Graphics.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Logging;

namespace Quaver.Graphics.GameOverlay.Multiplayer
{
    class ChatManager : IGameOverlayComponent
    {
        private QuaverContainer QuaverContainer { get; set; }

        private Sprites.QuaverSprite QuaverSprite { get; set; }

        private QuaverTextInputField ChatInputField { get; set; }

        //chat lines todo: use proper class for chatting
        private int ChatSize { get; } = 10;

        private string[] ChatValues { get; set; }

        private TextBoxSprite[] ChatTextBoxes { get; set; }

        public void Initialize()
        {
            // Create main boundary
            QuaverContainer = new QuaverContainer();

            // Create background dimmer
            QuaverSprite = new Sprites.QuaverSprite()
            {
                Size = new UDim2(0, 0, 1, 1),
                Alignment = Alignment.MidCenter,
                Alpha = 0.8f,
                Tint = Color.Black,
                Parent = QuaverContainer
            };

            // Create input box
            ChatInputField = new QuaverTextInputField(new Vector2(400, 30), "Type something", OnChatSubmit)
            {
                PosY = -200,
                Alignment = Alignment.BotLeft,
                Parent = QuaverContainer
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
                    Parent = QuaverContainer
                };
            }
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

        public void Draw()
        {
            QuaverContainer.Draw();
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
        }
    }
}
