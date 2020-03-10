using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Quaver.Shared.Scripting;

namespace Quaver.Shared.Screens.Edit.Plugins
{
    public class EditorPlugin : LuaImGui
    {
        /// <summary>
        /// </summary>
        private EditScreen Editor { get; }

        /// <summary>
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// </summary>
        public string Description { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="editScreen"></param>
        /// <param name="name"></param>
        /// <param name="author"></param>
        /// <param name="description"></param>
        /// <param name="filePath"></param>
        /// <param name="isResource"></param>
        public EditorPlugin(EditScreen editScreen, string name, string author, string description, string filePath, bool isResource = false) : base(filePath, isResource)
        {
            Editor = editScreen;
            Name = name;
            Author = author;
            Description = description;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void SetFrameState()
        {
            WorkingScript.Globals["utils"] = typeof(EditorPluginUtils);
            WorkingScript.Globals["actions"] = Editor.ActionManager.PluginActionManager;

            var state = (EditorPluginState) State;

            state.SongTime = (int) Math.Round(Editor.Track.Time, MidpointRounding.AwayFromZero);
            state.ScrollVelocities = Editor.WorkingMap.SliderVelocities;
            state.HitObjects = Editor.WorkingMap.HitObjects;

            base.SetFrameState();

            state.PushImguiStyle();
            PushDefaultStyles();
        }

        /// <summary>
        ///     Called after rendering the plugin to pop the default style vars
        /// </summary>
        public override void AfterRender()
        {
            ImGui.PopStyleVar();
            base.AfterRender();
        }

        /// <summary>
        ///     To push any default styling for plugin windows
        /// </summary>
        private void PushDefaultStyles()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(12, 4));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override LuaPluginState GetStateObject() => new EditorPluginState(Options);
    }
}