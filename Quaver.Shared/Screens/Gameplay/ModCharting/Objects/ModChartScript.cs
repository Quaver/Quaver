using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonSharp.Interpreter;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;
using Quaver.Shared.Screens.Gameplay.ModCharting.Proxy;
using Quaver.Shared.Screens.Gameplay.ModCharting.Tween;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Logging;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

public class ModChartScript
{
    public Script WorkingScript { get; set; }
    protected string FilePath { get; set; }
    protected bool IsResource { get; set; }
    protected string ScriptText { get; set; }
    
    protected ElementAccessShortcut Shortcut { get; }
    protected ModChartState State { get; set; }
    

    protected GameplayScreenView GameplayScreenView { get; set; }
    public ModChartActionManager ActionManager { get; set; }
    public TweenSetters TweenSetters { get; set; }

    public ModChartConstants ModChartConstants { get; set; }

    public ModChartStage ModChartStage { get; set; }
    public ModChartNotes ModChartNotes { get; set; }
    
    public ModChartEvents ModChartEvents { get; set; }
    
    public ModChartStateMachines ModChartStateMachines { get; set; }

    public ModChartScript(string path, GameplayScreenView screenView)
    {
        FilePath = path;

        GameplayScreenView = screenView;
        
        Shortcut = new ElementAccessShortcut(screenView);
        
        ActionManager = new ModChartActionManager(screenView);

        TweenSetters = new TweenSetters(screenView);

        ModChartConstants = new ModChartConstants();

        ModChartStage = new ModChartStage(screenView);

        ModChartNotes = new ModChartNotes(screenView);

        ModChartEvents = new ModChartEvents(screenView);

        ModChartStateMachines = new ModChartStateMachines(screenView);

        UserData.RegisterAssembly(Assembly.GetCallingAssembly());
        UserData.RegisterAssembly(typeof(SliderVelocityInfo).Assembly);
        UserData.RegisterType<Easing>();
        UserData.RegisterType<Alignment>();
        UserData.RegisterType<Judgement>();
        UserData.RegisterType<Direction>();
        UserData.RegisterType<ModChartEventType>();
        UserData.RegisterType<TweenPayload.SetterDelegate>();
        UserData.RegisterProxyType<QuaProxy, Qua>(q => new QuaProxy(q));
        UserData.RegisterProxyType<HitObjectInfoProxy, HitObjectInfo>(hitObjectInfo =>
            new HitObjectInfoProxy(hitObjectInfo));
        UserData.RegisterProxyType<TimingPointInfoProxy, TimingPointInfo>(
            tp => new TimingPointInfoProxy(tp));
        UserData.RegisterProxyType<SpriteProxy, Sprite>(s => new SpriteProxy(s));
        UserData.RegisterProxyType<AnimatableSpriteProxy, AnimatableSprite>(s => new AnimatableSpriteProxy(s));
        UserData.RegisterProxyType<SpriteTextPlusProxy, SpriteTextPlus>(t => new SpriteTextPlusProxy(t));
        UserData.RegisterProxyType<ContainerProxy, Container>(s => new ContainerProxy(s));
        UserData.RegisterProxyType<DrawableProxy, Drawable>(s => new DrawableProxy(s));
        UserData.RegisterProxyType<Texture2DProxy, Texture2D>(t => new Texture2DProxy(t));
        UserData.RegisterProxyType<GameplayHitObjectKeysProxy, GameplayHitObjectKeys>(s =>
            new GameplayHitObjectKeysProxy(s));
        UserData.RegisterProxyType<GameplayHitObjectKeysInfoProxy, GameplayHitObjectKeysInfo>(s =>
            new GameplayHitObjectKeysInfoProxy(s), friendlyName:"GameplayHitObjectKeys");


        RegisterAllVectors();
        LoadScript();
    }


