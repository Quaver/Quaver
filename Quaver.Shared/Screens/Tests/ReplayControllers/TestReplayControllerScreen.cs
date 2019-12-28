using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.ReplayControllers
{
    public sealed class TestReplayControllerScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestReplayControllerScreen() => View = new TestReplayControllerScreenView(this);
    }
}