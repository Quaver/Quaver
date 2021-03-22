using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.AutoMods
{
    public sealed class AutoModTestScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public AutoModTestScreen() => View = new AutoModTestScreenView(this);
    }
}