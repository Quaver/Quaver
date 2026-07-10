using Quaver.Server.Client.Objects;
using Quaver.Shared.Screens;
using Wobble;

namespace Quaver.Shared.Screens.Tests.ButtonPerformance
{
    public sealed class ButtonPerformanceTestScreen : QuaverScreen
    {
        public override QuaverScreenType Type { get; } = QuaverScreenType.Menu;

        public ButtonPerformanceTestScreen() => View = new ButtonPerformanceTestScreenView(this);

        public override void OnFirstUpdate()
        {
            GameBase.Game.GlobalUserInterface.Cursor.Show(1);
            GameBase.Game.GlobalUserInterface.Cursor.Alpha = 1;

            base.OnFirstUpdate();
        }

        public override UserClientStatus GetClientStatus() => null;
    }
}
