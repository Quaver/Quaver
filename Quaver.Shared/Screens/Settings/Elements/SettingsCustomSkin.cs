/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Quaver.Shared.Config;
using Quaver.Shared.Skinning;

namespace Quaver.Shared.Screens.Settings.Elements
{
    public class SettingsCustomSkin : SettingsHorizontalSelector
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        public SettingsCustomSkin(SettingsDialog dialog, string name)
            : base(dialog, name, GetCustomSkinList(), (val, index) => OnChange(dialog, val, index), GetSelectedIndex())
        {
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetCustomSkinList()
        {
            var skins = new List<string> { "Default Skin" };

            var skinDirectories = Directory.GetDirectories(ConfigManager.SkinDirectory.Value);
            skins.AddRange(skinDirectories.Select(dir => new DirectoryInfo(dir).Name));
            skins.Sort();

            return skins;
        }

        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="val"></param>
        /// <param name="index"></param>
        private static void OnChange(SettingsDialog dialog, string val, int index)
        {
            val = new DirectoryInfo(val).Name;
            var skin = ConfigManager.Skin.Value;

            switch (val)
            {
                // Check if the user already has the default skin enabled and switched back to it.
                // User wants to choose the default skin
                case "Default Skin" when string.IsNullOrEmpty(skin):
                    SkinManager.NewQueuedSkin = null;
                    break;
                // User is selecting a custom skin
                case "Default Skin" when !string.IsNullOrEmpty(skin):
                    SkinManager.NewQueuedSkin = "";
                    break;
                default:
                    if (val != skin)
                        SkinManager.NewQueuedSkin = val;
                    break;
            }
        }

        /// <summary>
        ///     Finds the index of the selected skin
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex()
        {
            var skins = GetCustomSkinList();
            var skin = ConfigManager.Skin.Value;
            return string.IsNullOrEmpty(skin) ? 0 : skins.FindIndex(x => x == skin);
        }
    }
}
