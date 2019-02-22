/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Quaver.Shared.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Download.UI.Filter
{
    public class RadioFilter : Sprite
    {
        /// <summary>
        /// </summary>
        private List<string> Options { get; }

        /// <summary>
        /// </summary>
        private string Name { get; }

        /// <summary>
        /// </summary>
        private int SelectedIndex { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText FilterName { get; set; }

        /// <summary>
        /// </summary>
        private QuaverHorizontalSelector Selector { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="name"></param>
        /// <param name="options"></param>
        /// <param name="onChange"></param>
        /// <param name="selectedIndex"></param>
        public RadioFilter(DownloadSearchFilters filters, string name, List<string> options, Action<string, int> onChange, int selectedIndex = 0)
        {
            Width = filters.Width;
            Name = name;
            SelectedIndex = selectedIndex;
            Alpha = 0;
            Options = options;

            CreateFilterName();
            CreateRadioButtons(onChange);

            Height = FilterName.Height;
        }

        /// <summary>
        /// </summary>
        private void CreateFilterName() => FilterName = new SpriteText(Fonts.SourceSansProBold, Name, 13) { Parent = this };

        /// <summary>
        /// </summary>
        private void CreateRadioButtons(Action<string, int> onChange) => Selector = new QuaverHorizontalSelector(Options, new ScalableVector2(200, 16),
            Fonts.SourceSansProBold, 13, new ScalableVector2(20, 16), onChange, SelectedIndex)
        {
            Parent = this,
            Alignment = Alignment.TopRight,
            X = -50
        };
    }
}
