using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Enums;
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
using Quaver.Shared.Screens.Edit.Actions.Layers.Colors;
using Quaver.Shared.Screens.Edit.Actions.Layers.Create;
using Quaver.Shared.Screens.Edit.Actions.Layers.Remove;
using Quaver.Shared.Screens.Edit.Actions.Layers.Rename;
using Quaver.Shared.Screens.Edit.Actions.SV.Add;
using Quaver.Shared.Screens.Edit.Actions.SV.Remove;

namespace Quaver.Shared.Screens.Edit.Actions
{
    [MoonSharpUserData]
    public class EditorActionManager : IDisposable
    {
        /// <summary>
        /// </summary>
        private Qua WorkingMap { get; }

        /// <summary>
        ///     Stores a LIFO structure of actions to undo.
        /// </summary>
        [MoonSharpVisible(false)]
        public Stack<IEditorAction> UndoStack { get; } = new Stack<IEditorAction>();

        /// <summary>
        ///     Stores a LIFO structure of actions to redo.
        /// </summary>
        [MoonSharpVisible(false)]
        public Stack<IEditorAction> RedoStack { get; } = new Stack<IEditorAction>();

        /// <summary>
        ///     The last action the user performed before saving
        /// </summary>
        [MoonSharpVisible(false)]
        public IEditorAction LastSaveAction { get; set; }

        /// <summary>
        ///    Detects if the user has made changes to the map before saving.
        /// </summary>
        [MoonSharpVisible(false)]
        public bool HasUnsavedChanges => UndoStack.Count != 0  && UndoStack.Peek() != LastSaveAction || UndoStack.Count == 0 && LastSaveAction != null;

        /// <summary>
        ///     Event invoked when a HitObject has been placed
        /// </summary>
        [MoonSharpVisible(false)]
        public event EventHandler<EditorHitObjectPlacedEventArgs> HitObjectPlaced;

        /// <summary>
        ///     Event invoked when a HitObject has been removed
        /// </summary>
        [MoonSharpVisible(false)]
        public event EventHandler<EditorHitObjectRemovedEventArgs> HitObjectRemoved;

        /// <summary>
        ///     Event invoked when a long note has been resized
        /// </summary>
        [MoonSharpVisible(false)]
        public event EventHandler<EditorLongNoteResizedEventArgs> LongNoteResized;

        /// <summary>
        ///     Event invoked when a batch of hitobjects have been removed
        /// </summary>
        [MoonSharpVisible(false)]
        public event EventHandler<EditorHitObjectBatchRemovedEventArgs> HitObjectBatchRemoved;

        /// <summary>
        ///     Event invoked when a batch of hitobjects have been placed
        /// </summary>
        [MoonSharpVisible(false)]
        public event EventHandler<EditorHitObjectBatchPlacedEventArgs> HitObjectBatchPlaced;

        /// <summary>
        ///     Event invoked when a batch of hitobjects have been flipped
        /// </summary>
        [MoonSharpVisible(false)]
        public event EventHandler<EditorHitObjectsFlippedEventArgs> HitObjectsFlipped;

        /// <summary>
        ///     Event invoked when a batch of hitobjects have been moved
        /// </summary>
        [MoonSharpVisible(false)]
        public event EventHandler<EditorHitObjectsMovedEventArgs> HitObjectsMoved;

        /// <summary>
        ///     Event invoked when a hitsound has been added to a group of objects
        /// </summary>
        [MoonSharpVisible(false)]
        public event EventHandler<EditorHitsoundAddedEventArgs> HitsoundAdded;

        /// <summary>
        ///     Event invoked when a hitsound has been removed from a group of objects
        /// </summary>
        [MoonSharpVisible(false)]
        public event EventHandler<EditorHitSoundRemovedEventArgs> HitsoundRemoved;

        /// <summary>
        ///     Event invoked when a layer has been created
        /// </summary>
        [MoonSharpVisible(false)]
        public event EventHandler<EditorLayerCreatedEventArgs> LayerCreated;

        /// <summary>
        ///     Event invoked when a layer has been deleted
        /// </summary>
        [MoonSharpVisible(false)]
        public event EventHandler<EditorLayerRemovedEventArgs> LayerDeleted;

        /// <summary>
        ///     Event invoked when a layer has been renamed
        /// </summary>
        [MoonSharpVisible(false)]
        public event EventHandler<EditorLayerRenamedEventArgs> LayerRenamed;

        /// <summary>
        ///     Event invoked when a layer's color has been changed
        /// </summary>
        [MoonSharpVisible(false)]
        public event EventHandler<EditorLayerColorChangedEventArgs> LayerColorChanged;

        /// <summary>
        ///     Event invoked when a scroll velocity has been added to the map
        /// </summary>
        [MoonSharpVisible(false)]
        public event EventHandler<EditorScrollVelocityAddedEventArgs> ScrollVelocityAdded;

        /// <summary>
        ///     Event invoked when a scroll velocity has been removed from the map
        /// </summary>
        [MoonSharpVisible(false)]
        public event EventHandler<EditorScrollVelocityRemovedEventArgs> ScrollVelocityRemoved;

        /// <summary>
        /// </summary>
        /// <param name="workingMap"></param>
        [MoonSharpVisible(false)]
        public EditorActionManager(Qua workingMap) => WorkingMap = workingMap;

        /// <summary>
        ///     Performs a given action for the editor to take.
        /// </summary>
        /// <param name="action"></param>
        [MoonSharpVisible(false)]
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
        /// </summary>
        /// <param name="lane"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="layer"></param>
        /// <param name="hitsounds"></param>
        public HitObjectInfo PlaceHitObject(int lane, int startTime, int endTime = 0, int layer = 0, HitSounds hitsounds = 0)
        {
            var hitObject = new HitObjectInfo
            {
                Lane = lane,
                StartTime = startTime,
                EndTime = endTime,
                EditorLayer = layer,
                HitSound = hitsounds
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
        ///     Places an sv down in the map
        /// </summary>
        /// <param name="sv"></param>
        public void PlaceScrollVelocity(SliderVelocityInfo sv) => Perform(new EditorActionAddScrollVelocity(this, WorkingMap, sv));

        /// <summary>
        ///     Triggers an event of a specific action type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        [MoonSharpVisible(false)]
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
                case EditorActionType.CreateLayer:
                    LayerCreated?.Invoke(this, (EditorLayerCreatedEventArgs) args);
                    break;
                case EditorActionType.RemoveLayer:
                    LayerDeleted?.Invoke(this, (EditorLayerRemovedEventArgs) args);
                    break;
                case EditorActionType.RenameLayer:
                    LayerRenamed?.Invoke(this, (EditorLayerRenamedEventArgs) args);
                    break;
                case EditorActionType.ColorLayer:
                    LayerColorChanged?.Invoke(this, (EditorLayerColorChangedEventArgs) args);
                    break;
                case EditorActionType.AddScrollVelocity:
                    ScrollVelocityAdded?.Invoke(this, (EditorScrollVelocityAddedEventArgs) args);
                    break;
                case EditorActionType.RemoveScrollVelocity:
                    ScrollVelocityRemoved?.Invoke(this, (EditorScrollVelocityRemovedEventArgs) args);
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
            LayerCreated = null;
            LayerDeleted = null;
            LayerRenamed = null;
            LayerColorChanged = null;
            ScrollVelocityAdded = null;
            ScrollVelocityRemoved = null;
        }
    }
}