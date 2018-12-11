/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Audio;
using Quaver.Shared.Graphics;

namespace Quaver.Shared.Screens.Edit.UI
{
    internal class EditorSongTimeDisplay : NumberDisplay
    {
        public EditorSongTimeDisplay(NumberDisplayType type, string startingValue, Vector2 imageScale)
                                                    : base(type, startingValue, imageScale) => X = -TotalWidth;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var currTime = new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds((int) AudioEngine.Track.Time);
            Value = currTime.ToString("mm:ss.fff");

            base.Update(gameTime);
        }
    }
}
