/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Config;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Settings.Elements
{
    public class SettingsScrollDirection4K : SettingsHorizontalSelector
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        public SettingsScrollDirection4K(SettingsDialog dialog)
            : base(dialog, "Scroll Direction 4K", ScrollDirectionToStringList(), OnChange, (int) ConfigManager.ScrollDirection4K.Value)
            => ConfigManager.ScrollDirection4K.ValueChanged += OnBindableValueChanged;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            ConfigManager.ScrollDirection4K.ValueChanged -= OnBindableValueChanged;
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="val"></param>
        /// <param name="index"></param>
        private static void OnChange(string val, int index) => ConfigManager.ScrollDirection4K.Value = (ScrollDirection) Enum.Parse(typeof(ScrollDirection), val);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> ScrollDirectionToStringList() => Enum.GetNames(typeof(ScrollDirection)).ToList();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBindableValueChanged(object sender, BindableValueChangedEventArgs<ScrollDirection> e) => Selector.SelectIndex((int) e.Value);
    }
}
