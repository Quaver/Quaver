using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Footer
{
    public sealed class MenuBorderTestScreen : Screen
    {
        public override ScreenView View { get; protected set; }
        
        public MenuBorderTestScreen() => View = new MenuBorderTestScreenView(this);
    }
}
