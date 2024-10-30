using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Batch;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.Add;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.AddBatch;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.Edit;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.Offset;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.Remove;
using Quaver.Shared.Screens.Edit.Actions.Bookmarks.RemoveBatch;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Flip;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Move;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Place;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.PlaceBatch;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Remove;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.RemoveBatch;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Resize;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Resnap;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Reverse;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Swap;
using Quaver.Shared.Screens.Edit.Actions.Hitsounds.Add;
using Quaver.Shared.Screens.Edit.Actions.Hitsounds.Remove;
using Quaver.Shared.Screens.Edit.Actions.Layers.Colors;
using Quaver.Shared.Screens.Edit.Actions.Layers.Create;
using Quaver.Shared.Screens.Edit.Actions.Layers.Move;
using Quaver.Shared.Screens.Edit.Actions.Layers.Remove;
using Quaver.Shared.Screens.Edit.Actions.Layers.Rename;
using Quaver.Shared.Screens.Edit.Actions.Layers.Visibility;
using Quaver.Shared.Screens.Edit.Actions.Preview;
using Quaver.Shared.Screens.Edit.Actions.SV.Add;
using Quaver.Shared.Screens.Edit.Actions.SV.AddBatch;
using Quaver.Shared.Screens.Edit.Actions.SV.ChangeMultiplierBatch;
using Quaver.Shared.Screens.Edit.Actions.SV.ChangeOffsetBatch;
using Quaver.Shared.Screens.Edit.Actions.SV.Remove;
using Quaver.Shared.Screens.Edit.Actions.SV.RemoveBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.Add;
using Quaver.Shared.Screens.Edit.Actions.Timing.AddBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeBpm;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeBpmBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeHidden;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeOffset;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeOffsetBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeSignature;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeSignatureBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.Remove;
using Quaver.Shared.Screens.Edit.Actions.Timing.RemoveBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.Reset;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.Colors;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.Create;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.MoveObjectsToTimingGroup;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.Remove;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.Rename;
using Quaver.Shared.Screens.Edit.Components;
using Quaver.Shared.Scripting;

namespace Quaver.Shared.Screens.Edit.Actions
{
    public class EditorActionManager : IDisposable
    {
        /// <summary>
        /// </summary>
        public EditScreen EditScreen { get; }

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
        public bool HasUnsavedChanges => UndoStack.Count != 0 && UndoStack.Peek() != LastSaveAction || UndoStack.Count == 0 && LastSaveAction != null;

        /// <summary>
        ///     An action manager dedicated for lua plugins
        /// </summary>
        public EditorPluginActionManager PluginActionManager { get; }

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
        ///     Event invoked when a batch of hitobjects have been swapped between 2 lanes
        /// </summary>
        public event EventHandler<EditorLanesSwappedEventArgs> LanesSwapped;

        /// <summary>
        ///     Event invoked when a batch of hitobjects have been reversed
        /// </summary>
        public event EventHandler<EditorHitObjectsReversedEventArgs> HitObjectsReversed;

        /// <summary>
        ///     Event invoked when a batch of hitobjects have been moved
        /// </summary>
        public event EventHandler<EditorHitObjectsMovedEventArgs> HitObjectsMoved;

        /// <summary>
        ///     Event invoked when hitobjects have been resnapped
        /// </summary>
        public event EventHandler<EditorActionHitObjectsResnappedEventArgs> HitObjectsResnapped;

        /// <summary>
        ///     Event invoked when a hitsound has been added to a group of objects
        /// </summary>
        public event EventHandler<EditorHitsoundAddedEventArgs> HitsoundAdded;

        /// <summary>
        ///     Event invoked when a hitsound has been removed from a group of objects
        /// </summary>
        public event EventHandler<EditorHitSoundRemovedEventArgs> HitsoundRemoved;

        /// <summary>
        ///     Event invoked when a layer has been created
        /// </summary>
        public event EventHandler<EditorLayerCreatedEventArgs> LayerCreated;

        /// <summary>
        ///     Event invoked when a layer has been deleted
        /// </summary>
        public event EventHandler<EditorLayerRemovedEventArgs> LayerDeleted;

        /// <summary>
        ///     Event invoked when a layer has been renamed
        /// </summary>
        public event EventHandler<EditorLayerRenamedEventArgs> LayerRenamed;

        /// <summary>
        ///     Event invoked when a timing group has been created
        /// </summary>
        public event EventHandler<EditorTimingGroupCreatedEventArgs> TimingGroupCreated;

        /// <summary>
        ///     Event invoked when a timing group has been deleted
        /// </summary>
        public event EventHandler<EditorTimingGroupRemovedEventArgs> TimingGroupDeleted;

        /// <summary>
        ///     Event invoked when a timing group has been renamed
        /// </summary>
        public event EventHandler<EditorTimingGroupRenamedEventArgs> TimingGroupRenamed;

