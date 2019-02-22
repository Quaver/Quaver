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
using Quaver.Shared.Screens.Settings;
using Wobble.Graphics;
using Wobble.Graphics.UI.Form;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.Metadata
{
    public class EditorMetadataHorizontalSelector : EditorMetadataItem
    {
        /// <summary>
        /// </summary>
        protected HorizontalSelector Selector { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="elements"></param>
        /// <param name="onChange"></param>
        /// <param name="selectedIndex"></param>
        /// <param name="saveValue"></param>
        protected EditorMetadataHorizontalSelector(Drawable parent, string name, List<string> elements, Action<string, int> onChange, int selectedIndex,
            Action<string> saveValue)
            : base(parent, name, elements[selectedIndex], saveValue)
        {
            Selector = new HorizontalSelector(elements, new ScalableVector2(196, 26),
                Fonts.SourceSansProSemiBold, 13, FontAwesome.Get(FontAwesomeIcon.fa_chevron_pointing_to_the_left),
                FontAwesome.Get(FontAwesomeIcon.fa_right_chevron),
                new ScalableVector2(18, 18), 5, onChange, selectedIndex)
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

            Selector.X -= 34;
        }

        public override string GetValue() => Selector.Options[Selector.SelectedIndex];

        public override bool HasChanged() => InitialValue != Selector.Options[Selector.SelectedIndex];
    }
}
