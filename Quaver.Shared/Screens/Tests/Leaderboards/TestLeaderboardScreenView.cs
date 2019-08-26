using Quaver.Shared.Screens.Selection.UI.Leaderboard;
using Quaver.Shared.Screens.Tests.FilterPanel;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Tests.Leaderboards
{
    public class TestLeaderboardScreenView : FilterPanelTestScreenView
    {
        public TestLeaderboardScreenView(FilterPanelTestScreen screen) : base(screen)
        {
            new LeaderboardContainer()
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                X = 50,
                Y = FilterPanel.Y + FilterPanel.Height + 18
            };
        }
    }
}