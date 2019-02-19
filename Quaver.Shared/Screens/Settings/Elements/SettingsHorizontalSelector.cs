/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Form;

namespace Quaver.Shared.Screens.Settings.Elements
{
    public abstract class SettingsHorizontalSelector : SettingsItem
    {
        /// <summary>
        /// </summary>
        protected HorizontalSelector Selector { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        /// <param name="elements"></param>
        /// <param name="onChange"></param>
        /// <param name="selectedIndex"></param>
        protected SettingsHorizontalSelector(SettingsDialog dialog, string name, List<string> elements, Action<string, int> onChange, int selectedIndex)
            : base(dialog, name)
        {
            Selector = new HorizontalSelector(elements, new ScalableVector2(200, 26),
                Fonts.SourceSansProSemiBold, 13, FontAwesome.Get(FontAwesomeIcon.fa_chevron_pointing_to_the_left),
                FontAwesome.Get(FontAwesomeIcon.fa_right_chevron),
                new ScalableVector2(30, 22), 5, onChange, selectedIndex)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Tint = Color.Transparent,
                SelectedItemText =
                {
                    Tint = Color.White,
                    UsePreviousSpriteBatchOptions = true
                },
                ButtonSelectLeft = { UsePreviousSpriteBatchOptions = true },
                ButtonSelectRight = { UsePreviousSpriteBatchOptions = true }
            };

            Selector.X -= 68;
        }
    }
}
