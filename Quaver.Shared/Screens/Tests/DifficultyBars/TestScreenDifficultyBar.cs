using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.DifficultyBars
{
    public class TestScreenDifficultyBar : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestScreenDifficultyBar() => View = new TestScreenDifficultyBarView(this);
    }
}