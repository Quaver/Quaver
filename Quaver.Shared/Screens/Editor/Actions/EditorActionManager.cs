/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using System.Linq;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Editor.Actions.Rulesets.Universal;
using Quaver.Shared.Screens.Editor.UI.Layering;
using Quaver.Shared.Screens.Editor.UI.Rulesets;

namespace Quaver.Shared.Screens.Editor.Actions
{
    public abstract class EditorActionManager
    {
        /// <summary>
        /// </summary>
        protected EditorScreen Screen { get; }

        /// <summary>
        ///     Stores a LIFO structure of actions to undo.
        /// </summary>
        public Stack<IEditorAction> UndoStack { get; } = new Stack<IEditorAction>();

        /// <summary>
        ///     Stores a LIFO structure of actions to redo.
        /// </summary>
        public Stack<IEditorAction> RedoStack { get; } = new Stack<IEditorAction>();

        /// <summary>
        ///     The last action the user performed before saving
        /// </summary>
        public IEditorAction LastSaveAction { get; set; }

        /// <summary>
        ///    Detects if the user has made changes to the map before saving.
        /// </summary>
        public bool HasUnsavedChanges => UndoStack.Count != 0  && UndoStack.Peek() != LastSaveAction || UndoStack.Count == 0 && LastSaveAction != null;

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public EditorActionManager(EditorScreen screen) => Screen = screen;

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

        /// <summary>
        ///     Removes a layer from the map
        /// </summary>
        /// <param name="workingMap"></param>
        /// <param name="compositor"></param>
        /// <param name="l"></param>
        public void RemoveLayer(Qua workingMap, EditorLayerCompositor compositor, EditorLayerInfo l)
            => Perform(new EditorActionRemoveLayer(workingMap, compositor, l));

        /// <summary>
        ///     Edits a given layer
        /// </summary>
        /// <param name="compositor"></param>
        /// <param name="layer"></param>
        /// <param name="name"></param>
        /// <param name="color"></param>
        public void EditLayer(EditorLayerCompositor compositor, EditorLayerInfo layer, string name, string color)
            => Perform(new EditorActionEditLayer(compositor, layer, name, color));
    }
}
