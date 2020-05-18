using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.ModifierSelectors
{
    public sealed class TestModifierSelectorScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public TestModifierSelectorScreen() => View = new TestModifierSelectorScreenView(this);
    }
}