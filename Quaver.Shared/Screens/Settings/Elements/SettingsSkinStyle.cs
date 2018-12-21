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

namespace Quaver.Shared.Screens.Settings.Elements
{
    public class SettingsSkinStyle : SettingsHorizontalSelector
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        public SettingsSkinStyle(SettingsDialog dialog, string name)
            : base(dialog, name, GetSelectorElements(), (val, i) => OnChange(dialog, val, i), GetSelectedIndex())
        {
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetSelectorElements() => Enum.GetNames(typeof(SkinStyles)).ToList();

        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="val"></param>
        /// <param name="index"></param>
        private static void OnChange(SettingsDialog dialog, string val, int index)
            => dialog.NewQueuedSkinStyle = (SkinStyles)Enum.Parse(typeof(SkinStyles), val);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex()
            => GetSelectorElements().FindIndex(x => x.ToString() == ConfigManager.SkinStyle.Value.ToString());
    }
}
