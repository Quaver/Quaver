/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Server.Common.Objects;

namespace Quaver.Shared.Screens.Download
{
    public class DownloadScreen : QuaverScreen
    {
        /// <summary>
        ///     Temporary cache of mapset banners throughout the game's life.
        /// </summary>
        public static Dictionary<int, Texture2D> MapsetBanners { get; } = new Dictionary<int, Texture2D>();

        public override QuaverScreenType Type { get; }
        public override UserClientStatus GetClientStatus() => throw new System.NotImplementedException();
    }
}