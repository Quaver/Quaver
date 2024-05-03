using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Debugging;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Edit.UI.Menu;
using Wobble;
using Wobble.Graphics.ImGUI;
using Wobble.Logging;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

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

        // <summary>
        // Gets or sets the name of the plugin
        // </summary>
        public string Name { get; }

        /// <summary>
        /// </summary>
        private bool IsResource { get; }

        private DateTime LastWatcher { get; set; }

        private int FailedVersion { get; set; } = -1;

        private int LoadedVersion { get; set; } = -1;

        private int Version { get; set; }

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
        /// <param name="name"></param>
        public LuaImGui(string filePath, bool isResource = false, string name = null)
            : base(false, EditorFileMenuBar.GetOptions(), ConfigManager.EditorImGuiScalePercentage.Value / 100f)
        {
            FilePath = filePath;
            IsResource = isResource;
            Name = name ?? Path.GetFileName(Path.GetDirectoryName(FilePath));

            // ReSharper disable once VirtualMemberCallInConstructor
            State = GetStateObject();

            UserData.RegisterAssembly(Assembly.GetCallingAssembly());
            UserData.RegisterAssembly(typeof(SliderVelocityInfo).Assembly);

            // ImGui
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
            UserData.RegisterType<ImGuiKey>();
            UserData.RegisterType<ImGuiCol>();
            UserData.RegisterType<ImGuiStyleVar>();
            UserData.RegisterType<ImDrawListPtr>();
            UserData.RegisterType<ImGuiComboFlags>();
            UserData.RegisterType<ImGuiFocusedFlags>();
            UserData.RegisterType<ImGuiHoveredFlags>();

            // MonoGame
            UserData.RegisterType<Keys>();

            RegisterAllVectors();
            LazyLoadScript();

            if (IsResource)
                return;

            Watcher = new(Path.GetDirectoryName(filePath) ?? "") { Filter = Path.GetFileName(filePath) };
            Watcher.Changed += OnFileChanged;
            Watcher.Created += OnFileChanged;
            Watcher.Renamed += OnFileChanged;

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
            if (LoadedVersion == FailedVersion)
                return;

            try
            {
                SetFrameState();

                if (WorkingScript.Globals["draw"] is Closure draw)
                    WorkingScript.Call(draw);

                AfterRender();
            }
            catch (Exception e)
            {
                HandleLuaException(e);
            }
        }

        /// <summary>
        ///     Returns an object with the state the plugin has
        /// </summary>
        /// <returns></returns>
        public virtual LuaPluginState GetStateObject() => new();

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
            WorkingScript.Globals["imgui_key"] = typeof(ImGuiKey);
            WorkingScript.Globals["imgui_col"] = typeof(ImGuiCol);
            WorkingScript.Globals["imgui_style_var"] = typeof(ImGuiStyleVar);
            WorkingScript.Globals["imgui_combo_flags"] = typeof(ImGuiComboFlags);
            WorkingScript.Globals["imgui_focused_flags"] = typeof(ImGuiFocusedFlags);
            WorkingScript.Globals["imgui_hovered_flags"] = typeof(ImGuiHoveredFlags);
            WorkingScript.Globals["keys"] = typeof(Keys);
        }

        /// <summary>
        ///     After the plugin has been rendered, this will be called.
        ///     Should be used to pop any styles
        /// </summary>
        public virtual void AfterRender() { }

        /// <summary>
        ///     Loads the text from the script
        /// </summary>
        private void LazyLoadScript()
        {
            if (LoadedVersion == Version)
                return;

            LoadedVersion++;

            WorkingScript = new(CoreModules.Preset_HardSandbox | CoreModules.Dynamic)
            {
                Globals = { ["print"] = CallbackFunction.FromDelegate(null, Print), ["state"] = State },
            };

            try
            {
                LoadScriptText();

                if (WorkingScript.DoString(ScriptText) is var ret && ret.Type is not DataType.Void)
                    NotificationManager.Show(
                        NotificationLevel.Success,
                        $"Plugin \"{Name}\" has returned {ret} on initialization."
                    );
                else if (LoadedVersion > 0)
                    NotificationManager.Show(
                        NotificationLevel.Success,
                        $"Plugin \"{Name}\" was hot-reloaded successfully."
                    );
            }
            catch (Exception e)
            {
                HandleLuaException(e);
            }

            WorkingScript.Globals["imgui"] = typeof(ImGuiWrapper);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (DateTime.Now - LastWatcher < TimeSpan.FromMilliseconds(50))
                return;

            LastWatcher = DateTime.Now;
            Version++;
            Logger.Important($"Script: \"{FilePath}\" will be hot-reloaded. (Revision #{Version})", LogType.Runtime);
            LazyLoadScript();
        }

        /// <summary>
        ///     Handles registering the Vector types for the script
        /// </summary>
        private static void RegisterAllVectors()
        {
            // Vector 2
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(
                DataType.Table,
                typeof(Vector2),
                dynVal =>
                {
                    var table = dynVal.Table;
                    var x = (float)(double)table[1];
                    var y = (float)(double)table[2];
                    return new Vector2(x, y);
                }
            );

            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Vector2>(
                (script, vector) =>
                {
                    var x = DynValue.NewNumber(vector.X);
                    var y = DynValue.NewNumber(vector.Y);
                    var dynVal = DynValue.NewTable(script, x, y);
                    return dynVal;
                }
            );

            // Vector3
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(
                DataType.Table,
                typeof(Vector3),
                dynVal =>
                {
                    var table = dynVal.Table;
                    var x = (float)(double)table[1];
                    var y = (float)(double)table[2];
                    var z = (float)(double)table[3];
                    return new Vector3(x, y, z);
                }
            );

            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Vector3>(
                (script, vector) =>
                {
                    var x = DynValue.NewNumber(vector.X);
                    var y = DynValue.NewNumber(vector.Y);
                    var z = DynValue.NewNumber(vector.Z);
                    var dynVal = DynValue.NewTable(script, x, y, z);
                    return dynVal;
                }
            );

            // Vector4
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(
                DataType.Table,
                typeof(Vector4),
                dynVal =>
                {
                    var table = dynVal.Table;
                    var x = (float)(double)table[1];
                    var y = (float)(double)table[2];
                    var z = (float)(double)table[3];
                    var w = (float)(double)table[4];
                    return new Vector4(x, y, z, w);
                }
            );

            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Vector4>(
                (script, vector) =>
                {
                    var x = DynValue.NewNumber(vector.X);
                    var y = DynValue.NewNumber(vector.Y);
                    var z = DynValue.NewNumber(vector.Z);
                    var w = DynValue.NewNumber(vector.W);
                    var dynVal = DynValue.NewTable(script, x, y, z, w);
                    return dynVal;
                }
            );
        }

        /// <summary>
        ///     Attempts to update <see cref="ScriptText"/>
        /// </summary>
        private void LoadScriptText()
        {
            if (IsResource)
            {
                var buffer = GameBase.Game.Resources.Get(FilePath);
                ScriptText = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                return;
            }

            const int Retries = 10;

            // There is no way to wait for the file lock to release. Every solution found in libraries,
            // stackoverflow answers etc. are a variation on the brute force approach which is used here.
            for (var i = 0; i < Retries; i++)
                try
                {
                    using FileStream file = new(FilePath, FileMode.Open, FileAccess.Read, FileShare.None);
                    using StreamReader reader = new(file);
                    ScriptText = reader.ReadToEnd();
                    return;
                }
                catch (Exception)
                {
                    if (i == Retries - 1)
                        throw;

                    Thread.Sleep(50);
                }
        }

        /// <summary>
        ///     Handles an exception that comes from the lua interpreter.
        /// </summary>
        private void HandleLuaException(Exception e)
        {
            FailedVersion = LoadedVersion;
            Logger.Error(e, LogType.Runtime);

            var summary = e switch
            {
                DynamicExpressionException => "a dynamic expression",
                InternalErrorException => "an internal",
                ScriptRuntimeException => "a script runtime",
                SyntaxErrorException => "a syntax",
                InterpreterException => "an interpreter",
                IOException => "an IO",
                // Engine causes an IndexOutOfRangeException on stack overflows
                IndexOutOfRangeException => "a stack overflow",
                _ => "an unknown",
            };

            var message = e switch
            {
                InterpreterException { DecoratedMessage: { } decorated } => $" at {decorated.Replace("chunk_0:", "")}",
                FileNotFoundException => ". Did \"plugin.lua\" get moved?",
                IndexOutOfRangeException => ".",
                _ => $": {e.Message}",
            };

            var callStack = (e as InterpreterException)?.CallStack is { } list
                ? $"\nCall stack:\n{string.Join("\n", list.Select(x => $"{x.Name}{(x.Location is { } location ? $" at {FormatSource(location)}" : "")}"))}"
                : "";

            NotificationManager.Show(
                NotificationLevel.Error,
                $"Plugin \"{Name}\" caused {summary} error{message}{callStack}"
            );
        }

        private void Print(params DynValue[] args)
        {
            NotificationLevel? level =
                args.Length is not 0 && Enum.TryParse(args[0].CastToString(), out NotificationLevel l) ? l : null;

            NotificationManager.Show(
                level ?? NotificationLevel.Info,
                $"{Name}:\n{string.Join("\n", args.Skip(level is null ? 0 : 1).Select(x => $"{x}"))}"
            );
        }

        private static string FormatSource(SourceRef source)
        {
            StringBuilder sb = new("(");
            sb.Append(source.FromLine);

            if (source.ToLine >= 0 && source.ToLine != source.FromLine)
                sb.Append('-').Append(source.ToLine);

            sb.Append(',').Append(source.FromChar);

            if (source.ToChar >= 0 && source.ToChar != source.FromChar)
                sb.Append('-').Append(source.ToChar);

            return sb.Append(')').ToString();
        }
    }
}
