using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Config;
using Wobble.Logging;
using Wobble.Platform;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using static Quaver.Shared.Screens.Edit.Input.KeybindActions;

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
                config.FillMissingKeys(true);
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
            {ExitEditor, new KeybindList(Keys.Escape)},
            {PlayPause, new KeybindList(new[] {new Keybind(KeyModifiers.Free, Keys.Space), new Keybind(KeyModifiers.Free, Keys.Enter)})},
            {ZoomIn, new KeybindList(Keys.PageUp)},
            {ZoomInLarge, new KeybindList(KeyModifiers.Ctrl, Keys.PageUp)},
            {ZoomOut, new KeybindList(Keys.PageDown)},
            {ZoomOutLarge, new KeybindList(KeyModifiers.Ctrl, Keys.PageDown)},
            {SeekForwards, new KeybindList(new[] {new Keybind(KeyModifiers.Free, Keys.Up), new Keybind(KeyModifiers.Free, Keys.I)})},
            {SeekBackwards, new KeybindList(new[] {new Keybind(KeyModifiers.Free, Keys.Down), new Keybind(KeyModifiers.Free, Keys.K)})},
            {SeekForwards1ms, new KeybindList(KeyModifiers.Free, Keys.OemPeriod)},
            {SeekBackwards1ms, new KeybindList(KeyModifiers.Free, Keys.OemComma)},
            {SeekToStartOfSelection, new KeybindList(new [] {new Keybind(Keys.U), new Keybind(KeyModifiers.Shift, Keys.U)})},
            {SeekToEndOfSelection, new KeybindList(new [] {new Keybind(Keys.O), new Keybind(KeyModifiers.Shift, Keys.O)})},
            {SeekToStart, new KeybindList(KeyModifiers.Free, Keys.Home)},
            {SeekToEnd, new KeybindList(KeyModifiers.Free, Keys.End)},
            {IncreasePlaybackRate, new KeybindList(new[] {new Keybind(Keys.OemPlus), new Keybind(KeyModifiers.Ctrl, Keys.OemPlus)})},
            {DecreasePlaybackRate, new KeybindList(new[] {new Keybind(Keys.OemMinus), new Keybind(KeyModifiers.Ctrl, Keys.OemMinus)})},
            {ChangeToolUp, new KeybindList()},
            {ChangeToolDown, new KeybindList()},
            {ChangeToolToSelect, new KeybindList(Keys.Q)},
            {ChangeToolToNote, new KeybindList(Keys.W)},
            {ChangeToolToLongNote, new KeybindList(Keys.E)},
            {IncreaseSnap, new KeybindList(new[] {new Keybind(KeyModifiers.Free, Keys.Right), new Keybind(KeyModifiers.Free, Keys.L)})},
            {DecreaseSnap, new KeybindList(new[] {new Keybind(KeyModifiers.Free, Keys.Left), new Keybind(KeyModifiers.Free, Keys.J)})},
            {ChangeSnapTo1, new KeybindList(new [] {new Keybind(KeyModifiers.Alt, Keys.D1), new Keybind(KeyModifiers.Shift, Keys.D1)})},
            {ChangeSnapTo2, new KeybindList(new [] {new Keybind(KeyModifiers.Alt, Keys.D2), new Keybind(KeyModifiers.Shift, Keys.D2)})},
            {ChangeSnapTo3, new KeybindList(new [] {new Keybind(KeyModifiers.Alt, Keys.D3), new Keybind(KeyModifiers.Shift, Keys.D3)})},
            {ChangeSnapTo4, new KeybindList(new [] {new Keybind(KeyModifiers.Alt, Keys.D4), new Keybind(KeyModifiers.Shift, Keys.D4)})},
            {ChangeSnapTo5, new KeybindList(new [] {new Keybind(KeyModifiers.Alt, Keys.D5), new Keybind(KeyModifiers.Shift, Keys.D5)})},
            {ChangeSnapTo6, new KeybindList(new [] {new Keybind(KeyModifiers.Alt, Keys.D6), new Keybind(KeyModifiers.Shift, Keys.D6)})},
            {ChangeSnapTo7, new KeybindList(new [] {new Keybind(KeyModifiers.Alt, Keys.D7), new Keybind(KeyModifiers.Shift, Keys.D7)})},
            {ChangeSnapTo8, new KeybindList(new [] {new Keybind(KeyModifiers.Alt, Keys.D8), new Keybind(KeyModifiers.Shift, Keys.D8)})},
            {ChangeSnapTo9, new KeybindList(new [] {new Keybind(KeyModifiers.Alt, Keys.D9), new Keybind(KeyModifiers.Shift, Keys.D9)})},
            {ChangeSnapTo10, new KeybindList()},
            {ChangeSnapTo12, new KeybindList()},
            {ChangeSnapTo16, new KeybindList()},
            {OpenCustomSnapDialog, new KeybindList(new [] {new Keybind(KeyModifiers.Alt, Keys.D0), new Keybind(KeyModifiers.Shift, Keys.D0)})},
            {OpenMetadataDialog, new KeybindList(new [] {new Keybind(Keys.F1)})},
            {OpenModifiersDialog, new KeybindList(new [] {new Keybind(Keys.F2)})},
            {ToggleGotoPanel, new KeybindList(new [] {new Keybind(Keys.F3)})},
            {TestPlay, new KeybindList(new [] {new Keybind(Keys.F4)})},
            {ToggleBpmPanel, new KeybindList(new [] {new Keybind(Keys.F5)})},
            {ToggleSvPanel, new KeybindList(new [] {new Keybind(Keys.F6)})},
            {ToggleLayerViewMode, new KeybindList(KeyModifiers.Shift, Keys.D)},
            {ChangeSelectedLayerUp, new KeybindList(new[] {new Keybind(KeyModifiers.Shift, Keys.Q)})},
            {ChangeSelectedLayerDown, new KeybindList(new[] {new Keybind(KeyModifiers.Shift, Keys.W)})},
            {ToggleCurrentLayerVisibility, new KeybindList(KeyModifiers.Shift, Keys.H)},
            {ToggleAllLayersVisibility, new KeybindList(KeyModifiers.Shift, Keys.G)},
            {MoveSelectedNotesToCurrentLayer, new KeybindList(KeyModifiers.Shift, Keys.V)},
            {CreateNewLayer, new KeybindList(KeyModifiers.Shift, Keys.N)},
            {DeleteCurrentLayer, new KeybindList(KeyModifiers.Shift, Keys.Delete)},
            {RenameCurrentLayer, new KeybindList(KeyModifiers.Shift, Keys.R)},
            {RecolorCurrentLayer, new KeybindList(KeyModifiers.Shift, Keys.E)},
        };
    }
}