namespace Quaver.Shared.Screens.V2.Multiplayer
{
    internal sealed class MultiplayerScreenView : PlaceholderScreenView
    {
        public MultiplayerScreenView(MultiplayerScreen screen) : base(screen,
            config => config.Screens.Multiplayer, "Screens.Multiplayer",
            "Screen_V2_MultiplayerScreen")
        {
        }
    }
}
