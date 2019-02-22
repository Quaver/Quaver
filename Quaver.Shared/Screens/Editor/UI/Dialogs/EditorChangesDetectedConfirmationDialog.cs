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
using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Dialogs;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Select;
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
   public class EditorChangesDetectedConfirmationDialog : ConfirmCancelDialog
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="file"></param>
        public EditorChangesDetectedConfirmationDialog(QuaverScreen screen, string file)
            : base("Detected outside changes to the .qua file. Would you like to reload the editor?", (o, e) => OnConfirm(screen, file))
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="file"></param>
        private static void OnConfirm(QuaverScreen screen, string file)
        {
            for (var i = DialogManager.Dialogs.Count - 1; i >= 0; i--)
            {
                DialogManager.Dialogs[i].Destroy();
                DialogManager.Dialogs.Remove(DialogManager.Dialogs[i]);
            }

            DialogManager.Update(new GameTime());

            if (!MapDatabaseCache.MapsToUpdate.Contains(MapManager.Selected.Value))
                MapDatabaseCache.MapsToUpdate.Add(MapManager.Selected.Value);

            screen.Exit(() =>
            {
                try
                {
                    return new EditorScreen(Qua.Parse(file, false));
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, "Failed to reload editor. Is your .qua file invalid?");
                    return new SelectScreen();
                }
            });
        }
    }
}
