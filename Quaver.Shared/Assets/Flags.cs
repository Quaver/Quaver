/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework.Graphics;
using Wobble;
using Wobble.Assets;
using Wobble.Managers;

namespace Quaver.Shared.Assets
{
    public static class Flags
    {
        public static Texture2D Get(string countryName) => TextureManager.Load($"Quaver.Resources/Textures/UI/Flags/{countryName}.png");
    }
}
