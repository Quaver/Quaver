using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.DrawableLeaderboardScores
{
    public sealed class TestScreenDrawableLeaderboardScore : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestScreenDrawableLeaderboardScore() => View = new TestScreenDrawableLeaderboardScoreView(this);
    }
}