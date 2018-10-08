using Quaver.API.Enums;
using Quaver.Modifiers;
using Quaver.Online;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => null;
    }
}