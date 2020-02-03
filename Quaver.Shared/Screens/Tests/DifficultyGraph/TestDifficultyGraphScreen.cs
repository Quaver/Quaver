using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.DifficultyGraph
{
    public sealed class TestDifficultyGraphScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestDifficultyGraphScreen() => View = new TestDifficultyGraphScreenView(this);
    }
}