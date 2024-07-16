using System;
using System.Collections.Generic;
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
using DataType = MoonSharp.Interpreter.DataType;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace Quaver.Shared.Scripting
{
    public class LuaImGui : SpriteImGui
    {
        const int Limit = 10;

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
        /// Determines whether the first draw call has been made.
        /// </summary>
        private bool IsFirstDrawCall { get; set; }

        /// <summary>
        /// Determines whether the plugin is a resource.
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
            UserData.RegisterType<IntPtr>();

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
            UserData.RegisterType<ImGuiStylePtr>();
            UserData.RegisterType<ImGuiStoragePtr>();
            UserData.RegisterType<ImGuiIOPtr>();
            UserData.RegisterType<ImGuiPayloadPtr>();
            UserData.RegisterType<ImDrawDataPtr>();
            UserData.RegisterType<ImFontPtr>();
            UserData.RegisterType<ImGuiIOPtr>();
            UserData.RegisterType<ImGuiComboFlags>();
            UserData.RegisterType<ImGuiFocusedFlags>();
            UserData.RegisterType<ImGuiHoveredFlags>();

            // Vectors, since imgui.CreateVectorN exists.
            UserData.RegisterType<Vector2>();
            UserData.RegisterType<Vector3>();
            UserData.RegisterType<Vector4>();

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

            if (IsFirstDrawCall)
            {
                IsFirstDrawCall = false;

                if (CallUserDefinedFunction("awake") is { } awakeException)
                    HandleLuaException(awakeException);
            }

            if (CallUserDefinedFunction("draw") is { } drawException)
                HandleLuaException(drawException);

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
        ///     Handles registering the Vector types for the script
        /// </summary>
        private static void RegisterAllVectors()
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(
                DataType.Table,
                typeof(Vector2),
                x => LuaVectorWrapper.TryCoerceTo<Vector2>(x) ?? throw UnableToCoerce(x)
            );

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(
                DataType.Table,
                typeof(Vector3),
                x => LuaVectorWrapper.TryCoerceTo<Vector3>(x) ?? throw UnableToCoerce(x)
            );

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(
                DataType.Table,
                typeof(Vector4),
                x => LuaVectorWrapper.TryCoerceTo<Vector4>(x) ?? throw UnableToCoerce(x)
            );
        }

        /// <summary>Indicates a </summary>
        /// <param name="dynVal"></param>
        /// <returns></returns>
        private static Exception UnableToCoerce(DynValue dynVal) =>
            new ArgumentException($"Value cannot be converted to a vector type: {Display(dynVal)}");

        /// <summary>
        ///     Gets the call stack.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <returns>The call stack.</returns>
        private static string CallStack(Exception e) =>
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

        /// <summary>
        ///     Creates the string representation of the dynamic value.
        /// </summary>
        /// <param name="value">The value to create the string for.</param>
        /// <returns>The string representation of the parameter <paramref name="value"/>.</returns>
        private static string Display(DynValue value) =>
            ToSimpleObject(value) switch
            {
                Dictionary<object, object> x => $"{{ {string.Join(", ", x.Count > Limit ? x.Take(Limit).Select(x => $"{x.Key} = {x.Value}").Append("…") : x.Select(x => $"{x.Key} = {x.Value}"))} }}",
                object[] x => $"[{string.Join(", ", x.Length > Limit ? x.Take(Limit).Append("…") : x)}]",
                _ => value.ToPrintString(),
            };

        /// <summary>
        ///     Converts the <see cref="DynValue"/> to a primitive, string, array, or dictionary.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="depth">The depth of the recursion.</param>
        /// <returns>The converted value.</returns>
        private static object ToSimpleObject(DynValue value, int depth = Limit) =>
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
                    DataType.UserData => value.UserData.Object switch
                    {
                        Vector2 v => new[] { v.X, v.Y },
                        Vector3 v => new[] { v.X, v.Y, v.Z },
                        Vector4 v => new[] { v.X, v.Y, v.Z, v.W },
                        BookmarkInfo i => new { i.Note, i.StartTime },
                        CustomAudioSampleInfo i => new { i.Path, i.UnaffectedByRate },
                        EditorLayerInfo i => new { i.ColorRgb, i.Hidden, i.Name },
                        HitObjectInfo i => new { i.EditorLayer, i.EndTime, i.HitSound, i.Lane, i.StartTime },
                        KeySoundInfo i => new { i.Sample, i.Volume },
                        SliderVelocityInfo i => new { i.Multiplier, i.StartTime },
                        SoundEffectInfo i => new { i.Sample, i.StartTime, i.Volume },
                        TimingPointInfo i => new { i.Bpm, i.Hidden, i.StartTime, i.Signature },
                        var o => o,
                    },
                    DataType.ClrFunction => value.Callback.Name,
                    DataType.YieldRequest => Array.ConvertAll(value.YieldRequest.ReturnValues, x => ToSimpleObject(x, depth - 1)),
                    DataType.Function or DataType.TailCallRequest or DataType.Thread => value.ToString(),
                    _ => null,
                };

        /// <summary>
        ///     Converts the <see cref="Table"/> to a primitive, string, array, or dictionary.
        /// </summary>
        /// <param name="table">The table to convert.</param>
        /// <param name="depth">The depth of the recursion.</param>
        /// <returns>The converted object.</returns>
        private static object ToSimpleObject(Table table, int depth)
        {
            var max = 1;

            foreach (var a in table.Pairs)
                if (a.Key.Type == DataType.Number && a.Key.Number % 1 is 0 && a.Key.Number <= int.MaxValue)
                {
                    if (a.Key.Number > max)
                        max = (int)a.Key.Number;
                }
                else
                    return table.Pairs.ToDictionary(
                        x => ToSimpleObject(x.Key, depth - 1),
                        x => ToSimpleObject(x.Value, depth - 1)
                    );

            var array = new object[max];

            for (var i = 0; i < array.Length; i++)
                array[i] = ToSimpleObject(table.Get(i + 1), depth - 1);

            return array;
        }

        /// <summary>
        ///     Reads the config file.
        /// </summary>
        /// <param name="context">The script execution context.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The deserialized object.</returns>
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

        /// <summary>
        ///     Writes the config file.
        /// </summary>
        /// <param name="_">The script execution context. This parameter is unused.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The value indicating whether the write was successful.</returns>
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
                    ["print"] = CallbackFunction.FromDelegate(null, Print),
                    ["read"] = CallbackFunction.FromDelegate(null, Read),
                    ["state"] = State,
                    ["vector"] = typeof(LuaVectorWrapper),
                    ["vector2"] = typeof(Vector2),
                    ["vector3"] = typeof(Vector3),
                    ["vector4"] = typeof(Vector4),
                    ["write"] = CallbackFunction.FromDelegate(null, Write),
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

            IsFirstDrawCall = true;
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
            NotificationLevel? level = args.FirstOrDefault()?.CastToString()?.ToUpperInvariant() switch
            {
                "I" or "I!" or "INF" or "INF!" or "INFO" or "INFO!" => NotificationLevel.Info,
                "W" or "W!" or "WRN" or "WRN!" or "WARN" or "WARN!" or "WARNING" or "WARNING!" => NotificationLevel.Warning,
                "E" or "E!" or "ERR" or "ERR!" or "ERROR" or "ERROR!" => NotificationLevel.Error,
                "S" or "S!" or "YAY" or "YAY!" or "SUCCESS" or "SUCCESS!" => NotificationLevel.Success,
                _ => null,
            };

            NotificationManager.Show(
                level ?? NotificationLevel.Info,
                $"{(level is null || args[0].CastToString().LastOrDefault() is not '!' ? $"{Name}: " : "")}" +
                $"{string.Join("\n", args.Skip(level is null ? 0 : 1).Select(Display))}"
            );
        }

        /// <summary>
        ///     Invokes the user-defined function, returning an <see cref="Exception"/> if it failed.
        /// </summary>
        /// <returns>The <see cref="Exception"/> if it failed.</returns>
        private Exception CallUserDefinedFunction(string functionName)
        {
            try
            {
                if (WorkingScript.Globals[functionName] is Closure draw)
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
        private string FormatException(Exception e)
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
    }
}
