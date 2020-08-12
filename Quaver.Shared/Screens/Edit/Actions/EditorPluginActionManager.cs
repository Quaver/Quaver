using System.Collections.Generic;
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
    }
}
