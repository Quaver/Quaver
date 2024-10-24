using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Components;

namespace Quaver.Shared.Screens.Edit.Actions
{
    [MoonSharpUserData]
    public class EditorPluginActionManager
    {
        /// <summary>
        /// </summary>
        [MoonSharpVisible(false)]
        public EditorActionManager ActionManager { get; }

        /// <summary>
        /// </summary>
        /// <param name="manager"></param>
        [MoonSharpVisible(false)]
        public EditorPluginActionManager(EditorActionManager manager) => ActionManager = manager;

        /// <summary>
        /// </summary>
        /// <param name="action"></param>
        public void Perform(IEditorAction action) => ActionManager.Perform(action, true);

        /// <summary>
        /// </summary>
        /// <param name="actions"></param>
        public void PerformBatch(List<IEditorAction> actions) => ActionManager.PerformBatch(actions, true);

        public void Redo() => ActionManager.Redo(true);

        public void Undo() => ActionManager.Undo(true);

        /// <summary>
        /// </summary>
        /// <param name="h"></param>
        public void PlaceHitObject(HitObjectInfo h) => ActionManager.PlaceHitObject(h, true);

        /// <summary>
        /// </summary>
        /// <param name="lane"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="layer"></param>
        /// <param name="hitsounds"></param>
        /// <param name="timingGroupId"></param>
        public HitObjectInfo PlaceHitObject(
            int lane,
            int startTime,
            int endTime = 0,
            int layer = 0,
            HitSounds hitsounds = 0,
            string timingGroupId = Qua.GlobalScrollGroupId
        ) =>
            ActionManager.PlaceHitObject(lane, startTime, endTime, layer, hitsounds, timingGroupId, true);

        /// <summary>
        /// </summary>
        /// <param name="hitObjects"></param>
        public void PlaceHitObjectBatch(List<HitObjectInfo> hitObjects) => ActionManager.PlaceHitObjectBatch(hitObjects, true);

        /// <summary>
        /// </summary>
        /// <param name="h"></param>
        public void RemoveHitObject(HitObjectInfo h) => ActionManager.RemoveHitObject(h, true);

        /// <summary>
        /// </summary>
        /// <param name="hitObjects"></param>
        public void RemoveHitObjectBatch(List<HitObjectInfo> hitObjects) =>
            ActionManager.RemoveHitObjectBatch(hitObjects, true);

        /// <summary>
        /// </summary>
        /// <param name="h"></param>
        /// <param name="originalTime"></param>
        /// <param name="time"></param>
        public void ResizeLongNote(HitObjectInfo h, int originalTime, int time) => ActionManager.ResizeLongNote(h, originalTime, time, true);

        /// <summary>
        /// </summary>
        /// <param name="sv"></param>
        /// <param name="scrollGroup"></param>
        public void PlaceScrollVelocity(SliderVelocityInfo sv, ScrollGroup scrollGroup) => ActionManager.PlaceScrollVelocity(sv, scrollGroup, true);

        /// <summary>
        /// </summary>
        /// <param name="svs"></param>
        /// <param name="scrollGroup"></param>
        public void PlaceScrollVelocityBatch(List<SliderVelocityInfo> svs, ScrollGroup scrollGroup) =>
            ActionManager.PlaceScrollVelocityBatch(svs, scrollGroup, true);

        /// <summary>
        /// </summary>
        /// <param name="sv"></param>
        /// <param name="scrollGroup"></param>
        public void RemoveScrollVelocity(SliderVelocityInfo sv, ScrollGroup scrollGroup) =>
            ActionManager.RemoveScrollVelocityBatch(new List<SliderVelocityInfo> { sv }, scrollGroup, true);

        /// <summary>
        /// </summary>
        /// <param name="svs"></param>
        /// <param name="scrollGroup"></param>
        public void RemoveScrollVelocityBatch(List<SliderVelocityInfo> svs, ScrollGroup scrollGroup) =>
            ActionManager.RemoveScrollVelocityBatch(svs, scrollGroup, true);

        /// <summary>
        /// </summary>
        /// <param name="tp"></param>
        public void PlaceTimingPoint(TimingPointInfo tp) => ActionManager.PlaceTimingPoint(tp, true);

        /// <summary>
        /// </summary>
        /// <param name="tp"></param>
        public void RemoveTimingPoint(TimingPointInfo tp) => ActionManager.RemoveTimingPoint(tp, true);

        /// <summary>
        /// </summary>
        /// <param name="tps"></param>
        public void PlaceTimingPointBatch(List<TimingPointInfo> tps) => ActionManager.PlaceTimingPointBatch(tps, true);

        /// <summary>
        /// </summary>
        /// <param name="tps"></param>
        public void RemoveTimingPointBatch(List<TimingPointInfo> tps) => ActionManager.RemoveTimingPointBatch(tps, true);

        /// <summary>
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="offset"></param>
        public void ChangeTimingPointOffset(TimingPointInfo tp, float offset) => ActionManager.ChangeTimingPointOffset(tp, offset, true);

