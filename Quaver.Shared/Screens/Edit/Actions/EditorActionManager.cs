using System;
using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.HitObjects;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Flip;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Move;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Place;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.PlaceBatch;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Remove;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.RemoveBatch;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Resize;
using Quaver.Shared.Screens.Edit.Actions.Hitsounds.Add;
using Quaver.Shared.Screens.Edit.Actions.Hitsounds.Remove;

namespace Quaver.Shared.Screens.Edit.Actions
{
    public class EditorActionManager : IDisposable
    {
        /// <summary>
        /// </summary>
        private Qua WorkingMap { get; }

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
        ///     Event invoked when a HitObject has been placed
        /// </summary>
        public event EventHandler<EditorHitObjectPlacedEventArgs> HitObjectPlaced;

        /// <summary>
        ///     Event invoked when a HitObject has been removed
        /// </summary>
        public event EventHandler<EditorHitObjectRemovedEventArgs> HitObjectRemoved;

        /// <summary>
        ///     Event invoked when a long note has been resized
        /// </summary>
        public event EventHandler<EditorLongNoteResizedEventArgs> LongNoteResized;

        /// <summary>
        ///     Event invoked when a batch of hitobjects have been removed
        /// </summary>
        public event EventHandler<EditorHitObjectBatchRemovedEventArgs> HitObjectBatchRemoved;

        /// <summary>
        ///     Event invoked when a batch of hitobjects have been placed
        /// </summary>
        public event EventHandler<EditorHitObjectBatchPlacedEventArgs> HitObjectBatchPlaced;

        /// <summary>
        ///     Event invoked when a batch of hitobjects have been flipped
        /// </summary>
        public event EventHandler<EditorHitObjectsFlippedEventArgs> HitObjectsFlipped;

        /// <summary>
        ///     Event invoked when a batch of hitobjects have been moved
        /// </summary>
        public event EventHandler<EditorHitObjectsMovedEventArgs> HitObjectsMoved;

        /// <summary>
        ///     Event invoked when a hitsound has been added to a group of objects
        /// </summary>
        public event EventHandler<EditorHitsoundAddedEventArgs> HitsoundAdded;

        /// <summary>
        ///     Event invoked when a hitsound has been removed from a group of objects
        /// </summary>
        public event EventHandler<EditorHitSoundRemovedEventArgs> HitsoundRemoved;

        /// <summary>
        /// </summary>
        /// <param name="workingMap"></param>
        public EditorActionManager(Qua workingMap) => WorkingMap = workingMap;

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
        ///     TODO: Handle selected layer
        /// </summary>
        /// <param name="lane"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public HitObjectInfo PlaceHitObject(int lane, int startTime, int endTime = 0, int layer = 0)
        {
            var hitObject = new HitObjectInfo
            {
                Lane = lane,
                StartTime = startTime,
                EndTime = endTime,
                EditorLayer = layer
            };

            Perform(new EditorActionPlaceHitObject(this, WorkingMap, hitObject));

            return hitObject;
        }

        /// <summary>
        ///     Removes a HitObject from the map
        /// </summary>
        /// <param name="h"></param>
        public void RemoveHitObject(HitObjectInfo h) => Perform(new EditorActionRemoveHitObject(this, WorkingMap, h));

        /// <summary>
        ///     Resizes a hitobject/long note to a given time
        /// </summary>
        /// <param name="h"></param>
        /// <param name="time"></param>
        public void ResizeLongNote(HitObjectInfo h, int originalTime, int time)
            => Perform(new EditorActionResizeLongNote(this, WorkingMap, h, originalTime, time));

        /// <summary>
        ///     Triggers an event of a specific action type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        public void TriggerEvent(EditorActionType type, EventArgs args)
        {
            switch (type)
            {
                case EditorActionType.PlaceHitObject:
                    HitObjectPlaced?.Invoke(this, (EditorHitObjectPlacedEventArgs) args);
                    break;
                case EditorActionType.RemoveHitObject:
                    HitObjectRemoved?.Invoke(this, (EditorHitObjectRemovedEventArgs) args);
                    break;
                case EditorActionType.ResizeLongNote:
                    LongNoteResized?.Invoke(this, (EditorLongNoteResizedEventArgs) args);
                    break;
                case EditorActionType.RemoveHitObjectBatch:
                    HitObjectBatchRemoved?.Invoke(this, (EditorHitObjectBatchRemovedEventArgs) args);
                    break;
                case EditorActionType.PlaceHitObjectBatch:
                    HitObjectBatchPlaced?.Invoke(this, (EditorHitObjectBatchPlacedEventArgs) args);
                    break;
                case EditorActionType.FlipHitObjects:
                    HitObjectsFlipped?.Invoke(this, (EditorHitObjectsFlippedEventArgs) args);
                    break;
                case EditorActionType.MoveHitObjects:
                    HitObjectsMoved?.Invoke(this, (EditorHitObjectsMovedEventArgs) args);
                    break;
                case EditorActionType.AddHitsound:
                    HitsoundAdded?.Invoke(this, (EditorHitsoundAddedEventArgs) args);
                    break;
                case EditorActionType.RemoveHitsound:
                    HitsoundRemoved?.Invoke(this, (EditorHitSoundRemovedEventArgs) args);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Dispose()
        {
            HitObjectPlaced = null;
            HitObjectRemoved = null;
            LongNoteResized = null;
            HitObjectBatchRemoved = null;
            HitObjectBatchPlaced = null;
            HitObjectsFlipped = null;
            HitObjectsMoved = null;
            HitsoundAdded = null;
            HitsoundRemoved = null;
        }
    }
}