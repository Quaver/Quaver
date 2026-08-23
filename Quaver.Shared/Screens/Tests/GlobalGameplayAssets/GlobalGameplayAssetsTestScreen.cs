using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.GlobalGameplayAssets
{
    public sealed class GlobalGameplayAssetsTestScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public GlobalGameplayAssetsTestScreen() => View = new GlobalGameplayAssetsTestScreenView(this);
    }
}
