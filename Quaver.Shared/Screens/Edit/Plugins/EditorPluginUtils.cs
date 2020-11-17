using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.Batch;
using Quaver.Shared.Screens.Edit.Actions.HitObjects;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Place;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.PlaceBatch;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Remove;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.RemoveBatch;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Resize;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Resnap;
using Quaver.Shared.Screens.Edit.Actions.Layers.Colors;
using Quaver.Shared.Screens.Edit.Actions.Layers.Create;
using Quaver.Shared.Screens.Edit.Actions.Layers.Move;
using Quaver.Shared.Screens.Edit.Actions.Layers.Remove;
using Quaver.Shared.Screens.Edit.Actions.Layers.Rename;
using Quaver.Shared.Screens.Edit.Actions.Layers.Visibility;
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
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeOffset;
using Quaver.Shared.Screens.Edit.Actions.Timing.ChangeOffsetBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.Remove;
using Quaver.Shared.Screens.Edit.Actions.Timing.RemoveBatch;
using Quaver.Shared.Screens.Edit.Actions.Timing.Reset;
using Wobble.Input;

namespace Quaver.Shared.Screens.Edit.Plugins
{
    [MoonSharpUserData]
    public static class EditorPluginUtils
    {
        [MoonSharpVisible(false)]
        public static EditScreen EditScreen;

        /// <summary>
        /// </summary>
        /// <param name="time"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        public static SliderVelocityInfo CreateScrollVelocity(float time, float multiplier)
        {
            var sv = new SliderVelocityInfo()
            {
                StartTime = time,
                Multiplier = multiplier,
                IsEditableInLuaScript = true
            };

            return sv;
        }

        /// <summary>
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="lane"></param>
        /// <param name="endTime"></param>
        /// <param name="hitsounds"></param>
        /// <returns></returns>
        public static HitObjectInfo CreateHitObject(int startTime, int lane, int endTime = 0, HitSounds hitsounds = 0, int editorLayer = 0)
        {
            var ho = new HitObjectInfo()
            {
                StartTime = startTime,
                Lane = lane,
                EndTime = endTime,
                HitSound = hitsounds,
                EditorLayer = editorLayer
            };

            return ho;
        }