        /// <summary>
        ///     Event invoked when a timing group's color has been changed
        /// </summary>
        public event EventHandler<EditorTimingGroupColorChangedEventArgs> TimingGroupColorChanged;

        /// <summary>
        ///     Event invoked when a timing group has been renamed
        /// </summary>
        public event EventHandler<EditorMoveObjectsToTimingGroupEventArgs> HitObjectsMovedToTimingGroup;

        /// <summary>
        ///     Event invoked when a layer's color has been changed
        /// </summary>
        public event EventHandler<EditorLayerColorChangedEventArgs> LayerColorChanged;

        /// <summary>
        ///     Event invoked when a scroll velocity has been added to the map
        /// </summary>
        public event EventHandler<EditorScrollVelocityAddedEventArgs> ScrollVelocityAdded;

        /// <summary>
        ///     Event invoked when a scroll velocity has been removed from the map
        /// </summary>
        public event EventHandler<EditorScrollVelocityRemovedEventArgs> ScrollVelocityRemoved;

        /// <summary>
        ///     Event invoked when a batch of scroll velocities has been added to the map
        /// </summary>
        public event EventHandler<EditorScrollVelocityBatchAddedEventArgs> ScrollVelocityBatchAdded;

        /// <summary>
        ///     Event invoked when a batch of scroll velocities has been removed from the map
        /// </summary>
        public event EventHandler<EditorScrollVelocityBatchRemovedEventArgs> ScrollVelocityBatchRemoved;

        /// <summary>
        ///     Event invoked when a timing point has been added to the map
        /// </summary>
        public event EventHandler<EditorTimingPointAddedEventArgs> TimingPointAdded;

        /// <summary>
        ///     Event invoked when a timing point has been removed from the map
        /// </summary>
        public event EventHandler<EditorTimingPointAddedEventArgs> TimingPointRemoved;

        /// <summary>
        ///     Event invoked when a batch of timing points has been added to the map
        /// </summary>
        public event EventHandler<EditorTimingPointBatchAddedEventArgs> TimingPointBatchAdded;

        /// <summary>
        ///     Event invoked when a batch of timing points has been removed from the map
        /// </summary>
        public event EventHandler<EditorTimingPointBatchRemovedEventArgs> TimingPointBatchRemoved;

        /// <summary>
        ///     Event invoked when the preview time of the map has changed
        /// </summary>
        public event EventHandler<EditorChangedPreviewTimeEventArgs> PreviewTimeChanged;

        /// <summary>
        ///     Event invoked when the offset of a timing point has been changed
        /// </summary>
        public event EventHandler<EditorTimingPointOffsetChangedEventArgs> TimingPointOffsetChanged;

        /// <summary>
        ///     Event invoked when the BPM of a timing point has been changed
        /// </summary>
        public event EventHandler<EditorTimingPointBpmChangedEventArgs> TimingPointBpmChanged;

        /// <summary>
        ///     Event invoked when the Signature of a timing point has been changed
        /// </summary>
        public event EventHandler<EditorTimingPointSignatureChangedEventArgs> TimingPointSignatureChanged;

        /// <summary>
        ///     Event invoked when the lines of a timing point have been hidden or unhidden
        /// </summary>
        public event EventHandler<EditorTimingPointHiddenChangedEventArgs> TimingPointHiddenChanged;

        /// <summary>
        ///     Event invoked when a batch of timing points have had their BPM changed
        /// </summary>
        public event EventHandler<EditorChangedTimingPointBpmBatchEventArgs> TimingPointBpmBatchChanged;

        /// <summary>
        ///     Event invoked when a batch of timing points have had their Signature changed
        /// </summary>
        public event EventHandler<EditorChangedTimingPointSignatureBatchEventArgs> TimingPointSignatureBatchChanged;

        /// <summary>
        ///     Event invoked when batch of timing points have had their offset changed
        /// </summary>
        public event EventHandler<EditorChangedTimingPointOffsetBatchEventArgs> TimingPointOffsetBatchChanged;

        /// <summary>
        ///     Event invoked when a batch of scroll velocities have had their offset changed
        /// </summary>
        public event EventHandler<EditorChangedScrollVelocityOffsetBatchEventArgs> ScrollVelocityOffsetBatchChanged;

        /// <summary>
        ///     Event invoked when a batch of scroll velocities have had their multipliers changed
        /// </summary>
        public event EventHandler<EditorChangedScrollVelocityMultiplierBatchEventArgs> ScrollVelocityMultiplierBatchChanged;

        /// <summary>
        ///     Event invoked when a bookmark has been added.
        /// </summary>
        public event EventHandler<EditorActionBookmarkAddedEventArgs> BookmarkAdded;

        /// <summary>
        ///     Event invoked when a bookmark has been removed.
        /// </summary>
        public event EventHandler<EditorActionBookmarkRemovedEventArgs> BookmarkRemoved;

        /// <summary>
        ///     Event invoked when a bookmark has been added.
        /// </summary>
        public event EventHandler<EditorActionBookmarkBatchAddedEventArgs> BookmarkBatchAdded;

