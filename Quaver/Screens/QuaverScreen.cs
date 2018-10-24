using System;
using System.Collections.Generic;
using System.Text;
using Quaver.Server.Common.Objects;
using Wobble.Screens;

namespace Quaver.Screens
{
    public abstract class QuaverScreen : Screen
    {
        /// <summary>
        ///     The type of screen this is.
        /// </summary>
        public abstract QuaverScreenType Type { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override ScreenView View { get; protected set; }

        /// <summary>
        ///   Creates a user client status for this screen.
        /// </summary>
        /// <returns></returns>
        public abstract UserClientStatus GetClientStatus();
    }
}
