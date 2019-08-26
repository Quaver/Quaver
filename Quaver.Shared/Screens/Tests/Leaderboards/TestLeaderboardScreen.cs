using Quaver.Shared.Screens.Tests.FilterPanel;

namespace Quaver.Shared.Screens.Tests.Leaderboards
{
    public sealed class TestLeaderboardScreen : FilterPanelTestScreen
    {
        public TestLeaderboardScreen() => View = new TestLeaderboardScreenView(this);
    }
}