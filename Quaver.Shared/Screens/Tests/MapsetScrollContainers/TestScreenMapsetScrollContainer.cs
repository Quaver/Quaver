using Quaver.Shared.Screens.Tests.FilterPanel;

namespace Quaver.Shared.Screens.Tests.MapsetScrollContainers
{
    public sealed class TestScreenMapsetScrollContainer : FilterPanelTestScreen
    {
        public TestScreenMapsetScrollContainer() : base(true) => View = new TestScreenMapsetScrollContainerView(this);
    }
}