using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Notifications
{
    public sealed class TestNotificationScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestNotificationScreen() => View = new TestNotificationScreenView(this);
    }
}