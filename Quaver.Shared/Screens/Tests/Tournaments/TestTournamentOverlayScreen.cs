using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Tournaments
{
    public sealed class TestTournamentOverlayScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestTournamentOverlayScreen() => View = new TestTournamentOverlayScreenView(this);
    }
}