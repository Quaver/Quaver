/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.Shared.Screens.Settings;
using Quaver.Shared.Screens.Settings.Elements;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.Metadata
{
    public class EditorMetadataGameMode : EditorMetadataHorizontalSelector
    {
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        /// <param name="elements"></param>
        /// <param name="onChange"></param>
        /// <param name="selectedIndex"></param>
        public EditorMetadataGameMode(SettingsDialog dialog, string name, List<string> elements,
            Action<string, int> onChange, int selectedIndex) : base(dialog, name, elements, onChange, selectedIndex, null) => throw new NotImplementedException();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="mode"></param>
        /// <param name="saveValue"></param>
        public EditorMetadataGameMode(Drawable parent, string name, GameMode mode, Action<string> saveValue)
            : base(parent, name, GetGameModesList(), OnChange, mode == GameMode.Keys4 ? 0 : 1, saveValue)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="val"></param>
        /// <param name="index"></param>
        private static void OnChange(string val, int index)
        {
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetGameModesList() => new List<string>()
        {
            "4 Keys",
            "7 Keys"
        };
    }
}