        /// <summary>
        ///     Event invoked when a bookmark has been removed.
        /// </summary>
        public event EventHandler<EditorActionBookmarkBatchRemovedEventArgs> BookmarkBatchRemoved;

        /// <summary>
        ///     Event invoked whe na bookmark has been edited.
        /// </summary>
        public event EventHandler<EditorActionBookmarkEditedEventArgs> BookmarkEdited;

        /// <summary>
        ///     Event invoked when a batch of bookmark's offsets have been changed.
        /// </summary>
        public event EventHandler<EditorActionChangeBookmarkOffsetBatchEventArgs> BookmarkBatchOffsetChanged;

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="workingMap"></param>
        public EditorActionManager(EditScreen screen, Qua workingMap)
        {
            EditScreen = screen;
            WorkingMap = workingMap;
            PluginActionManager = new EditorPluginActionManager(this);
        }

        /// <summary>
        ///     Performs a given action for the editor to take.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="fromLua"></param>
        public void Perform(IEditorAction action, bool fromLua = false)
        {
            action.Perform();
            UndoStack.Push(action);
            RedoStack.Clear();
            LuaImGui.Inform(action, HistoryType.New, fromLua);
        }

        /// <summary>
        ///     Performs a list of actions as a single action.
        /// </summary>
        /// <param name="actions"></param>
        public void PerformBatch(List<IEditorAction> actions, bool fromLua = false) =>
            Perform(new EditorActionBatch(this, actions), fromLua);

        /// <summary>
        ///     Undoes the first action in the stack
        /// </summary>
        public void Undo(bool fromLua = false)
        {
            if (UndoStack.Count == 0)
                return;

            var action = UndoStack.Pop();
            action.Undo();

            RedoStack.Push(action);
            LuaImGui.Inform(action, HistoryType.Undo, fromLua);
        }

        /// <summary>
        ///     Redoes the first action in the stack
        /// </summary>
        public void Redo(bool fromLua = false)
        {
            if (RedoStack.Count == 0)
                return;

            var action = RedoStack.Pop();
            action.Perform();

            UndoStack.Push(action);
            LuaImGui.Inform(action, HistoryType.Redo, fromLua);
        }

        /// <summary>
        /// </summary>
        /// <param name="h"></param>
        public void PlaceHitObject(HitObjectInfo h, bool fromLua = false) => Perform(new EditorActionPlaceHitObject(this, WorkingMap, h), fromLua);

        /// <summary>
        /// </summary>
        /// <param name="lane"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="layer"></param>
        /// <param name="hitsounds"></param>
        /// <param name="timingGroupId"></param>
        /// <param name="fromLua"></param>
        public HitObjectInfo PlaceHitObject(int lane, int startTime, int endTime = 0, int layer = 0, HitSounds hitsounds = 0, string timingGroupId = Qua.DefaultScrollGroupId, bool fromLua = false)
        {
            var hitObject = new HitObjectInfo
            {
                Lane = lane,
                StartTime = startTime,
                EndTime = endTime,
                EditorLayer = layer,
                HitSound = hitsounds,
                TimingGroup = timingGroupId
            };

            Perform(new EditorActionPlaceHitObject(this, WorkingMap, hitObject), fromLua);

            return hitObject;
        }

        /// <summary>
        /// </summary>
        /// <param name="hitObjects"></param>
        public void PlaceHitObjectBatch(List<HitObjectInfo> hitObjects, bool fromLua = false) => Perform(new EditorActionPlaceHitObjectBatch(this, WorkingMap, hitObjects), fromLua);

        /// <summary>
        ///     Removes a HitObject from the map
        /// </summary>
        /// <param name="h"></param>
        public void RemoveHitObject(HitObjectInfo h, bool fromLua = false) => Perform(new EditorActionRemoveHitObject(this, WorkingMap, h), fromLua);

        /// <summary>
        ///     Removes a list of objects from the map
        /// </summary>
        /// <param name="objects"></param>
        public void RemoveHitObjectBatch(List<HitObjectInfo> objects, bool fromLua = false) =>
            Perform(new EditorActionRemoveHitObjectBatch(this, WorkingMap, objects), fromLua);

        /// <summary>
        ///     Resizes a hitobject/long note to a given time
        /// </summary>
        /// <param name="h"></param>
        /// <param name="time"></param>
        public void ResizeLongNote(HitObjectInfo h, int originalTime, int time, bool fromLua = false)
            => Perform(new EditorActionResizeLongNote(this, WorkingMap, h, originalTime, time), fromLua);

        /// <summary>
        ///     Places an sv down in the map
        /// </summary>
        /// <param name="sv"></param>
        /// <param name="scrollGroup"></param>
        /// <param name="fromLua"></param>
        public void PlaceScrollVelocity(SliderVelocityInfo sv, ScrollGroup scrollGroup, bool fromLua = false) => Perform(new EditorActionAddScrollVelocity(this, WorkingMap, sv, scrollGroup), fromLua);

