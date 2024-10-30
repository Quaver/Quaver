using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.Batch;
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
using Quaver.Shared.Screens.Edit.Actions.Offset;
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
            var sv = new SliderVelocityInfo
            {
                StartTime = time,
                Multiplier = multiplier,
                IsEditableInLuaScript = true,
            };

            return sv;
        }

        /// <summary>
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="lane"></param>
        /// <param name="endTime"></param>
        /// <param name="hitsounds"></param>
        /// <param name="editorLayer"></param>
        /// <returns></returns>
        public static HitObjectInfo CreateHitObject(int startTime, int lane, int endTime = 0, HitSounds hitsounds = 0, int editorLayer = 0)
        {
            var ho = new HitObjectInfo
            {
                StartTime = startTime,
                Lane = lane,
                EndTime = endTime,
                HitSound = hitsounds,
                EditorLayer = editorLayer,
            };

            return ho;
        }

        /// <summary>
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="bpm"></param>
        /// <param name="signature"></param>
        /// <param name="hidden"></param>
        /// <returns></returns>
        public static TimingPointInfo CreateTimingPoint(float startTime, float bpm, TimeSignature signature = TimeSignature.Quadruple, bool hidden = false)
        {
            var tp = new TimingPointInfo
            {
                StartTime = startTime,
                Bpm = bpm,
                Signature = signature,
                Hidden = hidden,
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
            var layer = new EditorLayerInfo
            {
                Name = name,
                Hidden = hidden,
                ColorRgb = colorRgb,
            };

            return layer;
        }

        /// <summary>
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        public static BookmarkInfo CreateBookmark(int startTime, string note)
        {
            var layer = new BookmarkInfo
            {
                StartTime = startTime,
                Note = note,
            };

            return layer;
        }

        /// <summary>
        /// </summary>
        /// <param name="svs"></param>
        /// <param name="initialSv"></param>
        /// <param name="colorRgb"></param>
        /// <returns></returns>
        public static ScrollGroup CreateScrollGroup(List<SliderVelocityInfo> svs, float initialSv = 1.0f, string colorRgb = default)
        {
            return new ScrollGroup
            {
                ColorRgb = colorRgb,
                InitialScrollVelocity = initialSv,
                ScrollVelocities = svs
            };
        }

        /// <summary>
        ///     Generates a unique ID for a new timing group
        /// </summary>
        /// <returns></returns>
        public static string GenerateTimingGroupId()
        {
            const string newGroupPrefix = "SG_";
            var newGroupNumber = 0;
            string newGroupId;
            while (EditScreen.WorkingMap.TimingGroups.ContainsKey(newGroupId = $"{newGroupPrefix}{newGroupNumber}"))
                newGroupNumber++;
            return newGroupId;
        }

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IEditorAction CreateEditorAction(EditorActionType type, params DynValue[] args) =>
            type switch
            {
                EditorActionType.PlaceHitObject => new EditorActionPlaceHitObject(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<HitObjectInfo>()
                ),
                EditorActionType.RemoveHitObject => new EditorActionRemoveHitObject(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<HitObjectInfo>()
                ),
                EditorActionType.ResizeLongNote => new EditorActionResizeLongNote(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<HitObjectInfo>(),
                    args[1].ToObject<int>(),
                    args[2].ToObject<int>()
                ),
                EditorActionType.RemoveHitObjectBatch => new EditorActionRemoveHitObjectBatch(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<List<HitObjectInfo>>()
                ),
                EditorActionType.PlaceHitObjectBatch => new EditorActionPlaceHitObjectBatch(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<List<HitObjectInfo>>()
                ),
                EditorActionType.AddHitsound => new EditorActionAddHitsound(
                    EditScreen.ActionManager,
                    args[0].ToObject<List<HitObjectInfo>>(),
                    args[1].ToObject<HitSounds>()
                ),
                EditorActionType.RemoveHitsound => new EditorActionRemoveHitsound(
                    EditScreen.ActionManager,
                    args[0].ToObject<List<HitObjectInfo>>(),
                    args[1].ToObject<HitSounds>()
                ),
                EditorActionType.CreateLayer => new EditorActionCreateLayer(
                    EditScreen.WorkingMap,
                    EditScreen.ActionManager,
                    EditScreen.SelectedHitObjects,
                    args[0].ToObject<EditorLayerInfo>()
                ),
                EditorActionType.RemoveLayer => new EditorActionRemoveLayer(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    EditScreen.SelectedHitObjects,
                    args[0].ToObject<EditorLayerInfo>()
                ),
                EditorActionType.RenameLayer => new EditorActionRenameLayer(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<EditorLayerInfo>(),
                    args[1].ToObject<string>()
                ),
                EditorActionType.MoveToLayer => new EditorActionMoveObjectsToLayer(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<EditorLayerInfo>(),
                    args[1].ToObject<List<HitObjectInfo>>()
                ),
                EditorActionType.ColorLayer => new EditorActionChangeLayerColor(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<EditorLayerInfo>(),
                    new(args[1].ToObject<int>(), args[2].ToObject<int>(), args[3].ToObject<int>())
                ),
                EditorActionType.ToggleLayerVisibility => new EditorActionToggleLayerVisibility(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<EditorLayerInfo>()
                ),
                EditorActionType.AddScrollVelocity => new EditorActionAddScrollVelocity(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<SliderVelocityInfo>(),
                    args.Length > 1 ? args[1].ToObject<ScrollGroup>() : null
                ),
                EditorActionType.RemoveScrollVelocity => new EditorActionRemoveScrollVelocity(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<SliderVelocityInfo>()
                ),
                EditorActionType.AddScrollVelocityBatch => new EditorActionAddScrollVelocityBatch(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<List<SliderVelocityInfo>>(),
                    args.Length > 1 ? args[1].ToObject<ScrollGroup>() : null
                ),
                EditorActionType.RemoveScrollVelocityBatch => new EditorActionRemoveScrollVelocityBatch(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<List<SliderVelocityInfo>>(),
                    args.Length > 1 ? args[1].ToObject<ScrollGroup>() : null
                ),
                EditorActionType.AddTimingPoint => new EditorActionAddTimingPoint(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<TimingPointInfo>()
                ),
                EditorActionType.RemoveTimingPoint => new EditorActionRemoveTimingPoint(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<TimingPointInfo>()
                ),
                EditorActionType.AddTimingPointBatch => new EditorActionAddTimingPointBatch(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<List<TimingPointInfo>>()
                ),
                EditorActionType.RemoveTimingPointBatch => new EditorActionRemoveTimingPointBatch(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<List<TimingPointInfo>>()
                ),
                EditorActionType.ChangeTimingPointOffset => new EditorActionChangeTimingPointOffset(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<TimingPointInfo>(),
                    args[1].ToObject<float>()
                ),
                EditorActionType.ChangeTimingPointBpm => new EditorActionChangeTimingPointBpm(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<TimingPointInfo>(),
                    args[1].ToObject<float>()
                ),
                EditorActionType.ResetTimingPoint => new EditorActionResetTimingPoint(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<TimingPointInfo>()
                ),
                EditorActionType.ChangeTimingPointBpmBatch => new EditorActionChangeTimingPointBpmBatch(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<List<TimingPointInfo>>(),
                    args[1].ToObject<float>()
                ),
                EditorActionType.ChangeTimingPointOffsetBatch => new EditorActionChangeTimingPointOffsetBatch(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<List<TimingPointInfo>>(),
                    args[1].ToObject<float>()
                ),
                EditorActionType.ChangeScrollVelocityOffsetBatch => new EditorActionChangeScrollVelocityOffsetBatch(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<List<SliderVelocityInfo>>(),
                    args[1].ToObject<float>()
                ),
                EditorActionType.ChangeScrollVelocityMultiplierBatch => new
                    EditorActionChangeScrollVelocityMultiplierBatch(
                        EditScreen.ActionManager,
                        EditScreen.WorkingMap,
                        args[0].ToObject<List<SliderVelocityInfo>>(),
                        args[1].ToObject<float>()
                    ),
                EditorActionType.AddBookmark => new EditorActionAddBookmark(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<BookmarkInfo>()
                ),
                EditorActionType.RemoveBookmark => new EditorActionRemoveBookmark(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<BookmarkInfo>()
                ),
                EditorActionType.AddBookmarkBatch => new EditorActionAddBookmarkBatch(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<List<BookmarkInfo>>()
                ),
                EditorActionType.RemoveBookmarkBatch => new EditorActionRemoveBookmarkBatch(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<List<BookmarkInfo>>()
                ),
                EditorActionType.EditBookmark => new EditorActionEditBookmark(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<BookmarkInfo>(),
                    args[1].ToObject<string>()
                ),
                EditorActionType.ChangeBookmarkOffsetBatch => new EditorActionChangeBookmarkOffsetBatch(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<List<BookmarkInfo>>(),
                    args[1].ToObject<int>()
                ),
                EditorActionType.ResnapHitObjects => new EditorActionResnapHitObjects(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<List<int>>(),
                    args[1].ToObject<List<HitObjectInfo>>(),
                    args[2].ToObject<bool>()
                ),
                EditorActionType.Batch => new EditorActionBatch(
                    EditScreen.ActionManager,
                    args[0].ToObject<List<IEditorAction>>()
                ),
                EditorActionType.FlipHitObjects => new EditorActionFlipHitObjects(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<List<HitObjectInfo>>()
                ),
                EditorActionType.SwapLanes => new EditorActionSwapLanes(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<List<HitObjectInfo>>(),
                    args[1].ToObject<int>(),
                    args[2].ToObject<int>()
                ),
                EditorActionType.MoveHitObjects => new EditorActionMoveHitObjects(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<List<HitObjectInfo>>(),
                    args[1].ToObject<int>(),
                    args[2].ToObject<int>(),
                    args.Length < 4 || args[3].ToObject<bool>()
                ),
                EditorActionType.ChangePreviewTime => new EditorActionChangePreviewTime(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<int>()
                ),
                EditorActionType.ChangeTimingPointSignature => new EditorActionChangeTimingPointSignature(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<TimingPointInfo>(),
                    args[1].ToObject<int>()
                ),
                EditorActionType.ChangeTimingPointHidden => new EditorActionChangeTimingPointHidden(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<TimingPointInfo>(),
                    args[1].ToObject<bool>()
                ),
                EditorActionType.ChangeTimingPointSignatureBatch => new EditorActionChangeTimingPointSignatureBatch(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<List<TimingPointInfo>>(),
                    args[1].ToObject<int>()
                ),
                EditorActionType.ApplyOffset => new EditorActionApplyOffset(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<int>()
                ),
                EditorActionType.ReverseHitObjects => new EditorActionReverseHitObjects(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<List<HitObjectInfo>>()
                ),
                EditorActionType.CreateTimingGroup => new EditorActionCreateTimingGroup(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<string>(),
                    args[1].ToObject<TimingGroup>(),
                    args[2].ToObject<List<HitObjectInfo>>()
                ),
                EditorActionType.RemoveTimingGroup => new EditorActionRemoveTimingGroup(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<string>(),
                    args[1].ToObject<TimingGroup>(),
                    null
                ),
                EditorActionType.RenameTimingGroup => new EditorActionRenameTimingGroup(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<string>(),
                    args[1].ToObject<string>(),
                    null
                ),
                EditorActionType.MoveObjectsToTimingGroup => new EditorActionMoveObjectsToTimingGroup(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<List<HitObjectInfo>>(),
                    args[1].ToObject<string>()
                ),
                EditorActionType.ColorTimingGroup => new EditorActionChangeTimingGroupColor(
                    EditScreen.ActionManager,
                    EditScreen.WorkingMap,
                    args[0].ToObject<string>(),
                    new Color(args[1].ToObject<int>(), args[2].ToObject<int>(), args[3].ToObject<int>())
                ),
                EditorActionType.None => null,
                _ => null,
            };

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

        /// <summary>
        ///     Casts the value to a <see cref="Half"/>.
        /// </summary>
        /// <remarks><para>This is required for plugins that perform <see cref="Half"/> emulation.</para></remarks>
        /// <param name="value">The value to cast.</param>
        /// <returns>The parameter <paramref name="value"/> as a <see cref="Half"/>.</returns>
        // Return type needs to be `float` because `Half` is not registered and MoonSharp doesn't coerce to number.
        public static float ToHalf(double value) => (float)(Half)value;

        /// <summary>
        ///     Casts the value to a <see cref="float"/>.
        /// </summary>
        /// <remarks><para>This is required for plugins that perform <see cref="float"/> emulation.</para></remarks>
        /// <param name="value">The value to cast.</param>
        /// <returns>The parameter <paramref name="value"/> as a <see cref="float"/>.</returns>
        public static float ToFloat(double value) => (float)value;
    }
}
