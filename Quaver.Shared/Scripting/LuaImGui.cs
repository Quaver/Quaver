using System;
using System.Collections;
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
using MoonSharp.Interpreter.CoreLib;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Plugins;
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
        public const int RecursionLimit = 10;

        private const int IterationLimit = 10;

        private static readonly IUserDataDescriptor s_imgui = UserData.RegisterType(typeof(ImGui));

        private static readonly IUserDataDescriptor s_imguiRedirects = UserData.RegisterType(typeof(ImGuiRedirect));

        private static readonly StringComparer s_comparer = StringComparer.OrdinalIgnoreCase;

        private static readonly Dictionary<string, DynValue> s_methods = MethodNamesOf(typeof(ImGui))
           .Distinct()
           .ToDictionary(x => x, s_imgui.GetWrappedFunctionThatPacksReturnedVectors, s_comparer);

        private static readonly Dictionary<string, DynValue> s_redirectMethods =
            MethodNamesOf(typeof(ImGuiRedirect))
               .Distinct()
               .ToDictionary(x => x, s_imguiRedirects.GetWrappedFunctionThatPacksReturnedVectors, s_comparer);

        private static readonly Dictionary<Type, DynValue> s_enums = new()
        {
            [typeof(ImGui)] = DynValue.NewTable(new Table(null) { MetaTable = new(null) { ["__index"] = Index } })
               .AsReadOnly(),
            [typeof(ImGuiCol)] = DefineEnum(
                ("TabActive", ImGuiCol.TabSelected),
                ("TabUnfocused", ImGuiCol.TabDimmed),
                ("TabUnfocusedActive", ImGuiCol.TabDimmedSelected)
            ),
            [typeof(ImGuiColorEditFlags)] = DefineEnum(
                ("_DataTypeMask", ImGuiColorEditFlags.DataTypeMask),
                ("_DisplayMask", ImGuiColorEditFlags.DisplayMask),
                ("_InputMask", ImGuiColorEditFlags.InputMask),
                ("_OptionsDefault", ImGuiColorEditFlags.DefaultOptions),
                ("_PickerMask", ImGuiColorEditFlags.PickerMask)
            ),
            [typeof(ImGuiDragDropFlags)] =
                DefineEnum(("SourceAutoExpirePayload", ImGuiDragDropFlags.PayloadAutoExpire)),
            [typeof(ImGuiInputTextFlags)] = DefineEnum(
                ("AlwaysInsertMode", ImGuiInputTextFlags.AlwaysOverwrite),
                ("Multiline", ImGuiInputTextFlags.None)
            ),
            [typeof(ImGuiTreeNodeFlags)] = DefineEnum(("AllowItemOverlap", ImGuiTreeNodeFlags.AllowOverlap)),
            [typeof(ImGuiSelectableFlags)] = DefineEnum(("DontClosePopups", ImGuiSelectableFlags.NoAutoClosePopups)),
        };

        private static readonly Regex s_capitals = new(@"\p{Lu}", RegexOptions.Compiled);

        private static readonly Regex s_chunks = new(@"chunk_\d+:", RegexOptions.Compiled);

        private static readonly Type[] s_types = typeof(ImGui).Assembly.GetTypes();

        private static Action<IEditorAction, HistoryType, bool> s_events;

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
        public bool IsFirstDrawCall { get; set; }

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

        private List<Action<IEditorAction, HistoryType, bool>> Events { get; } = new();

        /// <summary>
        /// </summary>
        public LuaPluginState State { get; private set; }

        static LuaImGui()
        {
            static void AddMethodAliases(object anonymous)
            {
                foreach (var property in anonymous.GetType().GetProperties())
                    s_methods[property.Name] = s_methods[$"{property.GetValue(anonymous)}"];
            }

            static void RegisterWithConversion<T>()
                where T : struct, IFormattable
            {
                UserData.RegisterType<T>();

                Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(
                    DataType.Table,
                    typeof(T),
                    x => LuaVectorWrapper.TryCoerceTo<T>(x) ?? throw UnableToCoerce(x)
                );
            }

            AddMethodAliases(
                new
                {
                    BeginChildFrame = nameof(ImGui.BeginChild),
                    CaptureKeyboardFromApp = nameof(ImGui.SetNextFrameWantCaptureKeyboard),
                    CaptureMouseFromApp = nameof(ImGui.SetNextFrameWantCaptureMouse),
                    EndChildFrame = nameof(ImGui.EndChild),
                    ListBoxHeader = nameof(ImGui.BeginListBox),
                    ListBoxFooter = nameof(ImGui.EndListBox),
                    PopButtonRepeat = nameof(ImGui.PopItemFlag),
                    PopTabStop = nameof(ImGui.PopItemFlag),
                    PopAllowKeyboardFocus = nameof(ImGui.PopItemFlag),
                    GetWindowContentRegionMin = nameof(ImGui.GetCursorPos),
                    SetNextTreeNodeOpen = nameof(ImGui.SetNextItemOpen),
                }
            );

            foreach (var type in s_types.Append(typeof(HistoryType)).Append(typeof(Keys)))
            {
                UserData.RegisterType(type);

                // The reason we are instantiating generics instead of making `DefineEnum` take `Type` is to eliminate
                // the runtime checks that would be necessary every single time the enum is used. This speeds
                // up functions like __pairs and __ipairs by not having to use Enum.Parse(Type) and similar.
                if (RegisterIfEnum(type) && !s_enums.ContainsKey(type))
                    s_enums[type] = DefineEnum(type);
            }

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(
                DataType.Boolean,
                typeof(ImGuiChildFlags),
                x => x.Boolean ? ImGuiChildFlags.Border : ImGuiChildFlags.None
            );

            UserData.RegisterAssembly(typeof(SliderVelocityInfo).Assembly);
            UserData.RegisterAssembly(Assembly.GetCallingAssembly());
            RegisterWithConversion<Vector2>();
            RegisterWithConversion<Vector3>();
            RegisterWithConversion<Vector4>();
            UserData.RegisterType<nuint>();
            UserData.RegisterType<nint>();
        }

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
            LazyLoadScript();

            if (IsResource)
                return;

            Watcher = new(Path.GetDirectoryName(filePath) ?? "") { Filter = Path.GetFileName(filePath) };
            Watcher.Changed += OnFileChanged;
            Watcher.Created += OnFileChanged;
            Watcher.Renamed += OnFileChanged;
            Watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        ///     Invokes all listeners for editor changes.
        /// </summary>
        /// <param name="change">The editor change.</param>
        /// <param name="kind">Whether the change is a new edit, undo, or redo.</param>
        /// <param name="fromLua">Whether the action was generated from the plugin.</param>
        public static void Inform(IEditorAction change, HistoryType kind, bool fromLua) =>
            s_events?.Invoke(change, kind, fromLua);

        /// <summary>
        ///     Creates the string representation of the dynamic value.
        /// </summary>
        /// <param name="value">The value to create the string for.</param>
        /// <returns>The string representation of the parameter <paramref name="value"/>.</returns>
        public static string Display(DynValue value) => Display(ToSimpleObject(value)) ?? value.ToPrintString();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            ClearEvents();
            Watcher?.Dispose();
            base.Destroy();
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
            const int MaxStack = 1024;

            static string Lower(Match x)
            {
                var length = x.ValueSpan.Length;
                var span = length <= MaxStack ? stackalloc char[length] : GC.AllocateUninitializedArray<char>(length);
                _ = x.ValueSpan.ToLowerInvariant(span);
                return span.ToString();
            }

            static string UnderscoreLower(Match x)
            {
                var length = x.ValueSpan.Length + 1;
                var span = length <= MaxStack ? stackalloc char[length] : GC.AllocateUninitializedArray<char>(length);
                span[0] = '_';
                _ = x.ValueSpan.ToLowerInvariant(span[1..]);
                return span.ToString();
            }

            if (!IsFirstDrawCall)
                return;

            foreach (var type in s_types)
                if (s_enums.TryGetValue(type, out var value) &&
                    type.Name.Replace(s_capitals, Lower, 2).Replace(s_capitals, UnderscoreLower) is var key)
                    WorkingScript.Globals[key] = value;
        }

        /// <summary>
        ///     After the plugin has been rendered, this will be called.
        ///     Should be used to pop any styles
        /// </summary>
        public virtual void AfterRender() { }

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

                if (CallUserDefinedFunction("Awake", "awake") is { } awakeException)
                    HandleLuaException(awakeException);
            }

            if (CallUserDefinedFunction("Draw", "draw") is { } drawException)
                HandleLuaException(drawException);

            AfterRender();
        }

        /// <summary>
        ///     Registers the enum, and returning whether it succeeded.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <returns>Whether the registration succeeded.</returns>
        protected static bool RegisterIfEnum(Type type)
        {
            if (!type.IsEnum)
                return false;

            UserData.RegisterType(type);

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(
                DataType.String,
                type,
                x => Enum.Parse(type, x.String)
            );

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(
                DataType.Number,
                type,
                x => Enum.ToObject(type, (int)(x.CastToNumber() ?? throw UnableToCoerce(x)))
            );

            return true;
        }

        /// <summary>
        ///     Creates the enum <see cref="Table"/> with the specified <paramref name="additions"/>.
        /// </summary>
        /// <remarks><para>
        ///     Used to polyfill the missing declarations within enums for backwards compatibility.
        /// </para></remarks>
        /// <typeparam name="T">The type of enum.</typeparam>
        /// <param name="additions">The aliases.</param>
        /// <returns>The table representing the enum.</returns>
        protected static DynValue DefineEnum<T>(params (string Key, T Value)[] additions)
            where T : struct, Enum
        {
            var values = Enum.GetValues<T>();

            DynValue Inext(ScriptExecutionContext context, CallbackArguments args) =>
                (int)args[1].Number + 1 is var x && x < values.Length
                    ? DynValue.NewTuple(DynValue.NewNumber(x), DynValue.FromObject(context.OwnerScript, values[x]))
                    : DynValue.Nil;

            DynValue Next(ScriptExecutionContext context, CallbackArguments args)
            {
                var str = args[1].String;

                if (str is not null)
                {
                    var i = Array.IndexOf(values, Enum.Parse<T>(str)) + 1;

                    if (i >= values.Length)
                        return DynValue.Nil;

                    return DynValue.NewTuple(
                        DynValue.NewString(values[i].ToString()),
                        DynValue.FromObject(context.OwnerScript, values[i])
                    );
                }

                if (values.Length is 0)
                    return DynValue.Nil;

                return DynValue.NewTuple(
                    DynValue.NewString(values[0].ToString()),
                    DynValue.FromObject(context.OwnerScript, values[0])
                );
            }

            var inext = DynValue.NewCallback(Inext);
            var next = DynValue.NewCallback(Next);

            DynValue Iterator(ScriptExecutionContext context, CallbackArguments args) =>
                // ReSharper disable once GenericEnumeratorNotDisposed
                DynValue.FromObject(null, values.AsEnumerable().GetEnumerator());

            DynValue Ipairs(ScriptExecutionContext context, CallbackArguments args) => inext;

            DynValue Pairs(ScriptExecutionContext context, CallbackArguments args) => next;

            Table table = new(null)
            {
                MetaTable = new(null) { ["__ipairs"] = Ipairs, ["__iterator"] = Iterator, ["__pairs"] = Pairs },
            };

            foreach (var value in values)
                table[Number(value)] = table[$"{value}"] = value;

            // ReSharper disable once InvertIf
            if (additions is not null)
                foreach (var (key, value) in additions)
                    table[key] = value;

            return DynValue.NewTable(table).AsReadOnly();
        }

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
        ///     Creates the string representation of the <see cref="object"/>.
        /// </summary>
        /// <param name="value">The value to create the string for.</param>
        /// <returns>The string representation of the parameter <paramref name="value"/>.</returns>
        private static string Display(object value) =>
            value switch
            {
                bool x => x ? "true" : "false",
                IDictionary x => Display(x),
                ICollection x => Display(x),
                _ => null,
            };

        /// <summary>
        ///     Creates the string representation of the array.
        /// </summary>
        /// <param name="value">The value to create the string for.</param>
        /// <returns>The string representation of the parameter <paramref name="value"/>.</returns>
        private static string Display(ICollection value)
        {
            var elements = value.Cast<object>().Select(x => Display(x) ?? x);
            var truncated = value.Count > IterationLimit ? elements.Take(IterationLimit).Append("…") : elements;
            return $"[{string.Join(", ", truncated)}]";
        }

        /// <summary>
        ///     Creates the string representation of the dictionary.
        /// </summary>
        /// <param name="value">The value to create the string for.</param>
        /// <returns>The string representation of the parameter <paramref name="value"/>.</returns>
        private static string Display(IDictionary value)
        {
            var elements = Iterate(value).Select(x => $"{ToSimpleDisplay(x.Key)} = {ToSimpleDisplay(x.Value)}");
            var truncated = value.Count > IterationLimit ? elements.Take(IterationLimit).Append("…") : elements;
            return $"{{ {string.Join(", ", truncated)} }}";
        }

        /// <summary>
        ///     Creates the string representation of the <see cref="object"/>.
        /// </summary>
        /// <param name="value">The value to create the string for.</param>
        /// <returns>The string representation of the parameter <paramref name="value"/>.</returns>
        private static object ToSimpleDisplay(object value)
        {
            var v = UserDataToSimpleObject(value, RecursionLimit);
            return Display(v) ?? v;
        }

        /// <summary>
        ///     Converts the <see cref="DynValue"/> to a primitive, string, array, or dictionary.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="depth">The depth of the recursion.</param>
        /// <returns>The converted value.</returns>
        private static object ToSimpleObject(DynValue value, int depth = RecursionLimit) =>
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
                    DataType.UserData => UserDataToSimpleObject(value.UserData.Object, depth),
                    DataType.ClrFunction => value.Callback.Name,
                    DataType.YieldRequest => Array.ConvertAll(
                        value.YieldRequest.ReturnValues,
                        x => ToSimpleObject(x, depth - 1)
                    ),
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
        ///     Converts the <see cref="UserData"/> <see cref="object"/> to a primitive, string, array, or dictionary.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="depth">The depth of the recursion.</param>
        /// <returns>The converted value.</returns>
        private static object UserDataToSimpleObject(object value, int depth) =>
            depth <= 0
                ? value.ToString()
                : value switch
                {
                    Vector2 v => new object[] { v.X, v.Y },
                    Vector3 v => new object[] { v.X, v.Y, v.Z },
                    Vector4 v => new object[] { v.X, v.Y, v.Z, v.W },
                    IEnumerable i and not IDictionary and not string => i
                       .Cast<object>()
                       .Select(x => UserDataToSimpleObject(x, depth - 1))
                       .ToArray(),
                    BookmarkInfo or
                        CustomAudioSampleInfo or
                        EditorLayerInfo or
                        HitObjectInfo or
                        IEditorAction or
                        KeySoundInfo or
                        SliderVelocityInfo or
                        SoundEffectInfo or
                        TimingPointInfo => value
                           .GetType()
                           .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                           .ToDictionary(
                                object (x) => x.Name,
                                x => UserDataToSimpleObject(x.GetMethod?.Invoke(value, null), depth - 1)
                            ),
                    _ => value,
                };

        /// <summary>
        ///     Creates the enum <see cref="Table"/>.
        /// </summary>
        /// <param name="type">The type of enum.</param>
        /// <returns>The table representing the enum.</returns>
        private static DynValue DefineEnum(Type type) =>
            type.IsEnum
                ? (DynValue)((Converter<(string, BindingFlags)[], DynValue>)DefineEnum)
               .Method
               .GetGenericMethodDefinition()
               .MakeGenericMethod(type)
               .Invoke(null, new object[] { null })
                : DynValue.Nil;

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
        private static DynValue Expr(ScriptExecutionContext context, CallbackArguments args)
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
        ///     Performs the index operation for the <c>imgui</c> global table.
        /// </summary>
        /// <param name="context">The script execution context.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The function for the index operation.</returns>
        private static DynValue Index(ScriptExecutionContext context, CallbackArguments args) =>
            args.RawGet(1, false) is not { String: var str } ? DynValue.Nil :
            s_redirectMethods.TryGetValue(str, out var prioritizedRet) ? prioritizedRet :
            s_methods.TryGetValue(str, out var ret) ? ret : throw new FormatException($"Invalid method: {str}");

        /// <summary>
        ///     Converts the userdata to the <see cref="DynValue"/> containing the number.
        /// </summary>
        /// <param name="i">The instance to convert.</param>
        /// <returns>The boxed number.</returns>
        private static DynValue Number(IConvertible i) =>
            DynValue.NewNumber(Convert.ToDouble(Convert.ChangeType(i, i.GetTypeCode())));

        /// <summary>
        ///     Overrides lua'a <c>tonumber</c> to allow the underlying number of an enum to be extracted.
        /// </summary>
        /// <param name="context">The script execution context.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The deserialized object.</returns>
        private static DynValue ToNumber(ScriptExecutionContext context, CallbackArguments args) =>
            args.RawGet(0, false)?.UserData?.Object is Enum e ? Number(e) : BasicModule.tonumber(context, args);

        /// <summary>
        ///     Indicates a failure to coerce a value to a vector.
        /// </summary>
        /// <param name="dynVal">The value that failed to coerce.</param>
        /// <returns>The exception to throw.</returns>
        private static Exception UnableToCoerce(DynValue dynVal) =>
            new ArgumentException($"Value cannot be converted to the destination type: {Display(dynVal)}");

        /// <summary>
        ///     Gets the enumeration of method names from the <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The type to get the method names from.</param>
        /// <returns>The method names in the given <see cref="Type"/> object.</returns>
        private static IEnumerable<string> MethodNamesOf(Type type) =>
            type.GetMethods().Where(x => !x.IsConstructor).Select(x => x.Name);

        /// <summary>
        ///     Gets the enumeration of <see cref="DictionaryEntry"/> from the <see cref="IDictionary"/> instance.
        /// </summary>
        /// <param name="value">The value to enumerate.</param>
        /// <returns>The enumeration of the parameter <paramref name="value"/>.</returns>
        private static IEnumerable<DictionaryEntry> Iterate(IDictionary value)
        {
            var e = value.GetEnumerator();

            try
            {
                while (e.MoveNext())
                    yield return e.Entry;
            }
            finally
            {
                (e as IDisposable)?.Dispose();
            }
        }

        /// <summary>
        ///     Clears all events.
        /// </summary>
        private void ClearEvents()
        {
            foreach (var e in Events)
                s_events -= e;

            Events.Clear(); // Lets the GC clean up delegates that won't be invoked again in case this stays in memory.
        }

        /// <summary>
        ///     Loads the text from the script
        /// </summary>
        private void LazyLoadScript()
        {
            static DynValue Warn(DynValue input)
            {
                var reason = input.Type is DataType.Table or DataType.Tuple
                    ? "being too deeply nested"
                    : "relying on the previous loaded script";

                var message =
                    $"The \"state\" global contains an invalid value that will be coerced to nil due to it {reason}. Please refactor the code such that the following object is not within the global: {Display(input)}";

                NotificationManager.Show(NotificationLevel.Warning, message);
                Logger.Warning(message, LogType.Runtime);
                return DynValue.Nil;
            }

            if (LoadedVersion == Version)
                return;

            WorkingScript = new(CoreModules.Preset_SoftSandbox ^ CoreModules.Dynamic)
            {
                Globals =
                {
                    [nameof(DynamicModule.eval)] = Eval,
                    ["expr"] = Expr,
                    ["history_type"] = s_enums[typeof(HistoryType)],
                    ["keys"] = s_enums[typeof(Keys)],
                    ["listen"] = Listen,
                    [nameof(BasicModule.print)] = Print,
                    ["read"] = Read,
                    ["state"] = State = State.Clone(Warn),
                    [nameof(BasicModule.tonumber)] = ToNumber,
                    ["vector"] = typeof(LuaVectorWrapper),
                    ["vector2"] = typeof(Vector2),
                    ["vector3"] = typeof(Vector3),
                    ["vector4"] = typeof(Vector4),
                    ["write"] = Write,
                },
            };

            ClearEvents();

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
            LastErrorMessage = null;
            LastException = default;
        }

        /// <summary>
        ///     Hot-reloads the script.
        /// </summary>
        /// <param name="sender">The sender used to lock updates.</param>
        /// <param name="_">The <see cref="EventArgs"/> instance. This parameter is unused.</param>
        private void OnFileChanged(object sender, FileSystemEventArgs _)
        {
            lock (sender)
            {
                if (DateTime.Now - LastWatcher < TimeSpan.FromMilliseconds(200))
                    return;

                LastWatcher = DateTime.Now;
            }

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

            const int Retries = 20;

            // There is no way to wait for the file lock to release. Every solution found in libraries,
            // stackoverflow answers, etc. are a variation on the brute force approach which is used here.
            for (var i = 0; i < Retries; i++)
                try
                {
                    Thread.Sleep(50);
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
                }
        }

        /// <summary>
        ///     Handles an exception that comes from the lua interpreter.
        /// </summary>
        /// <param name="e">The exception.</param>
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
                IndexOutOfRangeException => "a stack overflow", // Thrown by `Engine` in the event of a stack overflow.
                NullReferenceException => "an internal",
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
        ///     Hooks all <see cref="Closure"/> instances.
        /// </summary>
        /// <param name="_">The script execution context. This parameter is unused.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The value <see cref="DynValue.Nil"/>.</returns>
        private DynValue Listen(ScriptExecutionContext _, CallbackArguments args)
        {
            var count = args.Count;

            for (var i = 0; i < count; i++)
                if (args.RawGet(i, false) is { Type: DataType.Function, Function: var function })
                {
                    void EditorAction(IEditorAction change, HistoryType kind, bool fromLua)
                    {
                        if (this is IEditorPlugin { IsActive: false })
                            return;

                        try
                        {
                            function.Call(change, kind, fromLua);
                        }
                        catch (Exception e)
                        {
                            HandleLuaException(e);
                        }
                    }

                    var action = EditorAction;
                    Events.Add(action);
                    s_events += action;
                }

            return DynValue.Nil;
        }

        /// <summary>
        ///     Intercepted print function to display a notification.
        /// </summary>
        /// <param name="_">The script execution context. This parameter is unused.</param>
        /// <param name="args">The arguments to print.</param>
        /// <returns>The value <see cref="DynValue.Nil"/>.</returns>
        private DynValue Print(ScriptExecutionContext _, CallbackArguments args)
        {
            var a = args.GetArray();

            NotificationLevel? level = a.FirstOrDefault()?.CastToString()?.ToUpperInvariant() switch
            {
                "I" or "I!" or "INF" or "INF!" or "INFO" or "INFO!" => NotificationLevel.Info,
                "W" or "W!" or "WRN" or "WRN!" or "WARN" or "WARN!" or "WARNING" or "WARNING!" => NotificationLevel.Warning,
                "E" or "E!" or "ERR" or "ERR!" or "ERROR" or "ERROR!" => NotificationLevel.Error,
                "S" or "S!" or "YAY" or "YAY!" or "SUCCESS" or "SUCCESS!" => NotificationLevel.Success,
                _ => null,
            };

            NotificationManager.Show(
                level ?? NotificationLevel.Info,
                $"{(level is null || a[0].CastToString().LastOrDefault() is not '!' ? $"{Name}: " : "")}" +
                $"{string.Join("\n", a.Skip(level is null ? 0 : 1).Select(Display))}"
            );

            return DynValue.Nil;
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
        /// <returns>The value indicating whether the writing was successful.</returns>
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
        ///     Invokes the user-defined function, returning an <see cref="Exception"/> if it failed.
        /// </summary>
        /// <param name="closureName">
        /// The name of the global that contains the <see cref="Closure"/> instance to invoke.
        /// </param>
        /// <param name="otherClosureName">
        /// The name of the other global that contains the <see cref="Closure"/> instance to invoke.
        /// </param>
        /// <returns>The <see cref="Exception"/> if it failed.</returns>
        private Exception CallUserDefinedFunction(string closureName, string otherClosureName)
        {
            try
            {
                if (WorkingScript.Globals[closureName] is Closure closure)
                    WorkingScript.Call(closure);

                if (WorkingScript.Globals[otherClosureName] is Closure otherClosure)
                    WorkingScript.Call(otherClosure);

                LastErrorMessage = null;
                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }
    }
}
