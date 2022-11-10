using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Notifications;
using Wobble.Logging;
using Wobble.Platform;
using YamlDotNet.Serialization;
using static Quaver.Shared.Screens.Edit.Input.KeybindActions;

namespace Quaver.Shared.Screens.Edit.Input
{
    [Serializable]
    public class EditorInputConfig
    {
        [YamlIgnore] public static string ConfigPath = ConfigManager.GameDirectory.Value + "/editor_keys.yaml";

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
            EditorInputConfig config;

            if (!File.Exists(ConfigPath))
            {
                Logger.Debug("No editor key config found, using default", LogType.Runtime);
                config = new EditorInputConfig();
            }
            else
            {
                using (var file = File.OpenText(EditorInputConfig.ConfigPath))
                {
                    config = EditorInputConfig.Deserialize(file);
                }

                Logger.Debug("Loaded editor key config", LogType.Runtime);
            }

            config.SaveToConfig(); // Reformat after loading

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
            File.WriteAllText(ConfigPath, Serialize());
            Logger.Debug("Saved editor key config to file", LogType.Runtime);
        }

        public void OpenConfigFile()
        {
            try
            {
                Utils.NativeUtils.OpenNatively(ConfigPath);
            }
            catch (Exception e)
            {
                NotificationManager.Show(NotificationLevel.Error, "An error occurred while opening the config file.");
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
                NotificationManager.Show(NotificationLevel.Success, $"Filled {count} missing action keybinds");
            }
            else
                NotificationManager.Show(NotificationLevel.Info, $"No actions keybinds are missing");
        }

