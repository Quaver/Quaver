using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Overlays.Chatting;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Tests.UI.Borders;
using Wobble;
using Wobble.Graphics;
using Wobble.Input;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Chat
{
    public class TestChatScreenView : ScreenView
    {
        public OnlineChat OnlineChatOverlay { get; }

        public OnlineHub Hub { get; }

        public TestChatScreenView(Screen screen) : base(screen)
        {
            Hub = new OnlineHub()
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Y = MenuBorder.HEIGHT
            };

            OnlineChatOverlay = new OnlineChat()
            {
                Parent = Container,
                Alignment = Alignment.BotLeft
            };

            new TestMenuBorderHeader()
            {
                Parent = Container
            };
        }

        public override void Update(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.D2))
            {
                OnlineChatOverlay.Open();
                Hub.Open();
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.D1))
            {
                OnlineChatOverlay.Close();
                Hub.Close();
            }

            Container?.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#0f0f0f"));
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}