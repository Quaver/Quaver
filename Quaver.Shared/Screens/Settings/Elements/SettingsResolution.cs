/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Config;
using Wobble.Graphics;
using Wobble.Graphics.UI.Form;
using Wobble.Window;

namespace Quaver.Shared.Screens.Settings.Elements
{
    public class SettingsResolution : SettingsHorizontalSelector
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        public SettingsResolution(SettingsDialog dialog, string name)
            : base(dialog, name, GetElements(), (val, i) => OnChange(dialog, val, i), GetSelectedIndex())
        {
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetElements()
        {
            var resolutions = new List<Point>
            {
                new Point(1024, 576),
                new Point(1152, 648),
                new Point(1280, 720),
                new Point(1366, 768),
                new Point(1600, 900),
                new Point(1920, 1080),
                new Point(2560, 1440)
            };

            var userRes = new Point(ConfigManager.WindowWidth.Value, ConfigManager.WindowHeight.Value);

            // Put the user's resolution where it should go (ordered by width)
            if (!resolutions.Any(x => x.X == userRes.X && x.Y == userRes.Y))
                resolutions.Insert(resolutions.FindLastIndex(x => x.X > userRes.X), userRes);

            var resolutionList = new List<string>();
            resolutions.ForEach(x => resolutionList.Add($"{x.X}x{x.Y}"));

            return resolutionList;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex() =>
            GetElements().FindIndex(x => x == $"{ConfigManager.WindowWidth.Value}x{ConfigManager.WindowHeight.Value}");

        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="val"></param>
        /// <param name="index"></param>
        private static void OnChange(SettingsDialog dialog, string val, int index)
        {
            // Parse the value
            var split = val.Split('x');
            var resolution = new Point(int.Parse(split[0]), int.Parse(split[1]));

            dialog.NewQueuedScreenResolution = resolution;
        }
    }
}
