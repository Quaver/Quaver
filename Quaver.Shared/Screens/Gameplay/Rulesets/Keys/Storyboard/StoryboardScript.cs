using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text;
using MoonSharp.Interpreter;
using Quaver.API.Maps.Structures;
using Wobble;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard;

public class StoryboardScript
{
    protected Script WorkingScript { get; set; }
    protected string FilePath { get; set; }
    protected bool IsResource { get; set; }
    protected string ScriptText { get; set; }
    protected LuaStoryboardState State { get; set; }

    public StoryboardScript(string path)
    {
        FilePath = path;
        
        

        UserData.RegisterAssembly(Assembly.GetCallingAssembly());
        UserData.RegisterAssembly(typeof(SliderVelocityInfo).Assembly);
        
        
        RegisterAllVectors();
        LoadScript();
    }

    public void LoadScript()
    {
        
        State = new LuaStoryboardState();
        WorkingScript = new Script(CoreModules.Preset_HardSandbox);
        
        WorkingScript.Globals["actions"] = new StoryboardActionManager();
        WorkingScript.Globals["state"] = State;
        
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

            // Vector4
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Vector4),
                dynVal => {
                    var table = dynVal.Table;
                    var w = (float)((double)table[1]);
                    var x = (float)((double)table[2]);
                    var y = (float)((double)table[3]);
                    var z = (float)((double)table[4]);
                    return new Vector4(w, x, y, z);
                }
            );

            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Vector4>(
                (script, vector) => {
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