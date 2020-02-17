using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Editor
{
    public sealed class TestEditorScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestEditorScreen() => View = new TestEditorScreenView(this);
    }
}