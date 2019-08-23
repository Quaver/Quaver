using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.DrawableMaps
{
    public class TestDrawableMapScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestDrawableMapScreen() => View = new TestDrawableMapScreenView(this);
    }
}