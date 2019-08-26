using Quaver.Shared.Screens.Tests.FilterPanel;

namespace Quaver.Shared.Screens.Tests.MapScrollContainers
{
    public class TestScreenMapScrollContainer : FilterPanelTestScreen
    {
        public TestScreenMapScrollContainer() => View = new TestScreenMapScrollContainerView(this);
    }
}