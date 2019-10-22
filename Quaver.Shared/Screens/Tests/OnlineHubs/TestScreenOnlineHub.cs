using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.OnlineHubs
{
    public sealed class TestScreenOnlineHub : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestScreenOnlineHub() => View = new TestScreenOnlineHubView(this);
    }
}