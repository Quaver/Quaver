using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.ResultsMulti
{
    public sealed class TestResultsMultiScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestResultsMultiScreen() => View = new TestResultsMultiScreenView(this);
    }
}