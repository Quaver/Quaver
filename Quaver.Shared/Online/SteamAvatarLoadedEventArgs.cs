/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Quaver.Shared.Online
{
    public class SteamAvatarLoadedEventArgs : EventArgs
    {
        public ulong SteamId { get; }
        public Texture2D Texture { get; }

        public SteamAvatarLoadedEventArgs(ulong steamId, Texture2D tex)
        {
            SteamId = steamId;
            Texture = tex;
        }
    }
}
