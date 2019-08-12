using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.FilterPanel
{
    public sealed class FilterPanelTestScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public FilterPanelTestScreen() => View = new FilterPanelTestScreenView(this);
    }
}