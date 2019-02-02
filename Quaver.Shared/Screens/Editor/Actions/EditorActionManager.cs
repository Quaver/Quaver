/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2019 Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Editor.Actions.Rulesets.Universal;
using Quaver.Shared.Screens.Editor.UI.Layering;
using Quaver.Shared.Screens.Editor.UI.Rulesets;

namespace Quaver.Shared.Screens.Editor.Actions
{
    public class EditorActionManager
    {
        /// <summary>
        ///     Stores a LIFO structure of actions to undo.
        /// </summary>
        public Stack<IEditorAction> UndoStack { get; } = new Stack<IEditorAction>();

        /// <summary>
        ///     Stores a LIFO structure of actions to redo.
        /// </summary>
        private Stack<IEditorAction> RedoStack { get; } = new Stack<IEditorAction>();

        /// <summary>
        ///     Performs a given action for the editor to take.
        /// </summary>
        /// <param name="action"></param>
        public void Perform(IEditorAction action)
        {
            action.Perform();
            UndoStack.Push(action);
            RedoStack.Clear();
        }

        /// <summary>
        ///     Undos the first action in the stack
        /// </summary>
        public void Undo()
        {
            if (UndoStack.Count == 0)
                return;

            var action = UndoStack.Pop();
            action.Undo();

            RedoStack.Push(action);
        }

        /// <summary>
        ///     Redos the first action in the stack
        /// </summary>
        public void Redo()
        {
            if (RedoStack.Count == 0)
                return;

            var action = RedoStack.Pop();
            action.Perform();

            UndoStack.Push(action);
        }

        /// <summary>
        ///     Sets the time in the track for the audio preview.
        /// </summary>
        /// <param name="ruleset"></param>
        /// <param name="workingMap"></param>
        /// <param name="time"></param>
        public void SetPreviewTime(EditorRuleset ruleset, Qua workingMap, int time) => Perform(new EditorActionSetPreviewTime(ruleset, workingMap, time));

        /// <summary>
        ///     Adds a layer to the map.
        /// </summary>
        /// <param name="workingMap"></param>
        /// <param name="compositor"></param>
        /// <param name="l"></param>
        public void AddLayer(Qua workingMap, EditorLayerCompositor compositor, EditorLayerInfo l) => Perform(new EditorActionAddLayer(workingMap, compositor, l));
    }
}