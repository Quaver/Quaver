using Quaver.Online;
using Wobble.Screens;

namespace Quaver.Screens.Connecting
{
    public class ConnectingScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Connecting;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public ConnectingScreen()
        {
            OnlineManager.Login();
            View = new ConnectingScreenView(this);
        }
    }
}