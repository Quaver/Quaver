using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Chat
{
    public sealed class TestChatScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestChatScreen() => View = new TestChatScreenView(this);
    }
}