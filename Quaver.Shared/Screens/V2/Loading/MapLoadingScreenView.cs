namespace Quaver.Shared.Screens.V2.Loading
{
    internal sealed class MapLoadingScreenView : PlaceholderScreenView
    {
        public MapLoadingScreenView(MapLoadingScreen screen) : base(screen,
            config => config.Screens.Loading, "Screens.Loading",
            "Screen_V2_MapLoadingScreen")
        {
        }
    }
}
