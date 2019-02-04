/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Graphics.Dialogs;
using Quaver.Shared.Screens.Editor.UI.Rulesets;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs
{
    public class EditorLayerDeleteConfirmationDialog : ConfirmCancelDialog
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="ruleset"></param>
        /// <param name="layer"></param>
        public EditorLayerDeleteConfirmationDialog(EditorRuleset ruleset, EditorLayerInfo layer)
            : base($"Deleting this layer will also remove ALL objects inside of it. Confirm?",
                (o, e) => OnConfirm(ruleset, layer))
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="ruleset"></param>
        /// <param name="layer"></param>
        private static void OnConfirm(EditorRuleset ruleset, EditorLayerInfo layer)
        {
            var view = ruleset.Screen.View as EditorScreenView;
            ruleset.ActionManager.RemoveLayer(ruleset.WorkingMap, view?.LayerCompositor, layer);
        }
    }
}
