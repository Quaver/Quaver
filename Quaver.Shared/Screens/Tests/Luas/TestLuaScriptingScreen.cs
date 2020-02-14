using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Luas
{
    public sealed class TestLuaScriptingScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestLuaScriptingScreen() => View = new TestLuaScriptingScreenView(this);
    }
}