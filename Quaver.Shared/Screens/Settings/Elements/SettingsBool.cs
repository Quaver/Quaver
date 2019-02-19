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
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Form;

namespace Quaver.Shared.Screens.Settings.Elements
{
    public class SettingsBool : SettingsItem
    {
        /// <summary>
        ///     The binded value for this settings item.
        /// </summary>
        private Bindable<bool> Bindable { get; }

        /// <summary>
        /// </summary>
        private HorizontalSelector Selector { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        /// <param name="bindable"></param>
        public SettingsBool(SettingsDialog dialog, string name, Bindable<bool> bindable) : base(dialog, name)
        {
            Bindable = bindable;

            Selector = new HorizontalSelector(new List<string>
                {
                    "No",
                    "Yes",
                }, new ScalableVector2(200, 26), Fonts.Exo2Medium, 13, FontAwesome.Get(FontAwesomeIcon.fa_chevron_pointing_to_the_left),
                FontAwesome.Get(FontAwesomeIcon.fa_right_chevron),
                new ScalableVector2(30, 22), 5, OnSelectorChanged, Convert.ToInt32(Bindable.Value))
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Tint = Color.Transparent,
                SelectedItemText =
                {
                    Tint = Color.White,
                    UsePreviousSpriteBatchOptions = true
                },
                ButtonSelectLeft =
                {
                    UsePreviousSpriteBatchOptions = true
                },
                ButtonSelectRight =
                {
                    UsePreviousSpriteBatchOptions = true
                }
            };

            Selector.X -= 68;

            Bindable.ValueChanged += OnBindableValueChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            Bindable.ValueChanged -= OnBindableValueChanged;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="val"></param>
        /// <param name="index"></param>
        private void OnSelectorChanged(string val, int index) => Bindable.Value = Convert.ToBoolean(index);

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBindableValueChanged(object sender, BindableValueChangedEventArgs<bool> e) => Selector.SelectIndex(Convert.ToInt32(e.Value));
    }
}
