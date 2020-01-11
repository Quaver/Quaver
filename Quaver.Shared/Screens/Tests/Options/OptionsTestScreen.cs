using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Options
{
    public sealed class OptionsTestScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public OptionsTestScreen() => View = new OptionsTestScreenView(this);
    }
}