        /// <summary>
        ///     Places a batch of scroll velocities into the map
        /// </summary>
        /// <param name="svs"></param>
        /// <param name="scrollGroup"></param>
        /// <param name="fromLua"></param>
        public void PlaceScrollVelocityBatch(List<SliderVelocityInfo> svs, ScrollGroup scrollGroup, bool fromLua = false) => Perform(new EditorActionAddScrollVelocityBatch(this, WorkingMap, svs, scrollGroup), fromLua);

        /// <summary>
        ///     Removes a batch of scroll velocities from the map
        /// </summary>
        /// <param name="svs"></param>
        /// <param name="scrollGroup"></param>
        /// <param name="fromLua"></param>
        public void RemoveScrollVelocityBatch(List<SliderVelocityInfo> svs, ScrollGroup scrollGroup, bool fromLua = false) => Perform(new EditorActionRemoveScrollVelocityBatch(this, WorkingMap, svs, scrollGroup), fromLua);

        /// <summary>
        ///     Changes the offset of a batch of scroll velocities
        /// </summary>
        /// <param name="svs"></param>
        /// <param name="offset"></param>
        public void ChangeScrollVelocityOffsetBatch(List<SliderVelocityInfo> svs, float offset, bool fromLua = false) => Perform(new EditorActionChangeScrollVelocityOffsetBatch(this, WorkingMap, svs, offset), fromLua);

        /// <summary>
        ///     Changes the multiplier of a batch of scroll velocities
        /// </summary>
        /// <param name="svs"></param>
        /// <param name="multiplier"></param>
        public void ChangeScrollVelocityMultiplierBatch(List<SliderVelocityInfo> svs, float multiplier, bool fromLua = false) => Perform(new EditorActionChangeScrollVelocityMultiplierBatch(this, WorkingMap, svs, multiplier), fromLua);

        /// <summary>
        ///     Adds a timing point to the map
        /// </summary>
        /// <param name="tp"></param>
        public void PlaceTimingPoint(TimingPointInfo tp, bool fromLua = false) => Perform(new EditorActionAddTimingPoint(this, WorkingMap, tp), fromLua);

        /// <summary>
        ///     Removes a timing point from the map
        /// </summary>
        /// <param name="tp"></param>
        public void RemoveTimingPoint(TimingPointInfo tp, bool fromLua = false) => Perform(new EditorActionRemoveTimingPoint(this, WorkingMap, tp), fromLua);

        /// <summary>
        ///     Places a batch of timing points to the map
        /// </summary>
        /// <param name="tps"></param>
        public void PlaceTimingPointBatch(List<TimingPointInfo> tps, bool fromLua = false) => Perform(new EditorActionAddTimingPointBatch(this, WorkingMap, tps), fromLua);

        /// <summary>
        ///     Removes a batch of timing points from the map
        /// </summary>
        /// <param name="tps"></param>
        public void RemoveTimingPointBatch(List<TimingPointInfo> tps, bool fromLua = false) => Perform(new EditorActionRemoveTimingPointBatch(this, WorkingMap, tps), fromLua);

        /// <summary>
        ///     Changes the offset of a timing point
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="offset"></param>
        public void ChangeTimingPointOffset(TimingPointInfo tp, float offset, bool fromLua = false) => Perform(new EditorActionChangeTimingPointOffset(this, WorkingMap, tp, offset), fromLua);

        /// <summary>
        ///     Changes the BPM of an existing timing point
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="bpm"></param>
        public void ChangeTimingPointBpm(TimingPointInfo tp, float bpm, bool fromLua = false) => Perform(new EditorActionChangeTimingPointBpm(this, WorkingMap, tp, bpm), fromLua);

        /// <summary>
        ///     Changes the Signature of an existing timing point
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="timeSig"></param>
        public void ChangeTimingPointSignature(TimingPointInfo tp, int timeSig, bool fromLua = false) => Perform(new EditorActionChangeTimingPointSignature(this, WorkingMap, tp, timeSig), fromLua);

        /// <summary>
        ///     Changes whether an existing timing point's lines are hidden or not
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="hidden"></param>
        public void ChangeTimingPointHidden(TimingPointInfo tp, bool hidden, bool fromLua = false) => Perform(new EditorActionChangeTimingPointHidden(this, WorkingMap, tp, hidden), fromLua);

        /// <summary>
        ///     Changes a batch of timing points to a new BPM
        /// </summary>
        /// <param name="tps"></param>
        /// <param name="bpm"></param>
        public void ChangeTimingPointBpmBatch(List<TimingPointInfo> tps, float bpm, bool fromLua = false) => Perform(new EditorActionChangeTimingPointBpmBatch(this, WorkingMap, tps, bpm), fromLua);