        /// <summary>
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="bpm"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public static TimingPointInfo CreateTimingPoint(float startTime, float bpm, TimeSignature signature = TimeSignature.Quadruple)
        {
            var tp = new TimingPointInfo()
            {
                StartTime = startTime,
                Bpm = bpm,
                Signature = signature
            };

            return tp;
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="hidden"></param>
        /// <param name="colorRgb"></param>
        /// <returns></returns>
        public static EditorLayerInfo CreateEditorLayer(string name, bool hidden = false, string colorRgb = null)
        {
            var layer = new EditorLayerInfo()
            {
                Name = name,
                Hidden = hidden,
                ColorRgb = colorRgb
            };

            return layer;
        }

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <returns></returns>
        public static IEditorAction CreateEditorAction(EditorActionType type, DynValue arg1 = null, DynValue arg2 = null, DynValue arg3 = null, DynValue arg4 = null)
        {
            switch (type)
            {
                case EditorActionType.PlaceHitObject:
                    return new EditorActionPlaceHitObject(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<HitObjectInfo>());
                case EditorActionType.RemoveHitObject:
                    return new EditorActionRemoveHitObject(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<HitObjectInfo>());
                case EditorActionType.ResizeLongNote:
                    return new EditorActionResizeLongNote(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<HitObjectInfo>(), arg2.ToObject<int>(), arg3.ToObject<int>());
                case EditorActionType.RemoveHitObjectBatch:
                    return new EditorActionRemoveHitObjectBatch(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<List<HitObjectInfo>>());
                case EditorActionType.PlaceHitObjectBatch:
                    return new EditorActionPlaceHitObjectBatch(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<List<HitObjectInfo>>());
                case EditorActionType.CreateLayer:
                    return new EditorActionCreateLayer(EditScreen.WorkingMap, EditScreen.ActionManager, EditScreen.SelectedHitObjects, arg1.ToObject<EditorLayerInfo>());
                case EditorActionType.RemoveLayer:
                    return new EditorActionRemoveLayer(EditScreen.ActionManager, EditScreen.WorkingMap, EditScreen.SelectedHitObjects, arg1.ToObject<EditorLayerInfo>());
                case EditorActionType.RenameLayer:
                    return new EditorActionRenameLayer(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<EditorLayerInfo>(), arg2.ToObject<string>());
                case EditorActionType.MoveToLayer:
                    return new EditorActionMoveObjectsToLayer(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<EditorLayerInfo>(), arg2.ToObject<List<HitObjectInfo>>());
                case EditorActionType.ColorLayer:
                    return new EditorActionChangeLayerColor(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<EditorLayerInfo>(), new Color(arg2.ToObject<int>(), arg3.ToObject<int>(), arg4.ToObject<int>()));
                case EditorActionType.ToggleLayerVisibility:
                    return new EditorActionToggleLayerVisibility(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<EditorLayerInfo>());
                case EditorActionType.AddScrollVelocity:
                    return new EditorActionAddScrollVelocity(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<SliderVelocityInfo>());
                case EditorActionType.RemoveScrollVelocity:
                    return new EditorActionRemoveScrollVelocity(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<SliderVelocityInfo>());
                case EditorActionType.AddScrollVelocityBatch:
                    return new EditorActionAddScrollVelocityBatch(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<List<SliderVelocityInfo>>());
                case EditorActionType.RemoveScrollVelocityBatch:
                    return new EditorActionRemoveScrollVelocityBatch(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<List<SliderVelocityInfo>>());
                case EditorActionType.AddTimingPoint:
                    return new EditorActionAddTimingPoint(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<TimingPointInfo>());
                case EditorActionType.RemoveTimingPoint:
                    return new EditorActionRemoveTimingPoint(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<TimingPointInfo>());
                case EditorActionType.AddTimingPointBatch:
                    return new EditorActionAddTimingPointBatch(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<List<TimingPointInfo>>());
                case EditorActionType.RemoveTimingPointBatch:
                    return new EditorActionRemoveTimingPointBatch(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<List<TimingPointInfo>>());
                case EditorActionType.ChangeTimingPointOffset:
                    return new EditorActionChangeTimingPointOffset(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<TimingPointInfo>(), arg2.ToObject<float>());
                case EditorActionType.ChangeTimingPointBpm:
                    return new EditorActionChangeTimingPointBpm(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<TimingPointInfo>(), arg2.ToObject<float>());
                case EditorActionType.ResetTimingPoint:
                    return new EditorActionResetTimingPoint(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<TimingPointInfo>());
                case EditorActionType.ChangeTimingPointBpmBatch:
                    return new EditorActionChangeTimingPointBpmBatch(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<List<TimingPointInfo>>(), arg2.ToObject<float>());
                case EditorActionType.ChangeTimingPointOffsetBatch:
                    return new EditorActionChangeTimingPointOffsetBatch(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<List<TimingPointInfo>>(), arg2.ToObject<float>());
                case EditorActionType.ChangeScrollVelocityOffsetBatch:
                    return new EditorActionChangeScrollVelocityOffsetBatch(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<List<SliderVelocityInfo>>(), arg2.ToObject<float>());
                case EditorActionType.ChangeScrollVelocityMultiplierBatch:
                    return new EditorActionChangeScrollVelocityMultiplierBatch(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<List<SliderVelocityInfo>>(), arg2.ToObject<float>());
                case EditorActionType.ResnapHitObjects:
                    return new EditorActionResnapHitObjects(EditScreen.ActionManager, EditScreen.WorkingMap, arg1.ToObject<List<int>>(), arg2.ToObject<List<HitObjectInfo>>());
                case EditorActionType.Batch:
                    return new EditorActionBatch(EditScreen.ActionManager, arg1.ToObject<List<IEditorAction>>());
                default:
                    return null;
            }
        }

        /// <summary>
        ///     Converts milliseconds to the appropriate mm:ss:ms time
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string MillisecondsToTime(float time) => TimeSpan.FromMilliseconds(time).ToString(@"mm\:ss\.fff");

        public static bool IsKeyPressed(Keys k) => KeyboardManager.IsUniqueKeyPress(k);

        public static bool IsKeyReleased(Keys k) => KeyboardManager.IsUniqueKeyRelease(k);

        public static bool IsKeyDown(Keys k) => KeyboardManager.CurrentState.IsKeyDown(k);

        public static bool IsKeyUp(Keys k) => KeyboardManager.CurrentState.IsKeyUp(k);
    }
}