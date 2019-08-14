using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Dropdowns
{
    public sealed class DropdownTestScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public DropdownTestScreen() => View = new DropdownTestScreenView(this);
    }
}