        /// <summary>
        ///     Changes a batch of timing points to a new signature
        /// </summary>
        /// <param name="tps"></param>
        /// <param name="sig"></param>
        public void ChangeTimingPointSignatureBatch(List<TimingPointInfo> tps, int sig, bool fromLua = false) => Perform(new EditorActionChangeTimingPointSignatureBatch(this, WorkingMap, tps, sig), fromLua);

        /// <summary>
        ///     Moves a batch of timing points' offsets by a given value
        /// </summary>
        /// <param name="tps"></param>
        /// <param name="offset"></param>
        public void ChangeTimingPointOffsetBatch(List<TimingPointInfo> tps, float offset, bool fromLua = false) => Perform(new EditorActionChangeTimingPointOffsetBatch(this, WorkingMap, tps, offset), fromLua);

        /// <summary>
        ///     Resets a timing point back to zero
        /// </summary>
        /// <param name="tp"></param>
        public void ResetTimingPoint(TimingPointInfo tp, bool fromLua = false) => Perform(new EditorActionResetTimingPoint(this, WorkingMap, tp), fromLua);

        /// <summary>
        ///     Adds an editor layer to the map
        /// </summary>
        /// <param name="layer"></param>
        public void CreateLayer(EditorLayerInfo layer, int index = -1, bool fromLua = false) => Perform(new EditorActionCreateLayer(WorkingMap, this, EditScreen.SelectedHitObjects, layer, index), fromLua);

        /// <summary>
        ///     Removes a non-default editor layer from the map
        /// </summary>
        /// <param name="layer"></param>
        public void RemoveLayer(EditorLayerInfo layer, bool fromLua = false)
        {
            if (layer != EditScreen.DefaultLayer)
                Perform(new EditorActionRemoveLayer(this, WorkingMap, EditScreen.SelectedHitObjects, layer), fromLua);
        }

        /// <summary>
        ///     Changes the name of a non-default editor layer
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="name"></param>
        public void RenameLayer(EditorLayerInfo layer, string name, bool fromLua = false)
        {
            if (layer != EditScreen.DefaultLayer)
                Perform(new EditorActionRenameLayer(this, WorkingMap, layer, name), fromLua);
        }

        /// <summary>
        ///     Changes the editor layer of existing hitobjects
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="hitObjects"></param>
        public void MoveHitObjectsToLayer(EditorLayerInfo layer, List<HitObjectInfo> hitObjects, bool fromLua = false) => Perform(new EditorActionMoveObjectsToLayer(this, WorkingMap, layer, hitObjects), fromLua);

        /// <summary>
        ///     Changes the color of a non-default editor layer
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="color"></param>
        public void ChangeLayerColor(EditorLayerInfo layer, Color color, bool fromLua = false)
        {
            if (layer != EditScreen.DefaultLayer)
                Perform(new EditorActionChangeLayerColor(this, WorkingMap, layer, color), fromLua);
        }

        /// <summary>
        ///     Toggles the visibility of an existing editor layer
        ///     Does not get added to the undo stack
        /// </summary>
        /// <param name="layer"></param>
        public void ToggleLayerVisibility(EditorLayerInfo layer) => new EditorActionToggleLayerVisibility(this, WorkingMap, layer).Perform();

        /// <summary>
        ///     Adds an editor layer to the map
        /// </summary>
        /// <param name="id"></param>
        /// <param name="timingGroup"></param>
        /// <param name="hitObjectInfos"></param>
        /// <param name="fromLua"></param>
        public void CreateTimingGroup(string id, TimingGroup timingGroup, List<HitObjectInfo> hitObjectInfos,
            bool fromLua = false) => Perform(new EditorActionCreateTimingGroup(this, WorkingMap, id, timingGroup, hitObjectInfos), fromLua);

        /// <summary>
        ///     Removes a non-default editor layer from the map
        /// </summary>
        public void RemoveTimingGroup(string id, bool fromLua = false)
        {
            if (id is Qua.GlobalScrollGroupId or Qua.DefaultScrollGroupId
                || !WorkingMap.TimingGroups.TryGetValue(id, out var timingGroup)) return;
            Perform(new EditorActionRemoveTimingGroup(this, WorkingMap, id, timingGroup, null), fromLua);
        }

        /// <summary>
        ///     Changes the name of a non-default editor layer
        /// </summary>
        /// <param name="oldId"></param>
        /// <param name="newId"></param>
        /// <param name="fromLua"></param>
        public bool RenameTimingGroup(string oldId, string newId, bool fromLua = false)
        {
            if (oldId is Qua.DefaultScrollGroupId or Qua.GlobalScrollGroupId
                || newId is Qua.DefaultScrollGroupId or Qua.GlobalScrollGroupId
                || !WorkingMap.TimingGroups.ContainsKey(oldId)
                || WorkingMap.TimingGroups.ContainsKey(newId)
                || !Regex.IsMatch(newId, "^[a-zA-Z0-9_]+$"))
                return false;

            Perform(new EditorActionRenameTimingGroup(this, WorkingMap, oldId, newId, null), fromLua);
            return true;

        }

