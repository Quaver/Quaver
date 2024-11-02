using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Scripting;
using Wobble.Graphics.ImGUI;

#pragma warning disable CA1822

namespace Quaver.Shared.Screens.Edit.Plugins
{
    [MoonSharpUserData]
    public class EditorPluginState : LuaPluginState
    {
        /// <summary>
        ///     The current time in the song
        /// </summary>
        public double SongTime => EditorPluginUtils.EditScreen.Track.Time;

        /// <summary>
        ///     The objects that are currently selected by the user
        /// </summary>
        public List<HitObjectInfo> SelectedHitObjects => EditorPluginUtils.EditScreen.SelectedHitObjects.Value;

        /// <summary>
        ///     Current scroll group id we're working on
        /// </summary>
        public string SelectedScrollGroupId => EditorPluginUtils.EditScreen.SelectedScrollGroupId;

        /// <summary>
        ///     Current scroll group we're working on
        /// </summary>
        public ScrollGroup SelectedScrollGroup => EditorPluginUtils.EditScreen.SelectedScrollGroup;
        
        /// <summary>
        ///     The current scroll velocity in the map
        /// </summary>
        public BookmarkInfo CurrentBookmark => EditorPluginUtils.EditScreen.WorkingMap.GetBookmarkAt((int)SongTime);

        /// <summary>
        ///     The current scroll velocity in the map
        /// </summary>
        public SliderVelocityInfo CurrentScrollVelocity =>
            EditorPluginUtils.EditScreen.WorkingMap.GetScrollVelocityAt(SongTime);

        /// <summary>
        ///     The current timing point in the map
        /// </summary>
        public TimingPointInfo CurrentTimingPoint => EditorPluginUtils.EditScreen.WorkingMap.GetTimingPointAt(SongTime);

        /// <summary>
        ///     The currently selected editor layer
        /// </summary>
        public EditorLayerInfo CurrentLayer =>
            EditorPluginUtils.EditScreen.SelectedLayer.Value ?? EditorPluginUtils.EditScreen.DefaultLayer;

        /// <summary>
        ///     The currently selected beat snap
        /// </summary>
        public int CurrentSnap => EditorPluginUtils.EditScreen.BeatSnap.Value;

        /// <summary>
        ///     ImGui options used to set styles/fonts for the window
        /// </summary>
        [MoonSharpVisible(false)]
        private ImGuiOptions Options { get; }

        [MoonSharpVisible(false)]
        public EditorPluginState(ImGuiOptions options) => Options = options;

        /// <summary>
        ///     Pushes all styles to the current imgui context
        /// </summary>
        public void PushImguiStyle() => ImGui.PushFont(Options.Fonts.First().Context);

        /// <summary>
        ///     Creates the deep copy of this instance.
        /// </summary>
        /// <returns>The deep copy of this instance.</returns>
        public override LuaPluginState Clone(Converter<DynValue, DynValue> nonconvertible) =>
            new EditorPluginState(Options) { IsWindowHovered = IsWindowHovered, Values = CloneValues(nonconvertible) };
    }
}
