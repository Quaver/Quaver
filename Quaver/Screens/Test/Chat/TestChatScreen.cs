using Wobble.Screens;

namespace Quaver.Screens.Test.Chat
{
    public class TestChatScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public TestChatScreen() => View = new TestChatScreenView(this);
    }
}