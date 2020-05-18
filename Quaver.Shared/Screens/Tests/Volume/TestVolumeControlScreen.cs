using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Volume
{
    public sealed class TestVolumeControlScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestVolumeControlScreen() => View = new TestVolumeControlScreenView(this);
    }
}