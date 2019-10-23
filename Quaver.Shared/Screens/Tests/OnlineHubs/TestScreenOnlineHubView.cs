using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Tests.UI.Borders;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Input;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.Tests.OnlineHubs
{
    public class TestScreenOnlineHubView : ScreenView
    {
        private MenuBorder Header { get; }

        private MenuBorder Footer { get; }

        private OnlineHub Hub { get; }

        public TestScreenOnlineHubView(Screen screen) : base(screen)
        {
            Header = new TestMenuBorderHeader()
            {
                Parent = Container
            };

            Footer = new TestMenuBorderFooter()
            {
                Parent = Container,
                Alignment = Alignment.BotLeft
            };

            new Sprite()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height - Header.Height * 2),
                Tint = Color.Black,
                Alpha = 0.75f
            };

            Hub = new OnlineHub()
            {
                Parent = Container,
                Alignment = Alignment.MidRight,
            };

            Hub.X = Hub.Width;

            Hub.Open();
        }

        public override void Update(GameTime gameTime)
        {
            Container?.Update(gameTime);

            if (KeyboardManager.IsUniqueKeyPress(Keys.D0))
            {
                if (Hub.IsOpen)
                    Hub.Close();
                else
                    Hub.Open();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#2f2f2f"));
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}