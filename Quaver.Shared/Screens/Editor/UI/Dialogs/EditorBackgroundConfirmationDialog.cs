/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Dialogs;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Window;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs
{
    public class EditorBackgroundConfirmationDialog : ConfirmCancelDialog
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="file"></param>
        public EditorBackgroundConfirmationDialog(EditorScreen screen, string file)
            : base("Would you like to change the background?", (o, e) => OnConfirm(screen, file))
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="file"></param>
        private static void OnConfirm(EditorScreen screen, string file)
        {
            var view = screen.View as EditorScreenView;
            view?.FadeBackgroundOut();

            var fileName = Path.GetFileName(file);

            try
            {
                File.Copy(file, ConfigManager.SongDirectory + "/" + MapManager.Selected.Value.Directory + "/" + fileName, true);
            }
            catch (Exception)
            {
                // ignored
            }

            screen.WorkingMap.BackgroundFile = fileName;
            MapManager.Selected.Value.BackgroundPath = fileName;

            BackgroundHelper.Load(MapManager.Selected.Value);
            screen.Save();
        }
    }
}
