/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Wobble;
using Wobble.Assets;

namespace Quaver.Shared.Assets
{
    public static class CompetitiveBadgeHelper
    {
        public static Texture2D Get(CompetitveBadge badge)
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            return AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get($"Quaver.Resources/Textures/UI/Competitive/comp-{(int) badge}.png"));
        }
    }
}
