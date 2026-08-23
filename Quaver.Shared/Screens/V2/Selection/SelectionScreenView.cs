using Quaver.Shared.Screens.V2.UI;

namespace Quaver.Shared.Screens.V2.Selection
{
    internal sealed class SelectionScreenView : PlaceholderScreenView
    {
        public SelectionScreenView(SelectionScreen screen) : base(screen,
            config => config.Screens.Selection, "Screens.Selection",
            "Screen_V2_SelectionScreen")
        {
        }

        protected override void ConfigureNavigation(ScreenNavigation navigation)
        {
            navigation.ShowApplicationTopBar(QuaverScreenType.Select);
            navigation.ShowSelectionFooter();
        }
    }
}
