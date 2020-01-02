using Microsoft.Xna.Framework;
using Quaver.Shared.Graphics.Overlays.Volume;
using Quaver.Shared.Helpers;
using Wobble;
using Wobble.Graphics;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Volume
{
    public class TestVolumeControlScreenView : ScreenView
    {
        public TestVolumeControlScreenView(Screen screen) : base(screen)
        {
            new VolumeControl()
            {
                Parent = Container,
                Alignment = Alignment.TopRight
            };
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