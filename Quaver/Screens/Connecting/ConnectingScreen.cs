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
        public ConnectingScreen() => View = new ConnectingScreenView(this);
    }
}