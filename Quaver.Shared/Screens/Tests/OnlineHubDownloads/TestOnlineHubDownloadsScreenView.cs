using Microsoft.Xna.Framework;
using Quaver.Shared.Helpers;
using Wobble;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.OnlineHubDownloads
{
    public class TestOnlineHubDownloadsScreenView : ScreenView
    {
        public TestOnlineHubDownloadsScreenView(Screen screen) : base(screen)
        {
            /*new DrawableDownload(null, new MapsetDownload)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
            };*/
        }

        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#181818"));
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}