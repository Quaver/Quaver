using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text;
using ImGuiNET;
using MoonSharp.Interpreter;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.UI.Menu;
using Wobble;
using Wobble.Graphics.ImGUI;
using Wobble.Logging;

namespace Quaver.Shared.Scripting
{
    public class LuaImGui : SpriteImGui
    {
        /// <summary>
        /// </summary>
        protected Script WorkingScript { get; private set; }

        /// <summary>
        /// </summary>
        private string FilePath { get; }

        /// <summary>
        /// </summary>
        private bool IsResource { get; }

        /// <summary>
        /// </summary>
        private string ScriptText { get; set; }

        /// <summary>
        /// </summary>
        private FileSystemWatcher Watcher { get; }

        /// <summary>
        /// </summary>
        public LuaPluginState State { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="isResource"></param>
        public LuaImGui(string filePath, bool isResource = false) : base(true, EditorFileMenuBar.GetOptions())
        {
            FilePath = filePath;
            IsResource = isResource;

            // ReSharper disable once VirtualMemberCallInConstructor
            State = GetStateObject();

            UserData.RegisterAssembly(Assembly.GetCallingAssembly());
            UserData.RegisterAssembly(typeof(SliderVelocityInfo).Assembly);
            UserData.RegisterType<ImGuiInputTextFlags>();
            UserData.RegisterType<ImGuiDataType>();
            UserData.RegisterType<ImGuiTreeNodeFlags>();
            UserData.RegisterType<ImGuiSelectableFlags>();
            UserData.RegisterType<ImGuiMouseCursor>();
            UserData.RegisterType<ImGuiCond>();
            UserData.RegisterType<ImGuiWindowFlags>();
            UserData.RegisterType<ImGuiDir>();
            UserData.RegisterType<ImGuiDragDropFlags>();
            UserData.RegisterType<ImGuiTabBarFlags>();
            UserData.RegisterType<ImGuiTabItemFlags>();
            UserData.RegisterType<ImGuiColorEditFlags>();

            RegisterAllVectors();
            LoadScript();

            if (IsResource)
                return;

            Watcher = new FileSystemWatcher(Path.GetDirectoryName(filePath))
            {
                Filter = Path.GetFileName(filePath)
            };

            Watcher.Changed += OnFileChanged;
            Watcher.Created += OnFileChanged;
            Watcher.Deleted += OnFileChanged;

            // Begin watching.
            Watcher.EnableRaisingEvents = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Watcher?.Dispose();
            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void RenderImguiLayout()
        {
            try
            {
                SetFrameState();
                WorkingScript.Call(WorkingScript.Globals["draw"]);
                AfterRender();
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Returns an object with the state the plugin has
        /// </summary>
        /// <returns></returns>
        public virtual LuaPluginState GetStateObject() => new LuaPluginState();

        /// <summary>
        ///     Sets the state of the plugin for this frame
        /// </summary>
        public virtual void SetFrameState()
        {
            State.DeltaTime = GameBase.Game.TimeSinceLastFrame;
            WorkingScript.Globals["imgui_input_text_flags"] = typeof(ImGuiInputTextFlags);
            WorkingScript.Globals["imgui_data_type"] = typeof(ImGuiDataType);
            WorkingScript.Globals["imgui_tree_node_flags"] = typeof(ImGuiTreeNodeFlags);
            WorkingScript.Globals["imgui_selectable_flags"] = typeof(ImGuiSelectableFlags);
            WorkingScript.Globals["imgui_mouse_cursor"] = typeof(ImGuiMouseCursor);
            WorkingScript.Globals["imgui_cond"] = typeof(ImGuiCond);
            WorkingScript.Globals["imgui_window_flags"] = typeof(ImGuiWindowFlags);
            WorkingScript.Globals["imgui_dir"] = typeof(ImGuiDir);
            WorkingScript.Globals["imgui_drag_drop_flags"] = typeof(ImGuiDragDropFlags);
            WorkingScript.Globals["imgui_tab_bar_flags"] = typeof(ImGuiTabBarFlags);
            WorkingScript.Globals["imgui_tab_item_flags"] = typeof(ImGuiTabItemFlags);
            WorkingScript.Globals["imgui_color_edit_flags"] = typeof(ImGuiColorEditFlags);
        }

        /// <summary>
        ///     After the plugin has been render, this will be called.
        ///     Should be used to pop any styles
        /// </summary>
        public virtual void AfterRender()
        {
        }

        /// <summary>
        ///     Loads the text from the script
        /// </summary>
        private void LoadScript()
        {
            WorkingScript = new Script(CoreModules.Preset_HardSandbox);

            try
            {
                if (IsResource)
                {
                    var buffer = GameBase.Game.Resources.Get(FilePath);
                    ScriptText = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                }
                else
                {
                    ScriptText = File.ReadAllText(FilePath);
                }

                WorkingScript.DoString(ScriptText);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }

            WorkingScript.Globals["imgui"] = typeof(ImGuiWrapper);
            WorkingScript.Globals["state"] = State;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            LoadScript();
            Logger.Important($"Script: {FilePath} has been loaded", LogType.Runtime);
        }

        /// <summary>
        ///     Handles registering the Vector types for the script
        /// </summary>
        private void RegisterAllVectors()
        {
            // Vector 2
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Vector2),
                dynVal => {
                    var table = dynVal.Table;
                    var x = (float)(double)table[1];
                    var y = (float)(double)table[2];
                    return new Vector2(x, y);
                }
            );

            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Vector2>(
                (script, vector) => {
                    var x = DynValue.NewNumber(vector.X);
                    var y = DynValue.NewNumber(vector.Y);
                    var dynVal = DynValue.NewTable(script, x, y);
                    return dynVal;
                }
            );

            // Vector3
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Vector3),
                dynVal => {
                    var table = dynVal.Table;
                    var x = (float)((double)table[1]);
                    var y = (float)((double)table[2]);
                    var z = (float)((double)table[3]);
                    return new Vector3(x, y, z);
                }
            );

            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Vector3>(
                (script, vector) => {
                    var x = DynValue.NewNumber(vector.X);
                    var y = DynValue.NewNumber(vector.Y);
                    var z = DynValue.NewNumber(vector.Z);
                    var dynVal = DynValue.NewTable(script, x, y, z);
                    return dynVal;
                }
            );
        }
    }
}