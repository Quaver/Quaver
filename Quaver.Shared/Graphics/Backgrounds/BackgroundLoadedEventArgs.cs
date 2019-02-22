/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Database.Maps;

namespace Quaver.Shared.Graphics.Backgrounds
{
    public class BackgroundLoadedEventArgs : EventArgs
    {
        public Map Map { get; }
        public Texture2D Texture { get; }

        public BackgroundLoadedEventArgs(Map map, Texture2D tex)
        {
            Map = map;
            Texture = tex;
        }
    }
}