        public void ResetConfigFile()
        {
            ReverseScrollSeekDirection = true;
            Keybinds = DefaultKeybinds;
            PluginKeybinds = new Dictionary<string, KeybindList>();
            SaveToConfig();
            NotificationManager.Show(NotificationLevel.Success, "Reset all editor keybinds");
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
            {SeekToBeginning, new KeybindList(KeyModifiers.Free, Keys.Home)},
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
            {ToggleLayerColorMode, new KeybindList(KeyModifiers.Shift, Keys.D)},
            {ChangeSelectedLayerUp, new KeybindList(new[] {new Keybind(KeyModifiers.Shift, Keys.Q)})},
            {ChangeSelectedLayerDown, new KeybindList(new[] {new Keybind(KeyModifiers.Shift, Keys.W)})},
            {ToggleCurrentLayerVisibility, new KeybindList(KeyModifiers.Shift, Keys.H)},
            {ToggleAllLayersVisibility, new KeybindList(KeyModifiers.Shift, Keys.G)},
            {MoveSelectedNotesToCurrentLayer, new KeybindList(KeyModifiers.Shift, Keys.V)},
            {CreateNewLayer, new KeybindList(KeyModifiers.Shift, Keys.N)},
            {DeleteCurrentLayer, new KeybindList(KeyModifiers.Shift, Keys.Delete)},
            {RenameCurrentLayer, new KeybindList(KeyModifiers.Shift, Keys.R)},
            {RecolorCurrentLayer, new KeybindList(KeyModifiers.Shift, Keys.E)},
            {CreateNewMapset, new KeybindList()},
            {CreateNewDifficulty, new KeybindList(KeyModifiers.Ctrl, Keys.N)},
            {CreateNewDifficultyFromCurrent, new KeybindList(new Keybind(new[] {KeyModifiers.Ctrl, KeyModifiers.Shift}, Keys.N))},
            {RefreshFileCache, new KeybindList()},
            {OpenMapFile, new KeybindList(KeyModifiers.Ctrl, Keys.Q)},
            {OpenMapDirectory, new KeybindList(KeyModifiers.Ctrl, Keys.W)},
            {Export, new KeybindList(KeyModifiers.Ctrl, Keys.E)},
            {UndoAction, new KeybindList(new[] {new Keybind(KeyModifiers.Ctrl, Keys.Z), new Keybind(Keys.Z),})},
            {RedoAction, new KeybindList(new[] {new Keybind(KeyModifiers.Ctrl, Keys.Y), new Keybind(new[] {KeyModifiers.Ctrl, KeyModifiers.Shift}, Keys.Z), new Keybind(Keys.Y),})},
            {SaveMap, new KeybindList(new[] {new Keybind(KeyModifiers.Ctrl, Keys.S), new Keybind(Keys.S),})},
            {UploadMapset, new KeybindList(KeyModifiers.Ctrl, Keys.U)},
            {SetPreviewPoint, new KeybindList(KeyModifiers.Ctrl, Keys.T)},
            {AdjustOffset, new KeybindList(KeyModifiers.Ctrl, Keys.B)},
            {SubmitForRanked, new KeybindList(new Keybind(new[] {KeyModifiers.Ctrl, KeyModifiers.Shift}, Keys.U))},
            {ToggleAutomod, new KeybindList(new [] {new Keybind(Keys.M), new Keybind(KeyModifiers.Ctrl, Keys.M)})},
            {ToggleGameplayPreview, new KeybindList(Keys.P)},
            {ToggleReferenceDifficulty, new KeybindList(KeyModifiers.Shift, Keys.P)},
            {ChangeReferenceDifficultyPrevious, new KeybindList(KeyModifiers.Shift, Keys.OemMinus)},
            {ChangeReferenceDifficultyNext, new KeybindList(KeyModifiers.Shift, Keys.OemPlus)},
            {ToggleWaveform, new KeybindList(KeyModifiers.Alt, Keys.P)},
            {ToggleWaveformLowPassFilter, new KeybindList(KeyModifiers.Alt, Keys.OemMinus)},
            {ToggleWaveformHighPassFilter, new KeybindList(KeyModifiers.Alt, Keys.OemPlus)},
            {ToggleHitsounds, new KeybindList(Keys.D8)},
            {ToggleMetronome, new KeybindList(Keys.D9)},
            {TogglePitchWithRate, new KeybindList(KeyModifiers.Ctrl, Keys.D0)},
            {ToggleLivePlaytest, new KeybindList(Keys.Tab)},
            {OpenGameplayTestModifiers, new KeybindList(Keys.F2)},
            {StartGameplayTest, new KeybindList(Keys.F4)},
            {StartGameplayTestFromBeginning, new KeybindList(KeyModifiers.Ctrl, Keys.F4)},
            {CutNotes, new KeybindList(new[] {new Keybind(KeyModifiers.Ctrl, Keys.X), new Keybind(Keys.X),})},
            {CopyNotes, new KeybindList(new[] {new Keybind(KeyModifiers.Ctrl, Keys.C), new Keybind(Keys.C),})},
            {PasteNotes, new KeybindList(new[] {new Keybind(KeyModifiers.Ctrl, Keys.V), new Keybind(Keys.V), new Keybind(Keys.Insert),})},
            {PasteNoResnap, new KeybindList(new Keybind(new[] {KeyModifiers.Ctrl, KeyModifiers.Shift}, Keys.V))},
            {DeleteCurrentNotesOrSelection, new KeybindList(new[] {new Keybind(Keys.Delete), new Keybind(Keys.Back)})},
            {SelectCurrentNotes, new KeybindList(new[] {new Keybind(KeyModifiers.Shift, Keys.C)})},
            {SelectAll, new KeybindList(new[] {new Keybind(KeyModifiers.Ctrl, Keys.A), new Keybind(Keys.A)})},
            {SelectAllInLayer, new KeybindList(new[] {new Keybind(new[] {KeyModifiers.Ctrl, KeyModifiers.Shift}, Keys.A), new Keybind(KeyModifiers.Shift, Keys.A)})},
            {DeselectAll, new KeybindList(new[] {new Keybind(KeyModifiers.Ctrl, Keys.D), new Keybind(Keys.D)})},
            {MirrorNotesLeftRight, new KeybindList(new[] {new Keybind(KeyModifiers.Ctrl, Keys.H), new Keybind(Keys.H)})},
            {MirrorNotesUpDown, new KeybindList(new[] {new Keybind(KeyModifiers.Ctrl, Keys.G), new Keybind(Keys.G)})},
            {ResnapAllNotes, new KeybindList(new[] {new Keybind(KeyModifiers.Ctrl, Keys.R), new Keybind(Keys.R)})},
            {PlaceNoteLane1, new KeybindList(Keys.D1)},
            {PlaceNoteLane2, new KeybindList(Keys.D2)},
            {PlaceNoteLane3, new KeybindList(Keys.D3)},
            {PlaceNoteLane4, new KeybindList(Keys.D4)},
            {PlaceNoteLane5, new KeybindList(Keys.D5)},
            {PlaceNoteLane6, new KeybindList(Keys.D6)},
            {PlaceNoteLane7, new KeybindList(Keys.D7)},
            {PlaceTimingPoint, new KeybindList()},
            {PlaceScrollVelocity, new KeybindList()},
            {ToggleMetadataPanel, new KeybindList(Keys.F1)},
            {ToggleTimingPointPanel, new KeybindList(Keys.F5)},
            {ToggleScrollVelocityPanel, new KeybindList(Keys.F6)},
            {ToggleGoToObjectsPanel, new KeybindList(new[] {new Keybind(KeyModifiers.Ctrl, Keys.F), new Keybind(Keys.F), new Keybind(Keys.F3),})},
            {CloseAllPlugins, new KeybindList(KeyModifiers.Ctrl, Keys.W)},
            {ReloadPlugins, new KeybindList()},
        };
    }
}