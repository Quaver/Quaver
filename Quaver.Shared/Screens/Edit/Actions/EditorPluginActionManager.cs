using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Remove;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Resize;
using Quaver.Shared.Screens.Edit.Actions.SV.Add;
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
        public void Perform(IEditorAction action) => ActionManager.Perform(action);

        /// <summary>
        /// </summary>
        /// <param name="actions"></param>
        public void PerformBatch(List<IEditorAction> actions) => ActionManager.PerformBatch(actions);

        /// <summary>
        /// </summary>
        /// <param name="h"></param>
        public void PlaceHitObject(HitObjectInfo h) => ActionManager.PlaceHitObject(h);

        /// <summary>
        /// </summary>
        /// <param name="lane"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="layer"></param>
        /// <param name="hitsounds"></param>
        public HitObjectInfo PlaceHitObject(int lane, int startTime, int endTime = 0, int layer = 0, HitSounds hitsounds = 0)
            => ActionManager.PlaceHitObject(lane, startTime, endTime, layer, hitsounds);

        /// <summary>
        /// </summary>
        /// <param name="hitObjects"></param>
        public void PlaceHitObjectBatch(List<HitObjectInfo> hitObjects) => ActionManager.PlaceHitObjectBatch(hitObjects);

        /// <summary>
        /// </summary>
        /// <param name="h"></param>
        public void RemoveHitObject(HitObjectInfo h) => ActionManager.RemoveHitObject(h);

        /// <summary>
        /// </summary>
        /// <param name="hitObjects"></param>
        public void RemoveHitObjectBatch(List<HitObjectInfo> hitObjects) =>
            ActionManager.RemoveHitObjectBatch(hitObjects);

        /// <summary>
        /// </summary>
        /// <param name="h"></param>
        /// <param name="originalTime"></param>
        /// <param name="time"></param>
        public void ResizeLongNote(HitObjectInfo h, int originalTime, int time) => ActionManager.ResizeLongNote(h, originalTime, time);

        /// <summary>
        /// </summary>
        /// <param name="sv"></param>
        public void PlaceScrollVelocity(SliderVelocityInfo sv) => ActionManager.PlaceScrollVelocity(sv);

        /// <summary>
        /// </summary>
        /// <param name="svs"></param>
        public void PlaceScrollVelocityBatch(List<SliderVelocityInfo> svs) => ActionManager.PlaceScrollVelocityBatch(svs);

        /// <summary>
        /// </summary>
        /// <param name="sv"></param>
        public void RemoveScrollVelocity(SliderVelocityInfo sv) => ActionManager.RemoveScrollVelocityBatch(new List<SliderVelocityInfo> { sv });

        /// <summary>
        /// </summary>
        /// <param name="svs"></param>
        public void RemoveScrollVelocityBatch(List<SliderVelocityInfo> svs) => ActionManager.RemoveScrollVelocityBatch(svs);

        /// <summary>
        /// </summary>
        /// <param name="tp"></param>
        public void PlaceTimingPoint(TimingPointInfo tp) => ActionManager.PlaceTimingPoint(tp);

        /// <summary>
        /// </summary>
        /// <param name="tp"></param>
        public void RemoveTimingPoint(TimingPointInfo tp) => ActionManager.RemoveTimingPoint(tp);

        /// <summary>
        /// </summary>
        /// <param name="tps"></param>
        public void PlaceTimingPointBatch(List<TimingPointInfo> tps) => ActionManager.PlaceTimingPointBatch(tps);

        /// <summary>
        /// </summary>
        /// <param name="tps"></param>
        public void RemoveTimingPointBatch(List<TimingPointInfo> tps) => ActionManager.RemoveTimingPointBatch(tps);

        /// <summary>
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="offset"></param>
        public void ChangeTimingPointOffset(TimingPointInfo tp, float offset) => ActionManager.ChangeTimingPointOffset(tp, offset);

        /// <summary>
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="bpm"></param>
        public void ChangeTimingPointBpm(TimingPointInfo tp, float bpm) => ActionManager.ChangeTimingPointBpm(tp, bpm);

        /// <summary>
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="hidden"></param>
        public void ChangeTimingPointHidden(TimingPointInfo tp, bool hidden) => ActionManager.ChangeTimingPointHidden(tp, hidden);

        /// <summary>
        /// </summary>
        /// <param name="tps"></param>
        /// <param name="bpm"></param>
        public void ChangeTimingPointBpmBatch(List<TimingPointInfo> tps, float bpm) => ActionManager.ChangeTimingPointBpmBatch(tps, bpm);

        /// <summary>
        /// </summary>
        /// <param name="tps"></param>
        /// <param name="offset"></param>
        public void ChangeTimingPointOffsetBatch(List<TimingPointInfo> tps, float offset) => ActionManager.ChangeTimingPointOffsetBatch(tps, offset);

        /// <summary>
        /// </summary>
        /// <param name="tp"></param>
        public void ResetTimingPoint(TimingPointInfo tp) => ActionManager.ResetTimingPoint(tp);

        /// <summary>
        /// </summary>
        /// <param name="input"></param>
        public void GoToObjects(string input) => ActionManager.GoToObjects(input);

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
        public void SetPreviewTime(int time) => ActionManager.SetPreviewTime(time);

        /// <summary>
        /// </summary>
        /// <param name="layer"></param>
        public void CreateLayer(EditorLayerInfo layer, int index = -1) => ActionManager.CreateLayer(layer, index);

        /// <summary>
        /// </summary>
        /// <param name="layer"></param>
        public void RemoveLayer(EditorLayerInfo layer) => ActionManager.RemoveLayer(layer);

        /// <summary>
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="name"></param>
        public void RenameLayer(EditorLayerInfo layer, string name) => ActionManager.RenameLayer(layer, name);

        /// <summary>
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="hitObjects"></param>
        public void MoveHitObjectsToLayer(EditorLayerInfo layer, List<HitObjectInfo> hitObjects) => ActionManager.MoveHitObjectsToLayer(layer, hitObjects);

        /// <summary>
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="color"></param>
        public void ChangeLayerColor(EditorLayerInfo layer, int r, int g, int b) => ActionManager.ChangeLayerColor(layer, new Color(r, g, b));

        /// <summary>
        /// </summary>
        /// <param name="layer"></param>
        public void ToggleLayerVisibility(EditorLayerInfo layer) => ActionManager.ToggleLayerVisibility(layer);

        /// <summary>
        /// </summary>
        /// <param name="snaps"></param>
        /// <param name="hitObjectsToResnap"></param>
        public void ResnapNotes(List<int> snaps, List<HitObjectInfo> hitObjectsToResnap) =>
            ActionManager.ResnapNotes(snaps, hitObjectsToResnap);

        public void AddBookmark(int time, string note) => ActionManager.AddBookmark(time, note);

        public void RemoveBookmark(BookmarkInfo bookmark) => ActionManager.RemoveBookmark(bookmark);

        public void EditBookmark(BookmarkInfo bookmark, string note) => ActionManager.EditBookmark(bookmark, note);

        public void ChangeBookmarkBatchOffset(List<BookmarkInfo> bookmarks, int offset) => ActionManager.ChangeBookmarkBatchOffset(bookmarks, offset);
    }
}