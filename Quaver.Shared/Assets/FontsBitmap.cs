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
using Wobble.Managers;

namespace Quaver.Shared.Assets
{
    public static class FontsBitmap
    {
        public static BitmapFont Exo2Regular => FontManager.LoadBitmapFont($"exo2-regular");
        public static BitmapFont AllerRegular => FontManager.LoadBitmapFont("aller-regular");
        public static BitmapFont MuliRegular => FontManager.LoadBitmapFont("muli");
        public static BitmapFont MuliBold => FontManager.LoadBitmapFont($"muli-bold");
        public static BitmapFont CodeProRegular => FontManager.LoadBitmapFont($"code-pro");
        public static BitmapFont CodeProBold => FontManager.LoadBitmapFont($"code-pro-bold");
        public static BitmapFont GothamRegular => FontManager.LoadBitmapFont("gotham");
        public static BitmapFont GothamBold => FontManager.LoadBitmapFont($"gotham-bold");
    }
}
