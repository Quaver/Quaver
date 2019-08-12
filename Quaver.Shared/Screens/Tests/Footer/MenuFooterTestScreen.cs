using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Footer
{
    public sealed class MenuFooterTestScreen : Screen
    {
        public override ScreenView View { get; protected set; }
        
        public MenuFooterTestScreen() => View = new MenuFooterTestScreenView(this);
    }
}
