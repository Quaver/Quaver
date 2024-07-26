using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using MoonSharp.Interpreter;
using Quaver.API.Enums;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Scripting;

namespace Quaver.Shared.Screens.Edit.Plugins
{
    public class EditorPlugin : LuaImGui, IEditorPlugin
    {
        /// <summary>
        /// </summary>
        private EditScreen Editor { get; }

        /// <summary>
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// </summary>
        public bool IsWindowHovered { get; set; }

        /// <summary>
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     If the plugin is built into the editor
        /// </summary>
        public bool IsBuiltIn { get; set; }

        public string Directory { get; set; }

        public bool IsWorkshop { get; set; }

        public EditorPluginMap EditorPluginMap { get; set; }

        static EditorPlugin()
        {
            UserData.RegisterType<GameMode>();
            UserData.RegisterType<HitSounds>();
            UserData.RegisterType<TimeSignature>();
            UserData.RegisterType<EditorActionType>();
            RegisterEnumConversion(typeof(GameMode));
            RegisterEnumConversion(typeof(HitSounds));
            RegisterEnumConversion(typeof(TimeSignature));
            RegisterEnumConversion(typeof(EditorActionType));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="editScreen"></param>
        /// <param name="name"></param>
        /// <param name="author"></param>
        /// <param name="description"></param>
        /// <param name="filePath"></param>
        /// <param name="isResource"></param>
        /// <param name="directory"></param>
        /// <param name="isWorkshop"></param>
        public EditorPlugin(EditScreen editScreen, string name, string author, string description, string filePath,
            bool isResource = false, string directory = null, bool isWorkshop = false) : base(filePath, isResource, name)
        {
            Editor = editScreen;
            Author = author;
            Description = description;
            IsBuiltIn = isResource;
            Directory = directory;
            IsWorkshop = isWorkshop;
            EditorPluginUtils.EditScreen = editScreen;
            EditorPluginMap = new EditorPluginMap();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Initialize()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void SetFrameState()
        {
            if (IsFirstDrawCall)
            {
                WorkingScript.Globals["utils"] = typeof(EditorPluginUtils);
                WorkingScript.Globals["game_mode"] = typeof(GameMode);
                WorkingScript.Globals["hitsounds"] = typeof(HitSounds);
                WorkingScript.Globals["time_signature"] = typeof(TimeSignature);
                WorkingScript.Globals["action_type"] = typeof(EditorActionType);
                WorkingScript.Globals["actions"] = Editor.ActionManager.PluginActionManager;
                WorkingScript.Globals["map"] = EditorPluginMap;
            }

            base.SetFrameState();
            ((EditorPluginState)State).PushImguiStyle();
            PushDefaultStyles();
        }

        /// <summary>
        ///     Called after rendering the plugin to pop the default style vars
        /// </summary>
        public override void AfterRender()
        {
            ImGui.PopStyleVar();
            IsWindowHovered = State.IsWindowHovered;
            base.AfterRender();
        }

        /// <summary>
        ///     To push any default styling for plugin windows
        /// </summary>
        private static void PushDefaultStyles() => ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(12, 4));

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override LuaPluginState GetStateObject() => new EditorPluginState(Options);
    }
}
