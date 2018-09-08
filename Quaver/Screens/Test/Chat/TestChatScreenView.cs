using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Online;
using Quaver.Server.Client.Structures;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Form;
using Wobble.Screens;

namespace Quaver.Screens.Test.Chat
{
    public class TestChatScreenView : ScreenView
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestChatScreenView(Screen screen) : base(screen)
        {
            var channel = new Textbox(TextboxStyle.SingleLine, new ScalableVector2(400, 35), Fonts.Exo2Regular24, "#quaver",
                "Channel", 0.65f)
            {
                Parent = Container,
                Image = UserInterface.UsernameSelectionTextbox,
                Alignment = Alignment.TopCenter,
                Y = 100
            };

            var message = new Textbox(TextboxStyle.SingleLine, new ScalableVector2(400, 35), Fonts.Exo2Regular24, "",
                "Message", 0.65f)
            {
                Parent = Container,
                Image = UserInterface.UsernameSelectionTextbox,
                Alignment = Alignment.TopCenter,
                Y = 200
            };

            var button = new TextButton(UserInterface.BlankBox, Fonts.Exo2Regular24, "Send", 0.65f, (o, e) =>
            {
                OnlineManager.Client.SendMessage(channel.RawText, message.RawText);
            })
            {
                Parent = Container,
                Tint = Color.Black,
                Alignment = Alignment.TopCenter,
                Size = new ScalableVector2(250, 50),
                Y = 300
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.SpriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}