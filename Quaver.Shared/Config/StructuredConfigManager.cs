using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Quaver.Server.Client.Helpers;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Bindables;
using Wobble.Logging;

namespace Quaver.Shared.Config;

public static class StructuredConfigManager
{
    /// <summary>
    ///     These are all values that should never ben
    /// </summary>
    private static string _gameDirectory;

    [StructuredPropertyIgnore] private static string ConfigPath => _gameDirectory + "/quaver.structcfg.json";

    public static JsonSerializer CreateSerializer() => new()
    {
        Converters = { new StringEnumConverter() },
        Formatting = Formatting.Indented,
        DefaultValueHandling = DefaultValueHandling.Include,
        NullValueHandling = NullValueHandling.Include
    };


    /// <summary>
    ///     Dictates whether or not this is the first write of the file for the current game session.
    ///     (Not saved in Config)
    /// </summary>
    [StructuredPropertyIgnore]
    private static bool FirstWrite { get; set; }

    /// <summary>
    ///     The last time we've wrote config.
    /// </summary>
    [StructuredPropertyIgnore]
    private static long LastWrite { get; set; }

    internal static Bindable<WindowStates> WindowStates { get; private set; }

    /// <summary>
    ///     Reads a Bindable<T>. Works on all types.
    /// </summary>
    /// <returns></returns>
    private static Bindable<T> BindValue<T>(string name, T defaultVal, StructuredConfigModel data) where T : class
    {
        var binded = new Bindable<T>(name, defaultVal);
        var useDefault = false;
        // Attempt to parse the value and default it if it can't.
        try
        {
            binded.Value = data.GetType().GetProperty(name)?.GetValue(data) as T;
            useDefault = binded.Value == null;
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to read {name}: {e}", LogType.Runtime);
            useDefault = true;
        }

        if (useDefault)
            binded.Value = defaultVal;

        binded.ValueChanged += AutoSaveConfiguration;
        return binded;
    }


    /// <summary>
    ///     Config Autosave functionality for Bindable<T>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="d"></param>
    private static void AutoSaveConfiguration<T>(object sender, BindableValueChangedEventArgs<T> d)
    {
        // ReSharper disable once ArrangeMethodOrOperatorBody
        CommonTaskScheduler.Add(CommonTask.WriteStructuredConfig);
    }

    /// <summary>
    ///     Takes all of the current values from the ConfigManager class and creates a file with them.
    ///     This will automatically be called whenever a configuration value is changed in the code.
    /// </summary>
    internal static async Task WriteConfigFileAsync()
    {
        var serializer = CreateSerializer();
        var configFilePath = _gameDirectory + "/quaver.structcfg.json";

        StructuredConfigModel data = new();

        // For every line we want to append "PropName = PropValue" to the string
        foreach (var prop in
                 typeof(StructuredConfigManager).GetProperties(BindingFlags.Static | BindingFlags.NonPublic))
        {
            if (prop.GetCustomAttribute<StructuredPropertyIgnoreAttribute>() != null)
                continue;

            try
            {
                var bindable = prop.GetValue(null);
                var value = prop.PropertyType.GetProperty("Value").GetValue(bindable);
                data.GetType().GetProperty(prop.Name).SetValue(data, value);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to save structured config item {prop.Name}: {e}", LogType.Runtime);
            }
        }


        var success = false;
        for (var i = 0; i < 3; i++)
        {
            try
            {
                // Create a new stream
                await using var sw = new StreamWriter(configFilePath);
                sw.AutoFlush = true;

                serializer.Serialize(sw, data);

                FirstWrite = false;
                success = true;
                break;
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to save structured config: {e}", LogType.Runtime);
            }
        }

        if (!success)
        {
            Logger.Error("Too many write attempts to the structured config file have been made.", LogType.Runtime);
        }

        LastWrite = GameBase.Game?.TimeRunning ?? -1;
    }

    public static void Initialize()
    {
        // When initializing, we manually set the directory fields rather than the props,
        // because we only want to write the config file one time at this stage.
        // Usually when a property is modified, it will automatically write the config file again,
        // so that's what we're preventing here.
        _gameDirectory = Directory.GetCurrentDirectory();
        ReadConfigFile();
        Logger.Important("Structured config file has been successfully read.", LogType.Runtime);
    }

    private static void ReadConfigFile()
    {
        var data = new StructuredConfigModel();
        // We'll want to write a quaver.cfg file if it doesn't already exist.
        // There's no need to read the config file afterwards, since we already have
        // all of the default values.
        if (File.Exists(ConfigPath))
        {
            try
            {
                using var testReader = new JsonTextReader(File.OpenText(ConfigPath));
                // Delete the config file if we catch an exception.
                var jsonTestSerializer = CreateSerializer();
                data = jsonTestSerializer.Deserialize<StructuredConfigModel>(testReader);
            }
            catch (Exception e)
            {
                Logger.Important($"Structured config file couldn't be read: {e}", LogType.Runtime);
                File.Copy(ConfigPath,
                    _gameDirectory + "/quaver.corrupted." + TimeHelper.GetUnixTimestampMilliseconds() +
                    ".structcfg.json");
                File.Delete(ConfigPath);
            }
        }
        else
        {
            File.WriteAllText(ConfigPath, "");
            Logger.Important("Creating a new structured config file...", LogType.Runtime);
        }

        data ??= new StructuredConfigModel();

        WindowStates = BindValue("WindowStates", new WindowStates(), data);
        WindowStates.TriggerChange();
    }
}