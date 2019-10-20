using Microsoft.Xna.Framework;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Music.UI.ListenerList;
using Wobble;
using Wobble.Graphics;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.ListenerLists
{
    public class TestScreenListenerListView : ScreenView
    {
        public TestScreenListenerListView(Screen screen) : base(screen)
        {
        }

        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#2F2F2F"));
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}