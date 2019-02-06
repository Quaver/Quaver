/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Quaver.Shared.Graphics.Dialogs;
using Quaver.Shared.Screens.Settings;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs
{
    public class EditorUploadConfirmationDialog : ConfirmCancelDialog
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorUploadConfirmationDialog() : base("Are you sure you want to upload your mapset?", OnConfirm)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnConfirm(object sender, EventArgs e)
        {
            DialogManager.Show(new EditorUploadMapsetDialog());
        }
    }
}