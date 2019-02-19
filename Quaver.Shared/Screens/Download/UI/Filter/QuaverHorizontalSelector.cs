/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.UI.Form;

namespace Quaver.Shared.Screens.Download.UI.Filter
{
    public class QuaverHorizontalSelector : HorizontalSelector
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="options"></param>
        /// <param name="selectorSize"></param>
        /// <param name="selectorFont"></param>
        /// <param name="fontSize"></param>
        /// <param name="buttonSize"></param>
        /// <param name="onChange"></param>
        /// <param name="selectedIndex"></param>
        public QuaverHorizontalSelector(List<string> options, ScalableVector2 selectorSize, string selectorFont, int fontSize,  ScalableVector2 buttonSize,
            Action<string, int> onChange, int selectedIndex = 0) : base(options, selectorSize, selectorFont, fontSize,
            FontAwesome.Get(FontAwesomeIcon.fa_chevron_pointing_to_the_left),
                FontAwesome.Get(FontAwesomeIcon.fa_right_chevron), buttonSize,
                5, onChange, selectedIndex)
        {
            Tint = Color.Transparent;
            SelectedItemText.Tint = Color.White;
        }
    }
}
