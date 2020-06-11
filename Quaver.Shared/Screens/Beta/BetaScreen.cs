/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Screens.Main;
using Wobble.Input;

namespace Quaver.Shared.Screens.Beta
{
    public sealed class BetaScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Alpha;

        /// <summary>
        ///     The amount of time the screen has been active.
        /// </summary>
        private double TimeScreenActive { get; set; }

        /// <summary>
        /// </summary>
        public BetaScreen() => View = new BetaScreenView(this);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            TimeScreenActive += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (TimeScreenActive >= 10000 && !Exiting)
                Exit(() => new MainMenuScreen(), 300);

            HandleInput(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandleInput(GameTime gameTime)
        {
            if (Exiting)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Enter))
                Exit(() => new MainMenuScreen(), 200);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => null;
    }
}
