using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.CheckboxContainers
{
    public sealed class TestCheckboxContainerScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestCheckboxContainerScreen() => View = new TestCheckboxContainerScreenView(this);
    }
}