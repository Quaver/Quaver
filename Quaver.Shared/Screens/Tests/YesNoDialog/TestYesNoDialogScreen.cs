using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.YesNoDialog
{
    public sealed class TestYesNoDialogScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestYesNoDialogScreen() => View = new TestYesNoDialogScreenView(this);
    }
}