/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

namespace Quaver.Shared.Screens.Editor.Actions
{
    public interface IEditorAction
    {
        /// <summary>
        ///     The type of action this is
        /// </summary>
        EditorActionType Type { get; }

        /// <summary>
        ///     Does performing logic for the action
        /// </summary>
        void Perform();

        /// <summary>
        ///     Undos the performing logic for the action
        /// </summary>
        void Undo();
    }
}
