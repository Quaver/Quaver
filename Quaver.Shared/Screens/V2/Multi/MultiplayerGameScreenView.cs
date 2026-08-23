namespace Quaver.Shared.Screens.V2.Multi
{
    internal sealed class MultiplayerGameScreenView : PlaceholderScreenView
    {
        public MultiplayerGameScreenView(MultiplayerGameScreen screen) : base(screen,
            config => config.Screens.MultiplayerGame, "Screens.MultiplayerGame",
            "Screen_V2_MultiplayerGameScreen")
        {
        }
    }
}
