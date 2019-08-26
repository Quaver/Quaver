using Quaver.Shared.Screens.Tests.FilterPanel;
using Quaver.Shared.Screens.Tests.Leaderboards;
using Quaver.Shared.Screens.Tests.MapScrollContainers;

namespace Quaver.Shared.Screens.Tests.LeaderboardWithMaps
{
    public sealed class TestLeaderboardWithMapsScreen : FilterPanelTestScreen
    {
        public TestLeaderboardWithMapsScreen() => View = new TestLeaderboardWithMapsScreenView(this);
    }
}