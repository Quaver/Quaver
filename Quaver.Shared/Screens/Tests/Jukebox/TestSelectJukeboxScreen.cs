using Quaver.Shared.Screens.Tests.FilterPanel;

namespace Quaver.Shared.Screens.Tests.Jukebox
{
    public class TestSelectJukeboxScreen : FilterPanelTestScreen
    {
        public TestSelectJukeboxScreen() => View = new TestSelectJukeboxScreenView(this);
    }
}