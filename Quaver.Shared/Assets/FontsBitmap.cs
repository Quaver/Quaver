/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Text;
using MonoGame.Extended.BitmapFonts;
using Wobble;

namespace Quaver.Shared.Assets
{
    public static class FontsBitmap
    {
        public static BitmapFont Exo2Regular { get; private set; }
        public static BitmapFont AllerRegular { get; private set; }
        public static BitmapFont MuliRegular { get; private set; }
        public static BitmapFont MuliBold { get; private set; }

        public static void Load()
        {
            Exo2Regular = GameBase.Game.Content.Load<BitmapFont>("exo2-regular");
            AllerRegular = GameBase.Game.Content.Load<BitmapFont>("aller-regular");
            MuliRegular = GameBase.Game.Content.Load<BitmapFont>("muli");
            MuliBold = GameBase.Game.Content.Load<BitmapFont>("muli-bold");
        }
    }
}
