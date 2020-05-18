using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Border
{
    public sealed class MenuBorderTestScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public MenuBorderTestScreen() => View = new MenuBorderTestScreenView(this);
    }
}
