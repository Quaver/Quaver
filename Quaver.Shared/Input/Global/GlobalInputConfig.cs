using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Quaver.Shared.Config;
using Wobble.Logging;
using Wobble.Platform;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Quaver.Shared.Input.Global
{
    [Serializable]
    public class GlobalInputConfig : IInputConfig<GlobalKeybindActions>
    {
        public static string ConfigPath => ConfigManager.GameDirectory?.Value + "/quaver-keybinds.yaml";

        private GlobalInputConfigModel _model;

        private InputActionMap<GlobalKeybindActions> _keybinds;

        public ulong Version { get; private set; }

        public GlobalInputConfig(GlobalInputConfigModel model)
        {
            _model = model;
            _keybinds = new InputActionMap<GlobalKeybindActions>(model.Keybinds);
            Version++;
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<GlobalKeybindActions, KeybindList> ReadOnlyKeybinds =>
            new ReadOnlyDictionary<GlobalKeybindActions, KeybindList>(_model.Keybinds);

        /// <inheritdoc />
        public KeybindList GetOrDefault(GlobalKeybindActions action)
        {
            return _keybinds.GetOrDefault(action);
        }

        /// <inheritdoc />
        public void AddKeybindToAction(GlobalKeybindActions action, Keybind keybind)
        {
            Version++;
            _keybinds.AddKeybindToAction(action, keybind);
        }

        /// <inheritdoc />
        public bool RemoveKeybindFromAction(GlobalKeybindActions action, Keybind keybind)
        {
            if (!_keybinds.RemoveKeybindFromAction(action, keybind))
                return false;

            Version++;
            return true;

        }

        /// <inheritdoc />
        public KeybindList? SetKeybindsForAction(GlobalKeybindActions action,
            KeybindList keybindList)
        {
            Version++;
            return _keybinds.SetKeybindsForAction(action, keybindList);
        }

        /// <inheritdoc />
        public bool TryGetActionsFor(Keybind keybind, out HashSet<GlobalKeybindActions>? set)
        {
            return _keybinds.TryGetActionsFor(keybind, out set);
        }

        private static GlobalInputConfig Default() =>
            new(new GlobalInputConfigModel(s_defaultKeybinds));

        public static GlobalInputConfig LoadFromConfig()
        {
            var config = Default();

            if (!File.Exists(ConfigPath))
            {
                Logger.Debug("No global key config found, using default", LogType.Runtime);
                config.SaveToConfig();
                return config;
            }

            try
            {
                using (var file = File.OpenText(ConfigPath))
                    config = Deserialize(file);
                Logger.Debug("Loaded global key config", LogType.Runtime);
                config.SaveToConfig(); // Reformat after loading
            }
            catch (YamlException e)
            {
                Logger.Error(
                    $"Could not load global key config, using default: {e}",
                    LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.Error($"Could not load global key config, using default: {e.Message}",
                    LogType.Runtime);
            }

            return config;
        }

        public void SaveToConfig()
        {
            try
            {
                File.WriteAllText(ConfigPath, Serialize());
                Logger.Debug("Saved global key config to file", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString(), LogType.Runtime);
            }
        }

        public void OpenConfigFile()
        {
            try
            {
                Utils.NativeUtils.OpenNatively(ConfigPath);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString(), LogType.Runtime);
            }
        }

        public int FillMissingKeys(bool fillWithDefaultBinds)
        {
            var count = 0;

            foreach (var (action, defaultBind) in s_defaultKeybinds)
            {
                var bind = fillWithDefaultBinds ? defaultBind : new KeybindList();
                if (_keybinds.SetKeybindsForActionIfNotExits(action, bind))
                    count++;
            }

            if (count > 0)
            {
                SaveToConfig();
                Logger.Debug($"Filled {count} missing action keybinds in key config file",
                    LogType.Runtime);
            }

            Version++;
            return count;
        }

        /// <inheritdoc />
        public KeybindList DefaultKeybindsFor(GlobalKeybindActions action) =>
            s_defaultKeybinds[action];

        public void ResetConfigFile()
        {
            _model.Keybinds = s_defaultKeybinds;
            _keybinds = new InputActionMap<GlobalKeybindActions>(_model.Keybinds);
            SaveToConfig();
            Version++;
            Logger.Debug("Reset global keybind config file", LogType.Runtime);
        }

        private static GlobalInputConfig Deserialize(StreamReader file)
        {
            var ds = new DeserializerBuilder()
                .WithTypeConverter(new KeybindYamlTypeConverter())
                .IgnoreUnmatchedProperties()
                .Build();

            var config = ds.Deserialize<GlobalInputConfigModel>(file);

            if (config == null)
            {
                Logger.Debug("Config file was empty, creating new default", LogType.Runtime);
                return Default();
            }

            return new(config);
        }

        private string Serialize()
        {
            var serializer = new SerializerBuilder()
                .WithEventEmitter(next => new KeybindListYamlFlowStyle(next))
                .WithTypeConverter(new KeybindYamlTypeConverter())
                .DisableAliases()
                .Build();

            var stringWriter = new StringWriter { NewLine = "\r\n" };
            serializer.Serialize(stringWriter, _model);
            return stringWriter.ToString();
        }

#pragma warning disable format // @formatter:off
        [YamlIgnore]
        private static Dictionary<GlobalKeybindActions, KeybindList> s_defaultKeybinds = new()
        { };
#pragma warning restore format // @formatter:on
    }
}