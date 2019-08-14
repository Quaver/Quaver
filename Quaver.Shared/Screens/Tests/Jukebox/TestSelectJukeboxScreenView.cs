using Quaver.Shared.Screens.Selection.Components;
using Quaver.Shared.Screens.Tests.FilterPanel;

namespace Quaver.Shared.Screens.Tests.Jukebox
{
    public class TestSelectJukeboxScreenView : FilterPanelTestScreenView
    {
        public TestSelectJukeboxScreenView(FilterPanelTestScreen screen) : base(screen)
        {
            new SelectJukebox() {Parent = Container};
        }
    }
}