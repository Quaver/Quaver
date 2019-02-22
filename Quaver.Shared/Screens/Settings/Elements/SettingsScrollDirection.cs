/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Config;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Settings.Elements
{
    public class SettingsScrollDirection : SettingsHorizontalSelector
    {
        private Bindable<ScrollDirection> BoundScrollDirection { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        /// <param name="binded"></param>
        public SettingsScrollDirection(SettingsDialog dialog, string name, Bindable<ScrollDirection> binded)
            : base(dialog, name, ScrollDirectionToStringList(), (val, index) => OnChange(val, binded), (int)binded.Value)
        {
            BoundScrollDirection = binded;
            BoundScrollDirection.ValueChanged += OnBindableValueChanged;
        }

        /// <summary>
        ///     Is called when the UI is changed.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="index"></param>
        /// <param name="binded"></param>
        private static void OnChange(string val, Bindable<ScrollDirection> binded) => binded.Value = (ScrollDirection)Enum.Parse(typeof(ScrollDirection), val);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> ScrollDirectionToStringList() => Enum.GetNames(typeof(ScrollDirection)).ToList();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            BoundScrollDirection.ValueChanged -= OnBindableValueChanged;
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBindableValueChanged(object sender, BindableValueChangedEventArgs<ScrollDirection> e) => Selector.SelectIndex((int)e.Value);
    }
}
