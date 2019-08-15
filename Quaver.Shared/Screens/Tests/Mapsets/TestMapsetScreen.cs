using Quaver.Shared.Screens.Tests.FilterPanel;

namespace Quaver.Shared.Screens.Tests.Mapsets
{
    public class TestMapsetScreen : FilterPanelTestScreen
    {
        public TestMapsetScreen() : base(true) => View = new TestMapsetScreenView(this);
    }
}