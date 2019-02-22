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
    public class SettingsFpsLimiter : SettingsHorizontalSelector
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        public SettingsFpsLimiter(SettingsDialog dialog)
            : base(dialog, "Frame Limiter Type", FpsLimiterTypesToStringList(), OnChange, (int) ConfigManager.FpsLimiterType.Value)
            => ConfigManager.FpsLimiterType.ValueChanged += OnBindableValueChanged;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            ConfigManager.FpsLimiterType.ValueChanged -= OnBindableValueChanged;
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="val"></param>
        /// <param name="index"></param>
        private static void OnChange(string val, int index) => ConfigManager.FpsLimiterType.Value = (FpsLimitType) Enum.Parse(typeof(FpsLimitType), val);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> FpsLimiterTypesToStringList() => Enum.GetNames(typeof(FpsLimitType)).ToList();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBindableValueChanged(object sender, BindableValueChangedEventArgs<FpsLimitType> e) => Selector.SelectIndex((int) e.Value);
    }
}