        /// <summary>
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="bpm"></param>
        public void ChangeTimingPointBpm(TimingPointInfo tp, float bpm) => ActionManager.ChangeTimingPointBpm(tp, bpm, true);

        /// <summary>
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="hidden"></param>
        public void ChangeTimingPointHidden(TimingPointInfo tp, bool hidden) => ActionManager.ChangeTimingPointHidden(tp, hidden, true);

        /// <summary>
        /// </summary>
        /// <param name="tps"></param>
        /// <param name="bpm"></param>
        public void ChangeTimingPointBpmBatch(List<TimingPointInfo> tps, float bpm) => ActionManager.ChangeTimingPointBpmBatch(tps, bpm, true);

        /// <summary>
        /// </summary>
        /// <param name="tps"></param>
        /// <param name="offset"></param>
        public void ChangeTimingPointOffsetBatch(List<TimingPointInfo> tps, float offset) => ActionManager.ChangeTimingPointOffsetBatch(tps, offset, true);

        /// <summary>
        /// </summary>
        /// <param name="tp"></param>
        public void ResetTimingPoint(TimingPointInfo tp) => ActionManager.ResetTimingPoint(tp, true);

        /// <summary>
        /// </summary>
        /// <param name="input"></param>
        public void GoToObjects(string input) => ActionManager.GoToObjects(input, true);

        /// <summary>
        /// </summary>
        /// <param name="hitObjects"></param>
        public void SetHitObjectSelection(List<HitObjectInfo> hitObjects) => ActionManager.SetHitObjectSelection(hitObjects);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public EditorBpmDetector DetectBpm() => ActionManager.DetectBpm();

        /// <summary>
        /// </summary>
        /// <param name="time"></param>
        public void SetPreviewTime(int time) => ActionManager.SetPreviewTime(time, true);

        /// <summary>
        /// </summary>
        /// <param name="layer"></param>
        public void CreateLayer(EditorLayerInfo layer, int index = -1) => ActionManager.CreateLayer(layer, index, true);

        /// <summary>
        /// </summary>
        /// <param name="layer"></param>
        public void RemoveLayer(EditorLayerInfo layer) => ActionManager.RemoveLayer(layer, true);

        /// <summary>
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="name"></param>
        public void RenameLayer(EditorLayerInfo layer, string name) => ActionManager.RenameLayer(layer, name, true);

        /// <summary>
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="hitObjects"></param>
        public void MoveHitObjectsToLayer(EditorLayerInfo layer, List<HitObjectInfo> hitObjects) => ActionManager.MoveHitObjectsToLayer(layer, hitObjects, true);

        /// <summary>
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="color"></param>
        public void ChangeLayerColor(EditorLayerInfo layer, int r, int g, int b) =>
            ActionManager.ChangeLayerColor(layer, new(r, g, b), true);

        /// <summary>
        /// </summary>
        /// <param name="layer"></param>
        public void ToggleLayerVisibility(EditorLayerInfo layer) => ActionManager.ToggleLayerVisibility(layer);

        /// <summary>
        /// </summary>
        /// <param name="snaps"></param>
        /// <param name="hitObjectsToResnap"></param>
        public void ResnapNotes(List<int> snaps, List<HitObjectInfo> hitObjectsToResnap) =>
            ActionManager.ResnapNotes(snaps, hitObjectsToResnap, true);

        public void AddBookmark(BookmarkInfo bookmarkInfo) => ActionManager.AddBookmark(bookmarkInfo, true);

        public void AddBookmark(int time, string note) => ActionManager.AddBookmark(time, note, true);

        public void AddBookmarkBatch(List<BookmarkInfo> bookmarks) => ActionManager.AddBookmarkBatch(bookmarks, true);

        public void RemoveBookmark(BookmarkInfo bookmark) => ActionManager.RemoveBookmark(bookmark, true);

        public void RemoveBookmarkBatch(List<BookmarkInfo> bookmarks) => ActionManager.RemoveBookmarkBatch(bookmarks, true);

        public void EditBookmark(BookmarkInfo bookmark, string note) => ActionManager.EditBookmark(bookmark, note, true);

        public void ChangeBookmarkBatchOffset(List<BookmarkInfo> bookmarks, int offset) =>
            ActionManager.ChangeBookmarkBatchOffset(bookmarks, offset, true);

        public void PlaceTimingGroup(string id, TimingGroup timingGroup) => ActionManager.CreateTimingGroup(id, timingGroup, true);

        public void RemoveTimingGroup(string id) => ActionManager.RemoveTimingGroup(id, true);

        public void RenameTimingGroup(string id, string newId) => ActionManager.RenameTimingGroup(id, newId, true);

        public void ChangeTimingGroupColor(string id, int r, int g, int b) =>
            ActionManager.ChangeTimingGroupColor(id, new Color(r, g, b), true);

        public void MoveHitObjectsToTimingGroup(string timingGroupId, List<HitObjectInfo> hitObjects) =>
            ActionManager.MoveHitObjectsToTimingGroup(timingGroupId, hitObjects, true);
    }
}
