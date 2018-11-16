using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects;
using Wobble.Screens;

namespace Quaver.Screens
{
    public abstract class QuaverScreen : Screen
    {
        /// <summary>
        ///     Called when the first update is called.
        /// </summary>
        private bool FirstUpdateCalled { get; set; }

        /// <summary>
        ///     The type of screen this is.
        /// </summary>
        public abstract QuaverScreenType Type { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override ScreenView View { get; protected set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (!FirstUpdateCalled)
            {
                OnFirstUpdate();
                FirstUpdateCalled = true;
            }

            base.Update(gameTime);
        }

        /// <summary>
        ///     Called the first update call.
        ///     Used for when the screen has already finished initializing
        /// </summary>
        public virtual void OnFirstUpdate()
        {

        }

        /// <summary>
        ///   Creates a user client status for this screen.
        /// </summary>
        /// <returns></returns>
        public abstract UserClientStatus GetClientStatus();
    }
}
