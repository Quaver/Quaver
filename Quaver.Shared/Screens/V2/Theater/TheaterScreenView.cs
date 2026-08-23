namespace Quaver.Shared.Screens.V2.Theater
{
    internal sealed class TheaterScreenView : PlaceholderScreenView
    {
        public TheaterScreenView(TheaterScreen screen) : base(screen,
            config => config.Screens.Theater, "Screens.Theater",
            "Screen_V2_TheaterScreen")
        {
        }
    }
}