        /// <summary>
        ///     Changes the editor layer of existing hitobjects
        /// </summary>
        /// <param name="timingGroupId"></param>
        /// <param name="hitObjects"></param>
        public void MoveHitObjectsToTimingGroup(string timingGroupId, List<HitObjectInfo> hitObjects, bool fromLua = false) => Perform(new EditorActionMoveObjectsToTimingGroup(this, WorkingMap, hitObjects, timingGroupId), fromLua);

        /// <summary>
        ///     Changes the color of a timing group
        /// </summary>
        /// <param name="timingGroup"></param>
        /// <param name="color"></param>
        public void ChangeTimingGroupColor(string id, Color color, bool fromLua = false)
        {
            if (id != Qua.DefaultScrollGroupId && WorkingMap.TimingGroups.TryGetValue(id, out TimingGroup timingGroup))
                Perform(new EditorActionChangeTimingGroupColor(this, WorkingMap, timingGroup, color), fromLua);
        }

        /// <summary>
        /// </summary>
        /// <param name="input"></param>
        public void GoToObjects(string input, bool fromLua = false) => EditScreen.GoToObjects(input);

        /// <summary>
        /// </summary>
        /// <param name="hitObjects"></param>
        public void SetHitObjectSelection(List<HitObjectInfo> hitObjects)
        {
            EditScreen.SelectedHitObjects.Clear();

            // Only select objects that exist in the map
            var existingHitObjects = EditScreen.WorkingMap.HitObjects.FindAll(
                a => hitObjects.Any(
                    b => a.StartTime == b.StartTime && a.Lane == b.Lane
                )
            );

            if (existingHitObjects.Count == 0)
                return;

            EditScreen.SelectedHitObjects.AddRange(existingHitObjects);
        }

        /// <summary>
        ///     Resnaps all notes in a given map to the closest of the specified snaps in the list.
        /// </summary>
        /// <remarks>
        ///     The reason for working with multiple snaps is because using the first common multiple
        ///     might not be accurate enough in terms of milliseconds. An example for this would be to
        ///     resnap to 1/12 and 1/16 in a 200BPM map, which would result in a common multiple of 1/192.
        ///     This results in a time of 1.56ms per snap, which is not accurate enough for our purposes.
        /// </remarks>
        /// <param name="snaps">List of snaps to snap to</param>
        /// <param name="hitObjectsToResnap">List of hitobjects to resnap</param>
        public void ResnapNotes(List<int> snaps, List<HitObjectInfo> hitObjectsToResnap, bool fromLua = false) => Perform(new EditorActionResnapHitObjects(this, WorkingMap, snaps, hitObjectsToResnap, true), fromLua);

        /// <summary>
        ///     Detects the BPM of the map and returns the object instance
        /// </summary>
        /// <returns></returns>
        public EditorBpmDetector DetectBpm() => new EditorBpmDetector(EditScreen.Track);

        /// <summary>
        /// </summary>
        /// <param name="time"></param>
        public void SetPreviewTime(int time, bool fromLua = false) => Perform(new EditorActionChangePreviewTime(this, WorkingMap, time), fromLua);

        /// <summary>
        ///     Adds a bookmark to the map
        /// </summary>
        public void AddBookmark(BookmarkInfo bookmarkInfo, bool fromLua = false) => Perform(new EditorActionAddBookmark(this, WorkingMap, bookmarkInfo), fromLua);

        public void AddBookmark(int time, string note, bool fromLua = false) => AddBookmark(new BookmarkInfo
        {
            StartTime = time,
            Note = note
        }, fromLua);

        /// <summary>
        ///     Adds a batch of bookmarks to the map
        /// </summary>
        /// <param name="bookmarks"></param>
        public void AddBookmarkBatch(List<BookmarkInfo> bookmarks, bool fromLua = false) => Perform(new EditorActionAddBookmarkBatch(this, WorkingMap, bookmarks), fromLua);
        /// <summary>
        ///     Removes a bookmark from the map.
        /// </summary>
        /// <param name="bookmark"></param>
        public void RemoveBookmark(BookmarkInfo bookmark, bool fromLua = false) => Perform(new EditorActionRemoveBookmark(this, WorkingMap, bookmark), fromLua);

        /// <summary>
        ///     Removes a batch of bookmarks from the map.
        /// </summary>
        /// <param name="bookmark"></param>
        public void RemoveBookmarkBatch(List<BookmarkInfo> bookmark, bool fromLua = false) => Perform(new EditorActionRemoveBookmarkBatch(this, WorkingMap, bookmark), fromLua);

        /// <summary>
        ///     Edits the note of an existing bookmark
        /// </summary>
        /// <param name="bookmark"></param>
        /// <param name="note"></param>
        public void EditBookmark(BookmarkInfo bookmark, string note, bool fromLua = false) => Perform(new EditorActionEditBookmark(this, WorkingMap, bookmark, note), fromLua);

