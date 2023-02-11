using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Config;
using RestSharp;
using Wobble.Logging;
using Wobble.Platform;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Quaver.Shared.Screens.Edit.Input
{
    [Serializable]
    public class EditorInputConfig
    {
        [YamlIgnore] public static string ConfigPath = ConfigManager.GameDirectory?.Value + "/editor_keys.yaml";

        public bool ReverseScrollSeekDirection { get; private set; }
        public Dictionary<KeybindActions, KeybindList> Keybinds { get; private set; }
        public Dictionary<string, KeybindList> PluginKeybinds { get; private set; }

        public EditorInputConfig()
        {
            ReverseScrollSeekDirection = true;
            Keybinds = DefaultKeybinds;
            PluginKeybinds = new Dictionary<string, KeybindList>();
        }

        public static EditorInputConfig LoadFromConfig()
        {
            var config = new EditorInputConfig();

            if (!File.Exists(ConfigPath))
            {
                Logger.Debug("No editor key config found, using default", LogType.Runtime);
                config.SaveToConfig();
                return config;
            }

            try
            {
                using (var file = File.OpenText(ConfigPath))
                    config = Deserialize(file);
                Logger.Debug("Loaded editor key config", LogType.Runtime);
                config.SaveToConfig(); // Reformat after loading
            }
            catch (YamlException e)
            {
                Logger.Error($"Could not load editor key config, using default: Failed to parse configuration in line {e.Start.Line}", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.Error($"Could not load editor key config, using default: {e.Message}", LogType.Runtime);
            }

            return config;
        }

        public KeybindList GetOrDefault(KeybindActions action) => Keybinds.GetValueOrDefault(action, new KeybindList());

        public KeybindList GetPluginKeybindOrDefault(string name) => PluginKeybinds.GetValueOrDefault(name, new KeybindList());

        public void AddKeybindToAction(KeybindActions action, Keybind keybind)
        {
            if (!Keybinds.ContainsKey(action))
                Keybinds.Add(action, new KeybindList(keybind));
            else
                Keybinds[action].Add(keybind);
        }

        public bool RemoveKeybindFromAction(KeybindActions action, Keybind keybind)
        {
            if (Keybinds.ContainsKey(action))
                return Keybinds[action].Remove(keybind);
            return false;
        }

        public void SaveToConfig()
        {
            try
            {
                File.WriteAllText(ConfigPath, Serialize());
                Logger.Debug("Saved editor key config to file", LogType.Runtime);
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

        public void FillMissingKeys(bool fillWithDefaultBinds)
        {
            var count = 0;

            foreach (var (action, defaultBind) in DefaultKeybinds)
            {
                if (!Keybinds.ContainsKey(action))
                {
                    var bind = fillWithDefaultBinds ? defaultBind : new KeybindList();
                    Keybinds.Add(action, bind);
                    count++;
                }
            }

            if (count > 0)
            {
                SaveToConfig();
                Logger.Debug($"Filled {count} missing action keybinds in key config file", LogType.Runtime);
            }
        }

        public void ResetConfigFile()
        {
            ReverseScrollSeekDirection = true;
            Keybinds = DefaultKeybinds;
            PluginKeybinds = new Dictionary<string, KeybindList>();
            SaveToConfig();
            Logger.Debug("Reset editor keybind config file", LogType.Runtime);
        }

        public Dictionary<Keybind, HashSet<KeybindActions>> ReverseDictionary()
        {
            var dict = new Dictionary<Keybind, HashSet<KeybindActions>>();

            foreach (var (action, keybinds) in Keybinds)
            {
                foreach (var keybind in keybinds.MatchingKeybinds())
                {
                    if (dict.ContainsKey(keybind))
                        dict[keybind].Add(action);
                    else
                        dict[keybind] = new HashSet<KeybindActions>() {action};
                }
            }

            return dict;
        }

        private static EditorInputConfig Deserialize(StreamReader file)
        {
            var ds = new DeserializerBuilder()
                .WithTypeConverter(new KeybindYamlTypeConverter())
                .IgnoreUnmatchedProperties()
                .Build();

            var config = ds.Deserialize<EditorInputConfig>(file);

            if (config == null)
            {
                Logger.Debug("Config file was empty, creating new default", LogType.Runtime);
                config = new EditorInputConfig();
            }

            return config;
        }

        private string Serialize()
        {
            var serializer = new SerializerBuilder()
                .WithEventEmitter(next => new KeybindListYamlFlowStyle(next))
                .WithTypeConverter(new KeybindYamlTypeConverter())
                .DisableAliases()
                .Build();

            var stringWriter = new StringWriter {NewLine = "\r\n"};
            serializer.Serialize(stringWriter, this);
            return stringWriter.ToString();
        }

        [YamlIgnore] public static Dictionary<KeybindActions, KeybindList> DefaultKeybinds = new Dictionary<KeybindActions, KeybindList>()
        {
        };
    }
}