    public void LoadScript()
    {
        State = new ModChartState();
        WorkingScript = new Script(CoreModules.Preset_HardSandbox);

        WorkingScript.Globals["actions"] = ActionManager;
        WorkingScript.Globals["state"] = State;
        WorkingScript.Globals["tweens"] = TweenSetters;
        WorkingScript.Globals["easing"] = typeof(Easing);
        WorkingScript.Globals["constants"] = ModChartConstants;
        WorkingScript.Globals["map"] = GameplayScreenView.Screen.Map;
        WorkingScript.Globals["stage"] = ModChartStage;
        WorkingScript.Globals["notes"] = ModChartNotes;
        WorkingScript.Globals["sm"] = ModChartStateMachines;
        WorkingScript.Globals["fonts"] = typeof(Fonts);
        WorkingScript.Globals["events"] = ModChartEvents;
        WorkingScript.Globals["alignment"] = typeof(Alignment);
        WorkingScript.Globals["direction"] = typeof(Direction);
        WorkingScript.Globals["event_type"] = typeof(ModChartEventType);
        WorkingScript.Options.DebugPrint = s => Logger.Debug(s, LogType.Runtime);

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

            // Update state at start
            Update(int.MinValue);
            WorkingScript.DoString(ScriptText, codeFriendlyName: Path.GetFileName(FilePath));
        }
        catch (ScriptRuntimeException e)
        {
            Logger.Error(e.DecoratedMessage, LogType.Runtime);
        }
        catch (SyntaxErrorException e)
        {
            Logger.Error(e.DecoratedMessage, LogType.Runtime);
        }
        catch (Exception e)
        {
            Logger.Error(e, LogType.Runtime);
        }
    }

    public void Update(int time)
    {
        State.SongTime = time;
        State.UnixTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        State.CurrentTimingPoint = GameplayScreenView.Screen.Map.GetTimingPointAt(State.SongTime);
        State.WindowSize = new Vector2(ConfigManager.WindowWidth.Value, ConfigManager.WindowHeight.Value);
        ModChartStateMachines.RootMachine.Update();
        ModChartEvents.DeferredEventQueue.Dispatch();
    }

    /// <summary>
    ///     Handles registering the Vector types for the script
    /// </summary>
    private void RegisterAllVectors()
    {
        // Vector 2
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Vector2),
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

        // Scalable Vector 2
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(ScalableVector2),
            dynVal =>
            {
                var table = dynVal.Table;
                var x = (float)(double)table[1];
                var y = (float)(double)table[2];
                var sx = (float)(double)table[3];
                var sy = (float)(double)table[4];
                return new ScalableVector2(x, y, sx, sy);
            }
        );

        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<ScalableVector2>(
            (script, vector) =>
            {
                var x = DynValue.NewNumber(vector.X.Value);
                var y = DynValue.NewNumber(vector.Y.Value);
                var sx = DynValue.NewNumber(vector.X.Scale);
                var sy = DynValue.NewNumber(vector.Y.Scale);
                var dynVal = DynValue.NewTable(script, x, y, sx, sy);
                return dynVal;
            }
        );

        // Vector3
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Vector3),
            dynVal =>
            {
                var table = dynVal.Table;
                var x = (float)((double)table[1]);
                var y = (float)((double)table[2]);
                var z = (float)((double)table[3]);
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
        
        // Color
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Color),
            dynVal =>
            {
                var table = dynVal.Table;
                var r = (byte)(double)table[1];
                var g = (byte)(double)table[2];
                var b = (byte)(double)table[3];
                var a = (byte)(double)table[4];
                return new Color(r, g, b, a);
            }
        );

        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Color>(
            (script, vector) =>
            {
                var r = DynValue.NewNumber(vector.R);
                var g = DynValue.NewNumber(vector.G);
                var b = DynValue.NewNumber(vector.B);
                var a = DynValue.NewNumber(vector.A);
                var dynVal = DynValue.NewTable(script, r, g, b, a);
                return dynVal;
            }
        );

        // Vector4
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Vector4),
            dynVal =>
            {
                var table = dynVal.Table;
                var x = (float)((double)table[1]);
                var y = (float)((double)table[2]);
                var z = (float)((double)table[3]);
                var w = (float)((double)table[4]);
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
}