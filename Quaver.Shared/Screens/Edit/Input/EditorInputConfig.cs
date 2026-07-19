using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Config;
using Quaver.Shared.Input;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Platform;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using static Quaver.Shared.Screens.Edit.Input.EditorKeybindActions;

namespace Quaver.Shared.Screens.Edit.Input
{
    [Serializable]
    public class EditorInputConfig : IInputConfig<EditorKeybindActions>
    {
        public static string ConfigPath => ConfigManager.GameDirectory?.Value + "/editor_keys.yaml";

        private EditorInputConfigModel _model;

        private Dictionary<EditorKeybindActions, KeybindList> Keybinds => _model.Keybinds;
        private Dictionary<Keybind, HashSet<EditorKeybindActions>> _keybindActions = [];

        public ulong Version { get; private set; }

        // For Yaml
        public EditorInputConfig()
        {
        }

        public EditorInputConfig(EditorInputConfigModel model)
        {
            _model = model;
            RebuildReverseDictionary();
            Version++;
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<EditorKeybindActions, KeybindList> ReadOnlyKeybinds =>
            new ReadOnlyDictionary<EditorKeybindActions, KeybindList>(Keybinds);

        public IReadOnlyDictionary<string, KeybindList> PluginKeybinds =>
            new ReadOnlyDictionary<string, KeybindList>(_model.PluginKeybinds);

        private static EditorInputConfig Default() =>
            new(new EditorInputConfigModel(s_defaultKeybinds));

        public static EditorInputConfig LoadFromConfig()
        {
            var config = Default();

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
                config.RebuildReverseDictionary();
                Logger.Debug("Loaded editor key config", LogType.Runtime);
                config.SaveToConfig(); // Reformat after loading
            }
            catch (YamlException e)
            {
                Logger.Error(
                    $"Could not load editor key config, using default: {e}",
                    LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.Error($"Could not load editor key config, using default: {e.Message}",
                    LogType.Runtime);
            }

            return config;
        }

        public KeybindList GetOrDefault(EditorKeybindActions action) =>
            Keybinds.GetValueOrDefault(action, new KeybindList());

        public void AddKeybindToAction(EditorKeybindActions action, Keybind keybind)
        {
            if (!Keybinds.TryGetValue(action, out KeybindList? value))
                Keybinds.Add(action, new KeybindList(keybind));
            else
                value.Add(keybind);

            foreach (var matchingKeybind in keybind.MatchingKeybinds())
            {
                if (!_keybindActions.TryGetValue(matchingKeybind, out var actions))
                {
                    actions = [];
                    _keybindActions.Add(matchingKeybind, actions);
                }

                actions.Add(action);
            }

            Version++;
        }

        public bool RemoveKeybindFromAction(EditorKeybindActions action, Keybind keybind)
        {
            if (!Keybinds.TryGetValue(action, out KeybindList? value) || !value.Remove(keybind))
                return false;

            var remainingMatchingKeybinds =
                value.SelectMany(k => k.MatchingKeybinds()).ToHashSet();

            foreach (var matchingKeybind in keybind.MatchingKeybinds())
            {
                if (remainingMatchingKeybinds.Contains(matchingKeybind))
                    continue;

                if (_keybindActions.TryGetValue(matchingKeybind, out var actions))
                    actions.Remove(action);
            }

            Version++;
            return true;
        }

        /// <inheritdoc />
        public KeybindList? SetKeybindsForAction(EditorKeybindActions action,
            KeybindList keybindList)
        {
            // Remove from reverse dict
            var previousList = Keybinds.GetValueOrDefault(action) ?? [];
            foreach (var keybind in previousList.SelectMany(k => k.MatchingKeybinds()))
            {
                if (_keybindActions.TryGetValue(keybind, out var actions))
                {
                    actions.Remove(action);
                }
            }

            Keybinds[action] = keybindList;

            // Add to reverse dict
            foreach (var keybind in keybindList.SelectMany(k => k.MatchingKeybinds()))
            {
                if (!_keybindActions.TryGetValue(keybind, out var actions))
                {
                    actions = [];
                    _keybindActions.Add(keybind, actions);
                }

                actions.Add(action);
            }

            Version++;
            return previousList;
        }

        /// <inheritdoc />
        public bool TryGetActionsFor(Keybind keybind, out HashSet<EditorKeybindActions>? set) =>
            _keybindActions.TryGetValue(keybind, out set);

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

        public int FillMissingKeys(bool fillWithDefaultBinds)
        {
            var count = 0;

            foreach (var (action, defaultBind) in s_defaultKeybinds)
            {
                if (!Keybinds.ContainsKey(action))
                {
                    var bind = fillWithDefaultBinds ? defaultBind : new KeybindList();
                    Keybinds.Add(action, bind);
                    count++;
                }
            }

            RebuildReverseDictionary();

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
        public KeybindList DefaultKeybindsFor(EditorKeybindActions action) =>
            s_defaultKeybinds[action];

        public void ResetConfigFile()
        {
            _model.Keybinds = s_defaultKeybinds;
            _model.PluginKeybinds.Clear();
            RebuildReverseDictionary();
            SaveToConfig();
            Version++;
            Logger.Debug("Reset editor keybind config file", LogType.Runtime);
        }

        private void RebuildReverseDictionary()
        {
            _keybindActions = new Dictionary<Keybind, HashSet<EditorKeybindActions>>();

            foreach (var (action, keybinds) in Keybinds)
            {
                foreach (var keybind in keybinds.MatchingKeybinds())
                {
                    if (_keybindActions.ContainsKey(keybind))
                        _keybindActions[keybind].Add(action);
                    else
                        _keybindActions[keybind] = [action];
                }
            }

            Version++;
        }

        private static EditorInputConfig Deserialize(StreamReader file)
        {
            var ds = new DeserializerBuilder()
                .WithTypeConverter(new KeybindYamlTypeConverter())
                .IgnoreUnmatchedProperties()
                .Build();

            var config = ds.Deserialize<EditorInputConfigModel>(file);

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
        private static Dictionary<EditorKeybindActions, KeybindList> s_defaultKeybinds = new()
        {
            {ExitEditor, new KeybindList(Keys.Escape)},
            {PlayPause, new KeybindList(new[] {new Keybind(KeyModifiers.Free, Keys.Space), new Keybind(KeyModifiers.Free, Keys.Enter)})},
            {ZoomIn, new KeybindList(Keys.PageUp)},
            {ZoomInLarge, new KeybindList(KeyModifiers.Ctrl, Keys.PageUp)},
            {ZoomOut, new KeybindList(Keys.PageDown)},
            {ZoomOutLarge, new KeybindList(KeyModifiers.Ctrl, Keys.PageDown)},
            {SeekForwards, new KeybindList(new[] {new Keybind(Keys.Up), new Keybind(Keys.I), new Keybind(MouseScrollDirection.Up)})},
            {SeekBackwards, new KeybindList(new[] {new Keybind(Keys.Down), new Keybind(Keys.K), new Keybind(MouseScrollDirection.Down)})},
            {SeekForwardsAndSelect, new KeybindList(new[] {new Keybind(KeyModifiers.Shift, Keys.Up), new Keybind(KeyModifiers.Shift, Keys.I)})},
            {SeekBackwardsAndSelect, new KeybindList(new[] {new Keybind(KeyModifiers.Shift, Keys.Down), new Keybind(KeyModifiers.Shift, Keys.K)})},
            {SeekForwardsAndMove, new KeybindList(new[] {new Keybind(KeyModifiers.Alt, Keys.I)})},
            {SeekBackwardsAndMove, new KeybindList(new[] {new Keybind(KeyModifiers.Alt, Keys.K)})},
            {SeekForwardsLarge, new KeybindList(new[] {new Keybind(KeyModifiers.Ctrl, Keys.Up), new Keybind(KeyModifiers.Ctrl, Keys.I)})},
            {SeekBackwardsLarge, new KeybindList(new[] {new Keybind(KeyModifiers.Ctrl, Keys.Down), new Keybind(KeyModifiers.Ctrl, Keys.K)})},
            {SeekForwards1ms, new KeybindList(new [] {new Keybind(Keys.OemPeriod), new Keybind(KeyModifiers.Ctrl, Keys.OemPeriod)})},
            {SeekBackwards1ms,  new KeybindList(new [] {new Keybind(Keys.OemComma), new Keybind(KeyModifiers.Ctrl, Keys.OemComma)})},
            {SeekForwards1msAndSelect, new KeybindList(new[] {new Keybind(KeyModifiers.Shift, Keys.OemPeriod)})},
            {SeekBackwards1msAndSelect, new KeybindList(new[] {new Keybind(KeyModifiers.Shift, Keys.OemComma)})},
            {SeekForwards1msAndMove, new KeybindList(new[] {new Keybind(KeyModifiers.Alt, Keys.OemComma)})},
            {SeekBackwards1msAndMove, new KeybindList(new[] {new Keybind(KeyModifiers.Alt, Keys.OemPeriod)})},
            {SeekToStartOfSelection, new KeybindList(new [] {new Keybind(Keys.U), new Keybind(KeyModifiers.Alt, Keys.U)})},
            {SeekToEndOfSelection, new KeybindList(new [] {new Keybind(Keys.O), new Keybind(KeyModifiers.Alt, Keys.O)})},
            {SeekToStartOfSelectionAndSelect, new KeybindList(KeyModifiers.Shift, Keys.U)},
            {SeekToEndOfSelectionAndSelect, new KeybindList(KeyModifiers.Shift, Keys.O)},
            {SeekToStart, new KeybindList(new [] {new Keybind(Keys.Home), new Keybind(KeyModifiers.Alt, Keys.Home), new Keybind(KeyModifiers.Ctrl, Keys.Home)})},
            {SeekToEnd, new KeybindList(new [] {new Keybind(Keys.End), new Keybind(KeyModifiers.Alt, Keys.End), new Keybind(KeyModifiers.Ctrl, Keys.End)})},
            {SeekToStartAndSelect, new KeybindList(KeyModifiers.Shift, Keys.Home)},
            {SeekToEndAndSelect, new KeybindList(KeyModifiers.Shift, Keys.End)},
            {IncreasePlaybackRate, new KeybindList(new[] {new Keybind(Keys.OemPlus), new Keybind(KeyModifiers.Ctrl, Keys.OemPlus)})},
            {DecreasePlaybackRate, new KeybindList(new[] {new Keybind(Keys.OemMinus), new Keybind(KeyModifiers.Ctrl, Keys.OemMinus)})},
            {SetPreviewTime, new KeybindList(KeyModifiers.Ctrl, Keys.T)},
            {ChangeToolUp, new KeybindList()},
            {ChangeToolDown, new KeybindList()},
            {ChangeToolToSelect, new KeybindList(Keys.Q)},
            {ChangeToolToNote, new KeybindList(Keys.W)},
            {ChangeToolToLongNote, new KeybindList(Keys.E)},
            {ChangeToolToMine, new KeybindList(Keys.R)},
            {IncreaseSnap, new KeybindList(new[] {new Keybind(Keys.Right), new Keybind(KeyModifiers.Shift, Keys.Right), new Keybind(KeyModifiers.Free, Keys.L), new Keybind(KeyModifiers.Ctrl, MouseScrollDirection.Up)})},
            {DecreaseSnap, new KeybindList(new[] {new Keybind(Keys.Left), new Keybind(KeyModifiers.Shift, Keys.Left), new Keybind(KeyModifiers.Free, Keys.J), new Keybind(KeyModifiers.Ctrl, MouseScrollDirection.Down)})},
            {ChangeSnapTo1, new KeybindList(new [] {new Keybind(KeyModifiers.Shift, Keys.D1)})},
            {ChangeSnapTo2, new KeybindList(new [] {new Keybind(KeyModifiers.Shift, Keys.D2)})},
            {ChangeSnapTo3, new KeybindList(new [] {new Keybind(KeyModifiers.Shift, Keys.D3)})},
            {ChangeSnapTo4, new KeybindList(new [] {new Keybind(KeyModifiers.Shift, Keys.D4)})},
            {ChangeSnapTo5, new KeybindList(new [] {new Keybind(KeyModifiers.Shift, Keys.D5)})},
            {ChangeSnapTo6, new KeybindList(new [] {new Keybind(KeyModifiers.Shift, Keys.D6)})},
            {ChangeSnapTo7, new KeybindList(new [] {new Keybind(KeyModifiers.Shift, Keys.D7)})},
            {ChangeSnapTo8, new KeybindList(new [] {new Keybind(KeyModifiers.Shift, Keys.D8)})},
            {ChangeSnapTo9, new KeybindList(new [] {new Keybind(KeyModifiers.Shift, Keys.D9)})},
            {ChangeSnapTo10, new KeybindList()},
            {ChangeSnapTo12, new KeybindList()},
            {ChangeSnapTo16, new KeybindList()},
            {PlaceNoteAtLane1, new KeybindList(Keys.D1)},
            {PlaceNoteAtLane2, new KeybindList(Keys.D2)},
            {PlaceNoteAtLane3, new KeybindList(Keys.D3)},
            {PlaceNoteAtLane4, new KeybindList(Keys.D4)},
            {PlaceNoteAtLane5, new KeybindList(Keys.D5)},
            {PlaceNoteAtLane6, new KeybindList(Keys.D6)},
            {PlaceNoteAtLane7, new KeybindList(Keys.D7)},
            {PlaceNoteAtLane8, new KeybindList(Keys.D8)},
            {PlaceNoteAtLane9, new KeybindList(Keys.D9)},
            {PlaceNoteAtLane10, new KeybindList(Keys.D0)},
            {SwapNoteAtLane1, new KeybindList(KeyModifiers.Alt, Keys.D1)},
            {SwapNoteAtLane2, new KeybindList(KeyModifiers.Alt, Keys.D2)},
            {SwapNoteAtLane3, new KeybindList(KeyModifiers.Alt, Keys.D3)},
            {SwapNoteAtLane4, new KeybindList(KeyModifiers.Alt, Keys.D4)},
            {SwapNoteAtLane5, new KeybindList(KeyModifiers.Alt, Keys.D5)},
            {SwapNoteAtLane6, new KeybindList(KeyModifiers.Alt, Keys.D6)},
            {SwapNoteAtLane7, new KeybindList(KeyModifiers.Alt, Keys.D7)},
            {SwapNoteAtLane8, new KeybindList(KeyModifiers.Alt, Keys.D8)},
            {SwapNoteAtLane9, new KeybindList(KeyModifiers.Alt, Keys.D9)},
            {SwapNoteAtLane10, new KeybindList(KeyModifiers.Alt, Keys.D0)},
            {OpenCustomSnapDialog, new KeybindList(new [] {new Keybind(KeyModifiers.Alt, Keys.D0), new Keybind(KeyModifiers.Shift, Keys.D0)})},
            {OpenMetadataDialog, new KeybindList(new [] {new Keybind(Keys.F1)})},
            {OpenModifiersDialog, new KeybindList(new [] {new Keybind(Keys.F2)})},
            {OpenQuaFile, new KeybindList(KeyModifiers.Ctrl, Keys.Q)},
            {OpenFolder, new KeybindList(KeyModifiers.Ctrl, Keys.W)},
            {CreateNewDifficulty, new KeybindList(KeyModifiers.Ctrl, Keys.N)},
            {CreateNewDifficultyFromCurrent, new KeybindList(new Keybind(new List<KeyModifiers>{ KeyModifiers.Ctrl, KeyModifiers.Shift }, Keys.N))},
            {Export, new KeybindList(KeyModifiers.Ctrl, Keys.E)},
            {Upload, new KeybindList(KeyModifiers.Ctrl, Keys.U)},
            {UploadAndSubmitForRank, new KeybindList(new Keybind(new List<KeyModifiers> { KeyModifiers.Ctrl, KeyModifiers.Shift }, Keys.U))},
            {ToggleGotoPanel, new KeybindList(new [] {new Keybind(Keys.F3), new Keybind(Keys.F), new Keybind(KeyModifiers.Ctrl, Keys.F)})},
            {PlayTest, new KeybindList(new [] {new Keybind(Keys.F4)})},
            {PlayTestFromBeginning, new KeybindList(new [] {new Keybind(KeyModifiers.Ctrl, Keys.F4)})},
            {ToggleBpmPanel, new KeybindList(new [] {new Keybind(Keys.F5)})},
            {ToggleSvPanel, new KeybindList(new [] {new Keybind(Keys.F6)})},
            {ToggleAutoMod, new KeybindList(new [] {new Keybind(Keys.M), new Keybind(KeyModifiers.Ctrl, Keys.M)})},
            {ToggleHitsounds, new KeybindList(KeyModifiers.Ctrl, Keys.D8)},
            {ToggleMetronome, new KeybindList(KeyModifiers.Ctrl, Keys.D9)},
            {TogglePitchRate, new KeybindList(new [] {new Keybind(KeyModifiers.Ctrl, Keys.D0)})},
            {ToggleWaveform, new KeybindList(KeyModifiers.Alt, Keys.P)},
            {ToggleWaveformLowPass, new KeybindList(KeyModifiers.Alt, Keys.OemMinus)},
            {ToggleWaveformHighPass, new KeybindList(KeyModifiers.Alt, Keys.OemPlus)},
            {ToggleReferenceDifficulty, new KeybindList(KeyModifiers.Shift, Keys.P)},
            {NextReferenceDifficulty, new KeybindList(KeyModifiers.Shift, Keys.OemPlus)},
            {PreviousReferenceDifficulty, new KeybindList(KeyModifiers.Shift, Keys.OemMinus)},
            {ToggleGameplayPreview, new KeybindList(new [] {new Keybind(Keys.P), new Keybind(KeyModifiers.Ctrl, Keys.P)})},
            {ToggleLayerViewMode, new KeybindList(KeyModifiers.Shift, Keys.D)},
            {ToggleTimingGroupViewMode, new KeybindList(KeyModifiers.Alt, Keys.D)},
            {ChangeSelectedLayerUp, new KeybindList(new[] {new Keybind(KeyModifiers.Shift, Keys.Q)})},
            {ChangeSelectedLayerDown, new KeybindList(new[] {new Keybind(KeyModifiers.Shift, Keys.W)})},
            {ToggleCurrentLayerVisibility, new KeybindList(KeyModifiers.Shift, Keys.H)},
            {ToggleAllLayersVisibility, new KeybindList(KeyModifiers.Shift, Keys.G)},
            {MoveSelectedNotesToCurrentLayer, new KeybindList(KeyModifiers.Shift, Keys.V)},
            {CreateNewLayer, new KeybindList(KeyModifiers.Shift, Keys.N)},
            {DeleteCurrentLayer, new KeybindList(KeyModifiers.Shift, Keys.Delete)},
            {RenameCurrentLayer, new KeybindList(KeyModifiers.Shift, Keys.R)},
            {RecolorCurrentLayer, new KeybindList(KeyModifiers.Shift, Keys.E)},
            {MoveSelectedNotesToCurrentTimingGroup, new KeybindList(KeyModifiers.Alt, Keys.V)},
            {CreateNewTimingGroup, new KeybindList(KeyModifiers.Alt, Keys.N)},
            {DeleteCurrentTimingGroup, new KeybindList(KeyModifiers.Alt, Keys.Delete)},
            {RecolorCurrentTimingGroup, new KeybindList(KeyModifiers.Alt, Keys.E)},
            {Undo, new KeybindList(new[] {new Keybind(Keys.Z), new Keybind(KeyModifiers.Ctrl, Keys.Z)})},
            {Redo, new KeybindList(new[] {new Keybind(Keys.Y), new Keybind(KeyModifiers.Ctrl, Keys.Y)})},
            {Cut, new KeybindList(new[] {new Keybind(Keys.X), new Keybind(KeyModifiers.Ctrl, Keys.X)})},
            {DeleteSelection, new KeybindList(new[] {new Keybind(Keys.Back), new Keybind(Keys.Delete), new Keybind(KeyModifiers.Ctrl, Keys.Delete)})},
            {Copy, new KeybindList(new[] {new Keybind(Keys.C), new Keybind(KeyModifiers.Ctrl, Keys.C), new Keybind(KeyModifiers.Ctrl, Keys.Insert)})},
            {Paste, new KeybindList(new[] {new Keybind(new List<KeyModifiers> { KeyModifiers.Ctrl , KeyModifiers.Shift}, Keys.V)})},
            {PasteResnap, new KeybindList(new[] {new Keybind(Keys.V), new Keybind(KeyModifiers.Ctrl, Keys.V), new Keybind(Keys.Insert), new Keybind(KeyModifiers.Shift, Keys.Insert), new Keybind(KeyModifiers.Alt, Keys.Insert)})},
            {SelectNotesAtCurrentTime, new KeybindList(KeyModifiers.Shift, Keys.C)},
            {SelectAllNotes, new KeybindList(new[] {new Keybind(Keys.A), new Keybind(KeyModifiers.Ctrl, Keys.A)})},
            {SelectAllNotesInLayer, new KeybindList(new[] {new Keybind(KeyModifiers.Shift, Keys.A)})},
            {SelectAllNotesInTimingGroup, new KeybindList(new[] {new Keybind(KeyModifiers.Alt, Keys.A)})},
            {MirrorNotesLeftRight, new KeybindList(new []{new Keybind(Keys.H), new Keybind(KeyModifiers.Ctrl, Keys.H)})},
            {Save, new KeybindList(new[] {new Keybind(Keys.S), new Keybind(KeyModifiers.Ctrl, Keys.S)})},
            {Deselect, new KeybindList(new[] {new Keybind(Keys.D), new Keybind(KeyModifiers.Ctrl, Keys.D)})},
            {ApplyOffsetToMap, new KeybindList()},
            {ResnapToCurrentBeatSnap, new KeybindList(new[] {new Keybind(KeyModifiers.Ctrl, Keys.R)})},
            {AddBookmark, new KeybindList(KeyModifiers.Ctrl, Keys.B)},
            {SeekToLastBookmark, new KeybindList(KeyModifiers.Ctrl, Keys.Left)},
            {SeekToNextBookmark, new KeybindList(KeyModifiers.Ctrl, Keys.Right)}
        };
#pragma warning restore format // @formatter:on
    }
}