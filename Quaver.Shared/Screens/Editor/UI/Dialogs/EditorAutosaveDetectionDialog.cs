/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.IO;
using Quaver.API.Maps;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Dialogs;
using Quaver.Shared.Graphics.Notifications;
using Wobble;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs
{
    public class EditorAutosaveDetectionDialog : ConfirmCancelDialog
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorAutosaveDetectionDialog()
            : base("The last time you edited this map, the game crashed. Would you like to recover your work?", OnConfirm, OnCancel)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnConfirm(object sender, EventArgs e)
        {
            var game = GameBase.Game as QuaverGame;

            try
            {
                var path = $"{ConfigManager.SongDirectory}/{MapManager.Selected.Value.Directory}/{MapManager.Selected.Value.Path}.autosave";
                var realPath = path.Replace(".autosave", "");

                File.Delete(realPath);
                File.Copy(path, realPath, true);
                File.Delete(path);

                if (!MapDatabaseCache.MapsToUpdate.Contains(MapManager.Selected.Value))
                    MapDatabaseCache.MapsToUpdate.Add(MapManager.Selected.Value);

                var qua = Qua.Parse(realPath, false);
                game?.CurrentScreen.Exit(() => new EditorScreen(qua));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Error, "Could not load autosave map");
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnCancel(object sender, EventArgs e)
        {
            // Delete the autosave file
            var path = $"{ConfigManager.SongDirectory}/{MapManager.Selected.Value.Directory}/{MapManager.Selected.Value.Path}.autosave";
            File.Delete(path);
        }
    }
}
