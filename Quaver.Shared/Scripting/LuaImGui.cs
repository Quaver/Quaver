using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
using YamlDotNet.Serialization;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace Quaver.Shared.Scripting
{
    public class LuaImGui : SpriteImGui
    {
        static readonly Regex s_chunks = new(@"chunk_\d+:", RegexOptions.Compiled);

        /// <summary>
        /// </summary>
        protected Script WorkingScript { get; private set; }

        /// <summary>
        /// </summary>
        private string FilePath { get; }

        private string ConfigFilePath
        {
            get
            {
                var directory = Path.GetDirectoryName(FilePath.AsSpan());
                return Path.Join(directory, "config.yaml");
            }
        }

        // <summary>
        // Gets or sets the name of the plugin
        // </summary>
        public string Name { get; }

        /// <summary>
        /// </summary>
        private bool IsResource { get; }

        private DateTime LastException { get; set; }

        private DateTime LastWatcher { get; set; }

        private int LoadedVersion { get; set; } = -1;

        private int Version { get; set; }

        private string LastErrorMessage { get; set; }

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
            if (DateTime.Now - LastException < TimeSpan.FromSeconds(1))
                return;

            SetFrameState();

            if (Draw() is { } e)
                HandleLuaException(e);

            AfterRender();
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

            WorkingScript = new(CoreModules.Preset_SoftSandbox ^ CoreModules.Dynamic)
            {
                Globals =
                {
                    ["eval"] = Eval,
                    ["eval_expr"] = EvalExpr,
                    ["imgui"] = typeof(ImGuiWrapper),
                    ["read"] = CallbackFunction.FromDelegate(null, Read),
                    ["write"] = CallbackFunction.FromDelegate(null, Write),
                    ["print"] = CallbackFunction.FromDelegate(null, Print),
                    ["state"] = State,
                },
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
                    var text = reader.ReadToEnd();

                    // It is possible that the file was only partially written to, which would lead to a syntax error.
                    WorkingScript.LoadString(text);
                    ScriptText = text;
                    LoadedVersion++;
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
            LastException = DateTime.Now;
            var message = FormatException(e);

            if (message == LastErrorMessage)
                return;

            LastErrorMessage = message;
            Logger.Error(e, LogType.Runtime);
            NotificationManager.Show(NotificationLevel.Error, message);
        }

        /// <summary>
        ///     Intercepted print function to display a notification.
        /// </summary>
        /// <param name="args">The arguments to print.</param>
        private void Print(params DynValue[] args)
        {
            NotificationLevel? level = (args.Length is 0 ? "" : args[0]?.CastToString()?.ToUpperInvariant()) switch
            {
                "I" or "INF" or "INFO" => NotificationLevel.Info,
                "W" or "WRN" or "WARN" or "WARNING" => NotificationLevel.Warning,
                "E" or "ERR" or "ERROR" => NotificationLevel.Error,
                "S" or "SUC" or "SUCCESS" => NotificationLevel.Success,
                _ => null,
            };

            NotificationManager.Show(
                level ?? NotificationLevel.Info,
                $"{Name}:\n{string.Join("\n", args.Skip(level is null ? 0 : 1).Select(x => $"{x}"))}"
            );
        }

        /// <summary>
        ///     Invokes the user-defined <c>draw</c> function, returning an <see cref="Exception"/> if it failed.
        /// </summary>
        /// <returns>The <see cref="Exception"/> if it failed.</returns>
        Exception Draw()
        {
            try
            {
                if (WorkingScript.Globals["draw"] is Closure draw)
                    WorkingScript.Call(draw);

                LastErrorMessage = null;
                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        /// <summary>
        ///     Formats the exception to be readable in a notification.
        /// </summary>
        /// <param name="e">The exception to format.</param>
        /// <returns>The formatted exception.</returns>
        string FormatException(Exception e)
        {
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
                InterpreterException { DecoratedMessage: { } decorated } => $" at {s_chunks.Replace(decorated, "")}",
                FileNotFoundException => ". Did \"plugin.lua\" get moved?",
                IndexOutOfRangeException => ".",
                _ => $": {e.Message}",
            };

            return $"Plugin \"{Name}\" caused {summary} error{message}{CallStack(e)}";
        }

        /// <summary>
        ///     Gets the call stack.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <returns>The call stack.</returns>
        static string CallStack(Exception e) =>
            (e as InterpreterException)?.CallStack is { } list
                ? $"\nCall stack:\n{string.Join("\n", list.Select(x => $"{x.Name}{(x.Location is { } location ? $" at {FormatSource(location)}" : "")}"))}"
                : "";

        /// <summary>
        ///     Formats the <see cref="SourceRef"/> to be human-friendly.
        /// </summary>
        /// <param name="source">The <see cref="SourceRef"/> to format.</param>
        /// <returns>The formatted string.</returns>
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

        /// <summary>
        ///     Evaluates code.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>
        /// The tuple with 2 elements, representing an <c>ok</c> and <c>err</c> value.
        /// The left element is the evaluated value, or <see cref="DynValue.Nil"/> if an exception occurred.
        /// The right element is the <see cref="Exception"/>, or <see cref="DynValue.Nil"/> if it succeeded.
        /// </returns>
        private static DynValue Eval(ScriptExecutionContext context, CallbackArguments args)
        {
            try
            {
                var code = args.RawGet(0, false).String;

                // Eval engine doesn't like empty code.
                if (string.IsNullOrWhiteSpace(code))
                    return DynValue.Nil;

                var ok = context.GetScript().DoString(code);
                return DynValue.NewTuple(ok, DynValue.Nil);
            }
            catch (Exception e)
            {
                var err = DynValue.NewString(e.Message);
                return DynValue.NewTuple(DynValue.Nil, err);
            }
        }

        /// <summary>
        ///     Evaluates a code expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>
        /// The tuple with 2 elements, representing an <c>ok</c> and <c>err</c> value.
        /// The left element is the evaluated value, or <see cref="DynValue.Nil"/> if an exception occurred.
        /// The right element is the <see cref="Exception"/>, or <see cref="DynValue.Nil"/> if it succeeded.
        /// </returns>
        private static DynValue EvalExpr(ScriptExecutionContext context, CallbackArguments args)
        {
            try
            {
                var code = args.RawGet(0, false).String;

                // Eval engine doesn't like empty code.
                if (string.IsNullOrWhiteSpace(code))
                    return DynValue.Nil;

                var ok = context.GetScript().CreateDynamicExpression(code).Evaluate(context);
                return DynValue.NewTuple(ok, DynValue.Nil);
            }
            catch (Exception e)
            {
                var err = DynValue.NewString(e.Message);
                return DynValue.NewTuple(DynValue.Nil, err);
            }
        }

        private DynValue Read(ScriptExecutionContext context, CallbackArguments args)
        {
            try
            {
                using FileStream file = new(ConfigFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
                using StreamReader fileReader = new(file);
                var obj = new DeserializerBuilder().Build().Deserialize(fileReader);
                return DynValue.FromObject(context.GetScript(), obj);
            }
            catch (Exception)
            {
                return DynValue.Nil;
            }
        }

        private DynValue Write(ScriptExecutionContext _, CallbackArguments args)
        {
            try
            {
                var toSerialize = args.GetArray() is var array && array.Length is 1
                    ? ToSimpleObject(array[0])
                    : Array.ConvertAll(array, x => ToSimpleObject(x));

                var serializer = new Serializer();
                var stringWriter = new StringWriter();
                serializer.Serialize(stringWriter, toSerialize);
                var serialized = stringWriter.ToString();
                File.WriteAllText(ConfigFilePath, serialized);
                return DynValue.True;
            }
            catch (Exception)
            {
                return DynValue.False;
            }
        }

        private object ToSimpleObject(DynValue value, int depth = 10) =>
            depth <= 0
                ? value.ToString()
                : value.Type switch
                {
                    DataType.Nil or DataType.Void => null,
                    DataType.Boolean => value.Boolean,
                    DataType.Number => value.Number,
                    DataType.String => value.String,
                    DataType.Table => ToSimpleObject(value.Table, depth),
                    DataType.Tuple => Array.ConvertAll(value.Tuple, x => ToSimpleObject(x, depth - 1)),
                    DataType.UserData => value.UserData.Object,
                    DataType.ClrFunction => value.Callback.Name,
                    DataType.YieldRequest => Array.ConvertAll(value.YieldRequest.ReturnValues, x => ToSimpleObject(x, depth - 1)),
                    DataType.Function or DataType.TailCallRequest or DataType.Thread => value.ToString(),
                    _ => null,
                };

        object ToSimpleObject(Table table, int depth)
        {
            var max = 1;

            foreach (var a in table.Pairs)
                if (a.Key.Type == DataType.Number && a.Key.Number % 1 is 0 && a.Key.Number <= int.MaxValue)
                {
                    if (a.Key.Number > max)
                        max = (int)a.Key.Number;
                }
                else
                    return table.Pairs.ToDictionary(x => ToSimpleObject(x.Key, depth - 1), x => ToSimpleObject(x.Value, depth - 1));

            var array = new object[max];

            for (var i = 0; i < array.Length; i++)
                array[i] = ToSimpleObject(table.Get(i + 1), depth - 1);

            return array;
        }
    }
}
