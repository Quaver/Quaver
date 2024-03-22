using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using MoonSharp.Interpreter;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Proxy;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Tween;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Scripting;

public class StoryboardScript
{
    public Script WorkingScript { get; set; }
    protected string FilePath { get; set; }
    protected bool IsResource { get; set; }
    protected string ScriptText { get; set; }
    
    protected ElementAccessShortcut Shortcut { get; }
    protected LuaStoryboardState State { get; set; }
    

    protected GameplayScreenView GameplayScreenView { get; set; }
    public StoryboardActionManager ActionManager { get; set; }
    public TweenSetters TweenSetters { get; set; }

    public StoryboardConstants StoryboardConstants { get; set; }

    public StoryboardSprites StoryboardSprites { get; set; }
    public StoryboardTextures StoryboardTextures { get; set; }
    public StoryboardNotes StoryboardNotes { get; set; }
    
    public LuaStoryboardStateMachine LuaStoryboardStateMachine { get; set; }

    public StoryboardScript(string path, GameplayScreenView screenView)
    {
        FilePath = path;

        GameplayScreenView = screenView;
        
        Shortcut = new ElementAccessShortcut(screenView);
        
        ActionManager = new StoryboardActionManager(screenView);

        TweenSetters = new TweenSetters(screenView);

        StoryboardConstants = new StoryboardConstants();

        StoryboardSprites = new StoryboardSprites(screenView);

        StoryboardTextures = new StoryboardTextures(screenView);

        StoryboardNotes = new StoryboardNotes(screenView);

        LuaStoryboardStateMachine = new LuaStoryboardStateMachine(screenView);

        UserData.RegisterAssembly(Assembly.GetCallingAssembly());
        UserData.RegisterAssembly(typeof(SliderVelocityInfo).Assembly);
        UserData.RegisterType<Easing>();
        UserData.RegisterType<TweenPayload.SetterDelegate>();
        UserData.RegisterProxyType<QuaProxy, Qua>(q => new QuaProxy(q));
        UserData.RegisterProxyType<HitObjectInfoProxy, HitObjectInfo>(hitObjectInfo =>
            new HitObjectInfoProxy(hitObjectInfo));
        UserData.RegisterProxyType<TimingPointInfoProxy, TimingPointInfo>(
            tp => new TimingPointInfoProxy(tp));
        UserData.RegisterProxyType<SpriteProxy, Sprite>(s => new SpriteProxy(s));
        UserData.RegisterProxyType<ContainerProxy, Container>(s => new ContainerProxy(s));
        UserData.RegisterProxyType<DrawableProxy, Drawable>(s => new DrawableProxy(s));
        UserData.RegisterProxyType<Texture2DProxy, Texture2D>(t => new Texture2DProxy(t));
        UserData.RegisterProxyType<GameplayHitObjectKeysProxy, GameplayHitObjectKeys>(s =>
            new GameplayHitObjectKeysProxy(s));
        UserData.RegisterProxyType<GameplayHitObjectKeysInfoProxy, GameplayHitObjectKeysInfo>(s =>
            new GameplayHitObjectKeysInfoProxy(s));


        RegisterAllVectors();
        LoadScript();
    }


    public void LoadScript()
    {
        State = new LuaStoryboardState();
        WorkingScript = new Script(CoreModules.Preset_HardSandbox);

        WorkingScript.Globals["actions"] = ActionManager;
        WorkingScript.Globals["states"] = State;
        WorkingScript.Globals["tweens"] = TweenSetters;
        WorkingScript.Globals["easing"] = typeof(Easing);
        WorkingScript.Globals["constants"] = StoryboardConstants;
        WorkingScript.Globals["map"] = GameplayScreenView.Screen.Map;
        WorkingScript.Globals["sprites"] = StoryboardSprites;
        WorkingScript.Globals["textures"] = StoryboardTextures;
        WorkingScript.Globals["notes"] = StoryboardNotes;
        WorkingScript.Globals["sm"] = LuaStoryboardStateMachine;
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

        // Vector4
        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Vector4),
            dynVal =>
            {
                var table = dynVal.Table;
                var w = (float)((double)table[1]);
                var x = (float)((double)table[2]);
                var y = (float)((double)table[3]);
                var z = (float)((double)table[4]);
                return new Vector4(w, x, y, z);
            }
        );

        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Vector4>(
            (script, vector) =>
            {
                var w = DynValue.NewNumber(vector.W);
                var x = DynValue.NewNumber(vector.X);
                var y = DynValue.NewNumber(vector.Y);
                var z = DynValue.NewNumber(vector.Z);
                var dynVal = DynValue.NewTable(script, w, x, y, z);
                return dynVal;
            }
        );
    }
}