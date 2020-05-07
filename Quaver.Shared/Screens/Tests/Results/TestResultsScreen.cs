using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Results
{
    public sealed class TestResultsScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestResultsScreen() => View = new TestResultsScreenView(this);
    }
}