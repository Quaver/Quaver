using Quaver.Shared.Screens.Tests.FilterPanel;

namespace Quaver.Shared.Screens.Tests.DrawableMapsetsMultiple
{
    public sealed class TestMapsetsMultipleScreen : FilterPanelTestScreen
    {
        public TestMapsetsMultipleScreen() => View = new TestMapsetsMultipleScreenView(this);   
    }
}