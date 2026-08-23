namespace Quaver.Shared.Screens.V2.Results
{
    internal sealed class ResultsScreenView : PlaceholderScreenView
    {
        public ResultsScreenView(ResultsScreen screen) : base(screen,
            config => config.Screens.Results, "Screens.Results",
            "Screen_V2_ResultsScreen")
        {
        }
    }
}
