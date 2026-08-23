using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Config;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Platform;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Quaver.Shared.Input.Global
{
    [Serializable]
    public class GlobalInputConfig : IInputConfig<GlobalKeybindActions>
    {
        public static string ConfigPath =>
            ConfigManager.GameDirectory?.Value + "/quaver-keybinds.yaml";

        private GlobalInputConfigModel _model;

        private InputActionMap<GlobalKeybindActions> _keybinds;

        public ulong Version { get; private set; }

        public event ConfigUpdated? OnConfigUpdated;

        public delegate void ConfigUpdated();

        public FrozenSet<GlobalKeybindActions> ConflictingActions { get; private set; }

        public GlobalInputConfig(GlobalInputConfigModel model)
        {
            _model = model;
            _keybinds = new InputActionMap<GlobalKeybindActions>(model.Keybinds);
            Version++;
            CalculateConflictingActions();
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
            _keybinds.AddKeybindToAction(action, keybind);
            NotifyUpdate();
        }

        /// <inheritdoc />
        public bool RemoveKeybindFromAction(GlobalKeybindActions action, Keybind keybind)
        {
            if (!_keybinds.RemoveKeybindFromAction(action, keybind))
                return false;
            NotifyUpdate();
            return true;
        }

        public void CalculateConflictingActions()
        {
            var actionSets = _keybinds.CalculateConflictingActionSets();
            HashSet<GlobalKeybindActions> result = [];
            foreach (var (keybind, actions) in actionSets)
            {
                foreach (var (action1, action2) in from action1 in actions
                         from action2 in actions
                         where (action1.Layer() & action2.Layer()) != 0 && action1 < action2
                         select (action1, action2))
                {
                    if (action1 < action2)
                        Logger.Error($"{keybind} has both {action1} and {action2} in layer {action1.Layer() & action2.Layer()}", LogType.Runtime);
                    result.Add(action1);
                    result.Add(action2);
                }
            }

            ConflictingActions = result.ToFrozenSet();
        }

        private void NotifyUpdate()
        {
            Version++;
            CalculateConflictingActions();
            OnConfigUpdated?.Invoke();
        }

        /// <inheritdoc />
        public KeybindList? SetKeybindsForAction(GlobalKeybindActions action,
            KeybindList keybindList)
        {
            var result = _keybinds.SetKeybindsForAction(action, keybindList);
            NotifyUpdate();
            return result;
        }

        /// <inheritdoc />
        public bool TryGetActionsFor(Keybind keybind, out HashSet<GlobalKeybindActions>? set)
        {
            return _keybinds.TryGetActionsFor(keybind, out set);
        }

        private static GlobalInputConfig Default() =>
            new(new GlobalInputConfigModel(CloneDefaultKeybinds()));

        private static Dictionary<GlobalKeybindActions, KeybindList> CloneDefaultKeybinds() =>
            s_defaultKeybinds.ToDictionary(x => x.Key,
                x => new KeybindList(x.Value.Select(k => k.Clone())));

        public static GlobalInputConfig LoadFromConfig()
        {
            var config = Default();

            if (!File.Exists(ConfigPath))
            {
                Logger.Debug("No global key config found, using default", LogType.Runtime);
                config = DefaultFromLegacyConfig();
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
                var bind = fillWithDefaultBinds ? LegacyOrDefaultKeybindsFor(action) : [];
                if (_keybinds.SetKeybindsForActionIfNotExits(action, bind))
                    count++;
            }

            if (count > 0)
            {
                SaveToConfig();
                Logger.Debug($"Filled {count} missing action keybinds in key config file",
                    LogType.Runtime);
            }

            NotifyUpdate();
            return count;
        }

        /// <inheritdoc />
        public KeybindList DefaultKeybindsFor(GlobalKeybindActions action) =>
            s_defaultKeybinds[action];

        public void ResetConfigFile()
        {
            _model.Keybinds = CloneDefaultKeybinds();
            _keybinds = new InputActionMap<GlobalKeybindActions>(_model.Keybinds);
            SaveToConfig();
            NotifyUpdate();
            Logger.Debug("Reset global keybind config file", LogType.Runtime);
        }

#pragma warning disable format // @formatter:off
        private static KeybindList LegacyOrDefaultKeybindsFor(GlobalKeybindActions action)
        {
            KeybindList? keybinds = action switch
            {
                GlobalKeybindActions.Screenshot => Keybinds(ConfigManager.KeyScreenshot.Value),
                GlobalKeybindActions.IncreaseRate => Keybinds(KeyModifiers.Ctrl, ConfigManager.KeyIncreaseGameplayAudioRate.Value),
                GlobalKeybindActions.DecreaseRate => Keybinds(KeyModifiers.Ctrl, ConfigManager.KeyDecreaseGameplayAudioRate.Value),
                GlobalKeybindActions.IncreaseRateSmall => Keybinds([KeyModifiers.Ctrl, KeyModifiers.Shift], ConfigManager.KeyIncreaseGameplayAudioRate.Value),
                GlobalKeybindActions.DecreaseRateSmall => Keybinds([KeyModifiers.Ctrl, KeyModifiers.Shift], ConfigManager.KeyDecreaseGameplayAudioRate.Value),
                GlobalKeybindActions.TogglePitch => Keybinds([KeyModifiers.Ctrl, KeyModifiers.Free], ConfigManager.KeyTogglePitch.Value),
                GlobalKeybindActions.RemoveMods => Keybinds([KeyModifiers.Ctrl, KeyModifiers.Free], ConfigManager.KeyRemoveAllMods.Value),
                GlobalKeybindActions.ToggleMirror => Keybinds([KeyModifiers.Ctrl, KeyModifiers.Free], ConfigManager.KeyToggleMirror.Value),
                GlobalKeybindActions.IncreaseScrollSpeed => Keybinds(ConfigManager.KeyIncreaseScrollSpeed.Value),
                GlobalKeybindActions.DecreaseScrollSpeed => Keybinds(ConfigManager.KeyDecreaseScrollSpeed.Value),
                GlobalKeybindActions.IncreaseLocalScrollSpeed => Keybinds(KeyModifiers.Shift, ConfigManager.KeyIncreaseScrollSpeed.Value),
                GlobalKeybindActions.DecreaseLocalScrollSpeed => Keybinds(KeyModifiers.Shift, ConfigManager.KeyDecreaseScrollSpeed.Value),
                GlobalKeybindActions.IncreaseScrollSpeedSmall => Keybinds(KeyModifiers.Ctrl, ConfigManager.KeyIncreaseScrollSpeed.Value),
                GlobalKeybindActions.DecreaseScrollSpeedSmall => Keybinds(KeyModifiers.Ctrl, ConfigManager.KeyDecreaseScrollSpeed.Value),
                GlobalKeybindActions.IncreaseLocalScrollSpeedSmall => Keybinds([KeyModifiers.Ctrl, KeyModifiers.Shift], ConfigManager.KeyIncreaseScrollSpeed.Value),
                GlobalKeybindActions.DecreaseLocalScrollSpeedSmall => Keybinds([KeyModifiers.Ctrl, KeyModifiers.Shift], ConfigManager.KeyDecreaseScrollSpeed.Value),
                GlobalKeybindActions.IncreaseOffset => Keybinds(ConfigManager.KeyIncreaseMapOffset.Value),
                GlobalKeybindActions.DecreaseOffset => Keybinds(ConfigManager.KeyDecreaseMapOffset.Value),
                GlobalKeybindActions.IncreaseOffsetSmall => Keybinds(KeyModifiers.Ctrl, ConfigManager.KeyIncreaseMapOffset.Value),
                GlobalKeybindActions.DecreaseOffsetSmall => Keybinds(KeyModifiers.Ctrl, ConfigManager.KeyDecreaseMapOffset.Value),
                GlobalKeybindActions.IncreaseVisualOffset => Keybinds(KeyModifiers.Alt, ConfigManager.KeyIncreaseMapOffset.Value),
                GlobalKeybindActions.DecreaseVisualOffset => Keybinds(KeyModifiers.Alt, ConfigManager.KeyDecreaseMapOffset.Value),
                GlobalKeybindActions.IncreaseVisualOffsetSmall => Keybinds([KeyModifiers.Alt, KeyModifiers.Ctrl], ConfigManager.KeyIncreaseMapOffset.Value),
                GlobalKeybindActions.DecreaseVisualOffsetSmall => Keybinds([KeyModifiers.Alt, KeyModifiers.Ctrl], ConfigManager.KeyDecreaseMapOffset.Value),
                GlobalKeybindActions.GameplayPause => Keybinds(ConfigManager.KeyPause.Value),
                GlobalKeybindActions.GameplayToggleScoreboard => Keybinds(ConfigManager.KeyScoreboardVisible.Value),
                GlobalKeybindActions.GameplayToggleOverlay => Keybinds(ConfigManager.KeyToggleOverlay.Value),
                GlobalKeybindActions.GameplayRetry => Keybinds(ConfigManager.KeyRestartMap.Value),
                GlobalKeybindActions.GameplayQuickExit => Keybinds(ConfigManager.KeyQuickExit.Value),
                GlobalKeybindActions.GameplaySkipIntro => Keybinds(ConfigManager.KeySkipIntro.Value),
                GlobalKeybindActions.GameplayTogglePlaytestAutoplay => Keybinds(ConfigManager.KeyTogglePlaytestAutoplay.Value),
                GlobalKeybindActions.ResetOffset => Keybinds(ConfigManager.KeyResetMapOffset.Value),
                GlobalKeybindActions.ResetVisualOffset => Keybinds(KeyModifiers.Alt, ConfigManager.KeyResetMapOffset.Value),
                GlobalKeybindActions.NavigateLeft => Keybinds(ConfigManager.KeyNavigateLeft.Value),
                GlobalKeybindActions.NavigateRight => Keybinds(ConfigManager.KeyNavigateRight.Value),
                GlobalKeybindActions.NavigateUp => Keybinds(ConfigManager.KeyNavigateUp.Value),
                GlobalKeybindActions.NavigateDown => Keybinds(ConfigManager.KeyNavigateDown.Value),
                GlobalKeybindActions.NavigateSelect => Keybinds(ConfigManager.KeyNavigateSelect.Value),
                GlobalKeybindActions.ResultsTab => Keybinds(new Keybind(KeyModifiers.Free, Keys.Tab)),
                GlobalKeybindActions.SelectionToggleModifiers => Keybinds(Keys.F1),
                GlobalKeybindActions.SelectionSelectRandomMap => Keybinds(Keys.F2),
                GlobalKeybindActions.SelectionSelectPreviousRandomMap => Keybinds(KeyModifiers.Shift, Keys.F2),
                GlobalKeybindActions.SelectionToggleMapPreview => Keybinds(Keys.F3),
                GlobalKeybindActions.SelectionToggleUserProfile => Keybinds(Keys.F4),
                GlobalKeybindActions.SelectionRefresh => Keybinds(KeyModifiers.Free, Keys.F5),
                _ => null
            };

            return keybinds ?? new KeybindList(s_defaultKeybinds[action].Select(k => k.Clone()));
        }
#pragma warning enable format // @formatter:on

        private static KeybindList Keybinds(Keys key) =>
            new(new Keybind(key));

        private static KeybindList Keybinds(KeyModifiers mods, Keys key) =>
            new(new Keybind(mods, key));

        private static KeybindList Keybinds(GenericKey key) =>
            new(new Keybind([], key));

        private static KeybindList Keybinds(ICollection<KeyModifiers> modifiers, Keys key) =>
            new(new Keybind(modifiers, key));

        private static KeybindList Keybinds(Keybind keybind) => new(keybind);

        private static GlobalInputConfig DefaultFromLegacyConfig()
        {
            var keybinds = s_defaultKeybinds.Keys.ToDictionary(x => x,
                LegacyOrDefaultKeybindsFor);

            return new GlobalInputConfig(new GlobalInputConfigModel(keybinds));
        }

        private sealed class SerializedGlobalInputConfigModel
        {
            public Dictionary<string, KeybindList> Keybinds { get; set; } = [];
        }


        private static GlobalInputConfig Deserialize(StreamReader file)
        {
            var ds = new DeserializerBuilder()
                .WithTypeConverter(new KeybindYamlTypeConverter())
                .IgnoreUnmatchedProperties()
                .Build();

            // This is here to handle entries that have been removed
            var serialized = ds.Deserialize<SerializedGlobalInputConfigModel>(file);

            if (serialized == null)
            {
                Logger.Debug("Config file was empty, creating new default", LogType.Runtime);
                return Default();
            }

            var keybinds = new Dictionary<GlobalKeybindActions, KeybindList>();

            foreach (var (name, binds) in serialized.Keybinds)
            {
                if (!Enum.TryParse(name, out GlobalKeybindActions action))
                {
                    Logger.Debug($"Ignoring unknown global keybind action: {name}", LogType.Runtime);
                    continue;
                }

                if (!s_defaultKeybinds.ContainsKey(action))
                {
                    Logger.Debug($"Ignoring unsupported global keybind action: {name}", LogType.Runtime);
                    continue;
                }

                keybinds[action] = binds;
            }


            return new GlobalInputConfig(new GlobalInputConfigModel(keybinds));
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
        {
            [GlobalKeybindActions.Screenshot] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.F12)),
            [GlobalKeybindActions.OpenOptions] = new KeybindList(new Keybind([KeyModifiers.Ctrl, KeyModifiers.Free], Keys.O)),
            [GlobalKeybindActions.ToggleFullscreen] = new KeybindList(new Keybind([KeyModifiers.Alt, KeyModifiers.Free], Keys.Enter)),
            [GlobalKeybindActions.TogglePause] = new KeybindList(new Keybind([KeyModifiers.Ctrl, KeyModifiers.Free], Keys.P)),
            [GlobalKeybindActions.CycleFpsLimiter] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.F7)),
            [GlobalKeybindActions.ToggleOnlineHub] = new KeybindList([
                new Keybind(KeyModifiers.Free, Keys.F8),
                new Keybind(KeyModifiers.Free, Keys.F9)
            ]),
            [GlobalKeybindActions.ReloadSkin] = new KeybindList(new Keybind([KeyModifiers.Ctrl, KeyModifiers.Free], Keys.S)),
            [GlobalKeybindActions.Back] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Escape)),
            [GlobalKeybindActions.IncreaseRate] = new KeybindList(new Keybind([KeyModifiers.Ctrl], Keys.OemPlus)),
            [GlobalKeybindActions.DecreaseRate] = new KeybindList(new Keybind([KeyModifiers.Ctrl], Keys.OemMinus)),
            [GlobalKeybindActions.IncreaseRateSmall] = new KeybindList(new Keybind([KeyModifiers.Ctrl, KeyModifiers.Shift], Keys.OemPlus)),
            [GlobalKeybindActions.DecreaseRateSmall] = new KeybindList(new Keybind([KeyModifiers.Ctrl, KeyModifiers.Shift], Keys.OemMinus)),
            [GlobalKeybindActions.TogglePitch] = new KeybindList(new Keybind([KeyModifiers.Ctrl, KeyModifiers.Free], Keys.OemPipe)),
            [GlobalKeybindActions.RemoveMods] = new KeybindList(new Keybind([KeyModifiers.Ctrl, KeyModifiers.Free], Keys.D0)),
            [GlobalKeybindActions.ToggleMirror] = new KeybindList(new Keybind([KeyModifiers.Ctrl, KeyModifiers.Free], Keys.H)),
            [GlobalKeybindActions.IncreaseScrollSpeed] = new KeybindList(new Keybind(Keys.F4)),
            [GlobalKeybindActions.DecreaseScrollSpeed] = new KeybindList(new Keybind(Keys.F3)),
            [GlobalKeybindActions.IncreaseLocalScrollSpeed] = new KeybindList(new Keybind(KeyModifiers.Shift, Keys.F4)),
            [GlobalKeybindActions.DecreaseLocalScrollSpeed] = new KeybindList(new Keybind(KeyModifiers.Shift, Keys.F3)),
            [GlobalKeybindActions.IncreaseScrollSpeedSmall] = new KeybindList(new Keybind(KeyModifiers.Ctrl, Keys.F4)),
            [GlobalKeybindActions.DecreaseScrollSpeedSmall] = new KeybindList(new Keybind(KeyModifiers.Ctrl, Keys.F3)),
            [GlobalKeybindActions.IncreaseLocalScrollSpeedSmall] = new KeybindList(new Keybind([KeyModifiers.Shift, KeyModifiers.Ctrl], Keys.F4)),
            [GlobalKeybindActions.DecreaseLocalScrollSpeedSmall] = new KeybindList(new Keybind([KeyModifiers.Shift, KeyModifiers.Ctrl], Keys.F3)),
            [GlobalKeybindActions.IncreaseOffset] = new KeybindList(new Keybind(Keys.OemPlus)),
            [GlobalKeybindActions.DecreaseOffset] = new KeybindList(new Keybind(Keys.OemMinus)),
            [GlobalKeybindActions.IncreaseOffsetSmall] = new KeybindList(new Keybind(KeyModifiers.Ctrl, Keys.OemPlus)),
            [GlobalKeybindActions.DecreaseOffsetSmall] = new KeybindList(new Keybind(KeyModifiers.Ctrl, Keys.OemMinus)),
            [GlobalKeybindActions.IncreaseVisualOffset] = new KeybindList(new Keybind(KeyModifiers.Alt, Keys.OemPlus)),
            [GlobalKeybindActions.DecreaseVisualOffset] = new KeybindList(new Keybind(KeyModifiers.Alt, Keys.OemMinus)),
            [GlobalKeybindActions.IncreaseVisualOffsetSmall] = new KeybindList(new Keybind([KeyModifiers.Ctrl, KeyModifiers.Alt], Keys.OemPlus)),
            [GlobalKeybindActions.DecreaseVisualOffsetSmall] = new KeybindList(new Keybind([KeyModifiers.Ctrl, KeyModifiers.Alt], Keys.OemMinus)),
            [GlobalKeybindActions.GameplayPause] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Escape)),
            [GlobalKeybindActions.GameplayToggleScoreboard] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Tab)),
            [GlobalKeybindActions.GameplayToggleOverlay] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.F8)),
            [GlobalKeybindActions.GameplayRetry] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.OemTilde)),
            [GlobalKeybindActions.GameplayQuickExit] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.F1)),
            [GlobalKeybindActions.GameplaySkipIntro] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Space)),
            [GlobalKeybindActions.GameplayTogglePlaytestAutoplay] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Tab)),
            [GlobalKeybindActions.ResetOffset] = new KeybindList(new Keybind(Keys.D0)),
            [GlobalKeybindActions.ResetVisualOffset] = new KeybindList(new Keybind(KeyModifiers.Alt, Keys.D0)),
            [GlobalKeybindActions.NavigateLeft] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Left)),
            [GlobalKeybindActions.NavigateRight] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Right)),
            [GlobalKeybindActions.NavigateUp] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Up)),
            [GlobalKeybindActions.NavigateDown] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Down)),
            [GlobalKeybindActions.NavigateSelect] = new KeybindList(new Keybind(Keys.Enter)),
            [GlobalKeybindActions.ResultsTab] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Tab)),
            [GlobalKeybindActions.SelectionToggleModifiers] = new KeybindList(new Keybind(Keys.F1)),
            [GlobalKeybindActions.SelectionSelectRandomMap] = new KeybindList(new Keybind(Keys.F2)),
            [GlobalKeybindActions.SelectionSelectPreviousRandomMap] = new KeybindList(new Keybind(KeyModifiers.Shift, Keys.F2)),
            [GlobalKeybindActions.SelectionToggleMapPreview] = new KeybindList(new Keybind(Keys.F3)),
            [GlobalKeybindActions.SelectionToggleUserProfile] = new KeybindList(new Keybind(Keys.F4)),
            [GlobalKeybindActions.SelectionRefresh] = new KeybindList(new Keybind(Keys.F5)),
        };
#pragma warning restore format // @formatter:on
    }
}
