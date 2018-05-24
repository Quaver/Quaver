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
using Quaver.Graphics.UniversalDim;
using Quaver.Logging;
using Quaver.Main;

namespace Quaver.Graphics.GameOverlay.Multiplayer
{
    class ChatManager : IGameOverlayComponent
    {
        private Container Container { get; set; }

        private Sprites.Sprite Sprite { get; set; }

        private TextInputField ChatInputField { get; set; }

        //chat lines todo: use proper class for chatting
        private int ChatSize { get; } = 10;

        private string[] ChatValues { get; set; }

        private QuaverSpriteText[] ChatQuaverTextBoxs { get; set; }

        public void Initialize()
        {
            // Create main boundary
            Container = new Container();

            // Create background dimmer
            Sprite = new Sprites.Sprite()
            {
                Size = new UDim2D(0, 0, 1, 1),
                Alignment = Alignment.MidCenter,
                Alpha = 0.8f,
                Tint = Color.Black,
                Parent = Container
            };

            // Create input box
            ChatInputField = new TextInputField(new Vector2(400, 30), "Type something", OnChatSubmit)
            {
                PosY = -200,
                Alignment = Alignment.BotLeft,
                Parent = Container
            };

            //create chat. todo: this is temporary
            ChatValues = new string[ChatSize];
            ChatQuaverTextBoxs = new QuaverSpriteText[ChatSize];
            for (var i = 0; i < ChatSize; i++)
            {
                ChatQuaverTextBoxs[i] = new QuaverSpriteText()
                {
                    Size = new UDim2D(400, 30),
                    Position = new UDim2D(10, -(230 + (i * 30))),
                    Alignment = Alignment.BotLeft,
                    TextAlignment = Alignment.MidLeft,
                    TextColor = Color.LightGreen,
                    Parent = Container
                };
            }
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

        public void Draw()
        {
            Container.Draw();
        }

        private void NewChatLine(object sender, EventArgs e)
        {
            for (var i = ChatSize -1; i > 0; i++)
            {
                ChatValues[i] = ChatValues[i - 1];
                ChatQuaverTextBoxs[i].Text = ChatValues[i];
            }
            ChatValues[0] = e.ToString(); //todo: chat event args
            ChatQuaverTextBoxs[0].Text = ChatValues[0];
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
