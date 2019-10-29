using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.OnlineHubDownloads
{
    public sealed class TestOnlineHubDownloadsScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestOnlineHubDownloadsScreen() => View = new TestOnlineHubDownloadsScreenView(this);
    }
}