namespace Quaver.Shared.Screens.V2.Importing
{
    internal sealed class ImportingScreenView : PlaceholderScreenView
    {
        public ImportingScreenView(ImportingScreen screen) : base(screen,
            config => config.Screens.Importing, "Screens.Importing",
            "Screen_V2_ImportingScreen")
        {
        }
    }
}
