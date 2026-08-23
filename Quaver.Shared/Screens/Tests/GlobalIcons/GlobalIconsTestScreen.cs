using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.GlobalIcons
{
    public sealed class GlobalIconsTestScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public GlobalIconsTestScreen() => View = new GlobalIconsTestScreenView(this);
    }
}
