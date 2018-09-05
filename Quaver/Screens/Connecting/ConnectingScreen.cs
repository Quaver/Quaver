using Quaver.Online;
using Wobble.Screens;

namespace Quaver.Screens.Connecting
{
    public class ConnectingScreen : Screen
    {
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