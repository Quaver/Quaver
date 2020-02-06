using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Profiles
{
    public sealed class TestUserProfileContainerScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestUserProfileContainerScreen() => View = new TestUserProfileContainerScreenView(this);
    }
}