        /// <summary>
        ///     Adjusts the offset of a batch of bookmarks
        /// </summary>
        /// <param name="bookmarks"></param>
        /// <param name="offset"></param>
        public void ChangeBookmarkBatchOffset(List<BookmarkInfo> bookmarks, int offset, bool fromLua = false) => Perform(new EditorActionChangeBookmarkOffsetBatch(this, WorkingMap, bookmarks, offset), fromLua);

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
                    HitObjectPlaced?.Invoke(this, (EditorHitObjectPlacedEventArgs)args);
                    break;
                case EditorActionType.RemoveHitObject:
                    HitObjectRemoved?.Invoke(this, (EditorHitObjectRemovedEventArgs)args);
                    break;
                case EditorActionType.ResizeLongNote:
                    LongNoteResized?.Invoke(this, (EditorLongNoteResizedEventArgs)args);
                    break;
                case EditorActionType.RemoveHitObjectBatch:
                    HitObjectBatchRemoved?.Invoke(this, (EditorHitObjectBatchRemovedEventArgs)args);
                    break;
                case EditorActionType.PlaceHitObjectBatch:
                    HitObjectBatchPlaced?.Invoke(this, (EditorHitObjectBatchPlacedEventArgs)args);
                    break;
                case EditorActionType.FlipHitObjects:
                    HitObjectsFlipped?.Invoke(this, (EditorHitObjectsFlippedEventArgs)args);
                    break;
                case EditorActionType.SwapLanes:
                    LanesSwapped?.Invoke(this, (EditorLanesSwappedEventArgs)args);
                    break;
                case EditorActionType.MoveHitObjects:
                    HitObjectsMoved?.Invoke(this, (EditorHitObjectsMovedEventArgs)args);
                    break;
                case EditorActionType.AddHitsound:
                    HitsoundAdded?.Invoke(this, (EditorHitsoundAddedEventArgs)args);
                    break;
                case EditorActionType.RemoveHitsound:
                    HitsoundRemoved?.Invoke(this, (EditorHitSoundRemovedEventArgs)args);
                    break;
                case EditorActionType.CreateLayer:
                    LayerCreated?.Invoke(this, (EditorLayerCreatedEventArgs)args);
                    break;
                case EditorActionType.RemoveLayer:
                    LayerDeleted?.Invoke(this, (EditorLayerRemovedEventArgs)args);
                    break;
                case EditorActionType.RenameLayer:
                    LayerRenamed?.Invoke(this, (EditorLayerRenamedEventArgs)args);
                    break;
                case EditorActionType.ColorLayer:
                    LayerColorChanged?.Invoke(this, (EditorLayerColorChangedEventArgs)args);
                    break;
                case EditorActionType.AddScrollVelocity:
                    ScrollVelocityAdded?.Invoke(this, (EditorScrollVelocityAddedEventArgs)args);
                    break;
                case EditorActionType.RemoveScrollVelocity:
                    ScrollVelocityRemoved?.Invoke(this, (EditorScrollVelocityRemovedEventArgs)args);
                    break;
                case EditorActionType.AddScrollVelocityBatch:
                    ScrollVelocityBatchAdded?.Invoke(this, (EditorScrollVelocityBatchAddedEventArgs)args);
                    break;
                case EditorActionType.RemoveScrollVelocityBatch:
                    ScrollVelocityBatchRemoved?.Invoke(this, (EditorScrollVelocityBatchRemovedEventArgs)args);
                    break;
                case EditorActionType.AddTimingPoint:
                    TimingPointAdded?.Invoke(this, (EditorTimingPointAddedEventArgs)args);
                    break;
                case EditorActionType.RemoveTimingPoint:
                    TimingPointRemoved?.Invoke(this, (EditorTimingPointRemovedEventArgs)args);
                    break;
                case EditorActionType.AddTimingPointBatch:
                    TimingPointBatchAdded?.Invoke(this, (EditorTimingPointBatchAddedEventArgs)args);
                    break;
                case EditorActionType.RemoveTimingPointBatch:
                    TimingPointBatchRemoved?.Invoke(this, (EditorTimingPointBatchRemovedEventArgs)args);
                    break;
                case EditorActionType.ChangePreviewTime:
                    PreviewTimeChanged?.Invoke(this, (EditorChangedPreviewTimeEventArgs)args);
                    break;
                case EditorActionType.ChangeTimingPointOffset:
                    TimingPointOffsetChanged?.Invoke(this, (EditorTimingPointOffsetChangedEventArgs)args);
                    break;
                case EditorActionType.ChangeTimingPointBpm:
                    TimingPointBpmChanged?.Invoke(this, (EditorTimingPointBpmChangedEventArgs)args);
                    break;
                case EditorActionType.ChangeTimingPointSignature:
                    TimingPointSignatureChanged?.Invoke(this, (EditorTimingPointSignatureChangedEventArgs)args);
                    break;
                case EditorActionType.ChangeTimingPointHidden:
                    TimingPointHiddenChanged?.Invoke(this, (EditorTimingPointHiddenChangedEventArgs)args);
                    break;
                case EditorActionType.ChangeTimingPointBpmBatch:
                    TimingPointBpmBatchChanged?.Invoke(this, (EditorChangedTimingPointBpmBatchEventArgs)args);
                    break;
                case EditorActionType.ChangeTimingPointSignatureBatch:
                    TimingPointSignatureBatchChanged?.Invoke(this, (EditorChangedTimingPointSignatureBatchEventArgs)args);
                    break;
                case EditorActionType.ChangeTimingPointOffsetBatch:
                    TimingPointOffsetBatchChanged?.Invoke(this, (EditorChangedTimingPointOffsetBatchEventArgs)args);
                    break;
                case EditorActionType.ChangeScrollVelocityOffsetBatch:
                    ScrollVelocityOffsetBatchChanged?.Invoke(this, (EditorChangedScrollVelocityOffsetBatchEventArgs)args);
                    break;
                case EditorActionType.ChangeScrollVelocityMultiplierBatch:
                    ScrollVelocityMultiplierBatchChanged?.Invoke(this, (EditorChangedScrollVelocityMultiplierBatchEventArgs)args);
                    break;
                case EditorActionType.ResnapHitObjects:
                    HitObjectsResnapped?.Invoke(this, (EditorActionHitObjectsResnappedEventArgs)args);
                    break;
                case EditorActionType.ReverseHitObjects:
                    HitObjectsReversed?.Invoke(this, (EditorHitObjectsReversedEventArgs)args);
                    break;
                case EditorActionType.AddBookmark:
                    BookmarkAdded?.Invoke(this, (EditorActionBookmarkAddedEventArgs)args);
                    break;
                case EditorActionType.RemoveBookmark:
                    BookmarkRemoved?.Invoke(this, (EditorActionBookmarkRemovedEventArgs)args);
                    break;
                case EditorActionType.AddBookmarkBatch:
                    BookmarkBatchAdded?.Invoke(this, (EditorActionBookmarkBatchAddedEventArgs)args);
                    break;
                case EditorActionType.RemoveBookmarkBatch:
                    BookmarkBatchRemoved?.Invoke(this, (EditorActionBookmarkBatchRemovedEventArgs)args);
                    break;
                case EditorActionType.EditBookmark:
                    BookmarkEdited?.Invoke(this, (EditorActionBookmarkEditedEventArgs)args);
                    break;
                case EditorActionType.ChangeBookmarkOffsetBatch:
                    BookmarkBatchOffsetChanged?.Invoke(this, (EditorActionChangeBookmarkOffsetBatchEventArgs)args);
                    break;
                case EditorActionType.CreateTimingGroup:
                    TimingGroupCreated?.Invoke(this, (EditorTimingGroupCreatedEventArgs)args);
                    break;
                case EditorActionType.RemoveTimingGroup:
                    TimingGroupDeleted?.Invoke(this, (EditorTimingGroupRemovedEventArgs)args);
                    break;
                case EditorActionType.RenameTimingGroup:
                    TimingGroupRenamed?.Invoke(this, (EditorTimingGroupRenamedEventArgs)args);
                    break;
                case EditorActionType.ColorTimingGroup:
                    TimingGroupColorChanged?.Invoke(this, (EditorTimingGroupColorChangedEventArgs)args);
                    break;
                case EditorActionType.MoveObjectsToTimingGroup:
                    HitObjectsMovedToTimingGroup?.Invoke(this, (EditorMoveObjectsToTimingGroupEventArgs)args);
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
            LanesSwapped = null;
            HitObjectsMoved = null;
            HitsoundAdded = null;
            HitsoundRemoved = null;
            LayerCreated = null;
            LayerDeleted = null;
            LayerRenamed = null;
            LayerColorChanged = null;
            ScrollVelocityAdded = null;
            ScrollVelocityRemoved = null;
            ScrollVelocityBatchAdded = null;
            ScrollVelocityBatchRemoved = null;
            TimingPointAdded = null;
            TimingPointRemoved = null;
            TimingPointBatchAdded = null;
            TimingPointBatchRemoved = null;
            PreviewTimeChanged = null;
            TimingPointOffsetChanged = null;
            TimingPointBpmChanged = null;
            TimingPointSignatureChanged = null;
            TimingPointHiddenChanged = null;
            TimingPointBpmBatchChanged = null;
            TimingPointSignatureBatchChanged = null;
            TimingPointOffsetBatchChanged = null;
            ScrollVelocityOffsetBatchChanged = null;
            ScrollVelocityMultiplierBatchChanged = null;
            HitObjectsResnapped = null;
            HitObjectsReversed = null;
            BookmarkAdded = null;
            BookmarkRemoved = null;
            BookmarkEdited = null;
            BookmarkBatchAdded = null;
            BookmarkBatchRemoved = null;
            BookmarkBatchOffsetChanged = null;
        }
    }
}
