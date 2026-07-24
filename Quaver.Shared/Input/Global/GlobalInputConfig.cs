using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            new(new GlobalInputConfigModel(CloneDefaultKeybinds()));

        private static Dictionary<GlobalKeybindActions, KeybindList> CloneDefaultKeybinds() =>
            s_defaultKeybinds.ToDictionary(x => x.Key, x => new KeybindList(x.Value.Select(k => k.Clone())));

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

            Version++;
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
            Version++;
            Logger.Debug("Reset global keybind config file", LogType.Runtime);
        }

        private static KeybindList LegacyOrDefaultKeybindsFor(GlobalKeybindActions action)
        {
            KeybindList? keybinds = action switch
            {
                GlobalKeybindActions.Screenshot => Keybinds(ConfigManager.KeyScreenshot.Value),
                GlobalKeybindActions.IncreaseRate => Keybinds([KeyModifiers.Ctrl, KeyModifiers.Free],
                    ConfigManager.KeyIncreaseGameplayAudioRate.Value),
                GlobalKeybindActions.DecreaseRate => Keybinds([KeyModifiers.Ctrl, KeyModifiers.Free],
                    ConfigManager.KeyDecreaseGameplayAudioRate.Value),
                GlobalKeybindActions.TogglePitch => Keybinds([KeyModifiers.Ctrl, KeyModifiers.Free],
                    ConfigManager.KeyTogglePitch.Value),
                GlobalKeybindActions.RemoveMods => Keybinds([KeyModifiers.Ctrl, KeyModifiers.Free],
                    ConfigManager.KeyRemoveAllMods.Value),
                GlobalKeybindActions.ToggleMirror => Keybinds([KeyModifiers.Ctrl, KeyModifiers.Free],
                    ConfigManager.KeyToggleMirror.Value),
                GlobalKeybindActions.IncreaseLocalScrollSpeed => Keybinds(ConfigManager.KeyIncreaseScrollSpeed.Value),
                GlobalKeybindActions.DecreaseLocalScrollSpeed => Keybinds(ConfigManager.KeyDecreaseScrollSpeed.Value),
                GlobalKeybindActions.IncreaseOffset => Keybinds(ConfigManager.KeyIncreaseMapOffset.Value),
                GlobalKeybindActions.DecreaseOffset => Keybinds(ConfigManager.KeyDecreaseMapOffset.Value),
                GlobalKeybindActions.GameplayPause => Keybinds(ConfigManager.KeyPause.Value),
                GlobalKeybindActions.GameplayPauseUp => Keybinds(ConfigManager.KeyPause.Value),
                GlobalKeybindActions.GameplayPauseDown => Keybinds(ConfigManager.KeyPause.Value),
                GlobalKeybindActions.GameplayToggleScoreboard => Keybinds(ConfigManager.KeyScoreboardVisible.Value),
                GlobalKeybindActions.GameplayToggleOverlay => Keybinds(ConfigManager.KeyToggleOverlay.Value),
                GlobalKeybindActions.GameplayRetry => Keybinds(ConfigManager.KeyRestartMap.Value),
                GlobalKeybindActions.GameplayQuickExit => Keybinds(ConfigManager.KeyQuickExit.Value),
                GlobalKeybindActions.GameplaySkipIntro => Keybinds(ConfigManager.KeySkipIntro.Value),
                GlobalKeybindActions.GameplayTogglePlaytestAutoplay => Keybinds(ConfigManager.KeyTogglePlaytestAutoplay.Value),
                GlobalKeybindActions.GameplayIncreaseOffset => Keybinds(ConfigManager.KeyIncreaseMapOffset.Value),
                GlobalKeybindActions.GameplayDecreaseOffset => Keybinds(ConfigManager.KeyDecreaseMapOffset.Value),
                GlobalKeybindActions.GameplayResetOffset => Keybinds(ConfigManager.KeyResetMapOffset.Value),
                GlobalKeybindActions.NavigateLeft => Keybinds(ConfigManager.KeyNavigateLeft.Value),
                GlobalKeybindActions.NavigateRight => Keybinds(ConfigManager.KeyNavigateRight.Value),
                GlobalKeybindActions.NavigateUp => Keybinds(ConfigManager.KeyNavigateUp.Value),
                GlobalKeybindActions.NavigateDown => Keybinds(ConfigManager.KeyNavigateDown.Value),
                GlobalKeybindActions.NavigateSelect => Keybinds(ConfigManager.KeyNavigateSelect.Value),
                GlobalKeybindActions.EditorPausePlay => Keybinds(ConfigManager.KeyEditorPausePlay.Value),
                GlobalKeybindActions.EditorDecreaseRate => Keybinds(ConfigManager.KeyEditorDecreaseAudioRate.Value),
                GlobalKeybindActions.EditorIncreaseRate => Keybinds(ConfigManager.KeyEditorIncreaseAudioRate.Value),
                GlobalKeybindActions.ResultsRetry => Keybinds(ConfigManager.KeyRestartMap.Value),
                GlobalKeybindActions.ResultsTab => Keybinds(new Keybind(KeyModifiers.Free, Keys.Tab)),
                GlobalKeybindActions.DialogConfirm => Keybinds(ConfigManager.KeyNavigateSelect.Value),
                GlobalKeybindActions.DialogCancel => Keybinds(ConfigManager.KeyNavigateBack.Value),
                _ => null
            };

            return keybinds ?? new KeybindList(s_defaultKeybinds[action].Select(k => k.Clone()));
        }

        private static KeybindList Keybinds(Keys key) =>
            new(new Keybind(KeyModifiers.Free, key));

        private static KeybindList Keybinds(GenericKey key) =>
            new(new Keybind([KeyModifiers.Free], key));

        private static KeybindList Keybinds(ICollection<KeyModifiers> modifiers, Keys key) =>
            new(new Keybind(modifiers, key));

        private static KeybindList Keybinds(Keybind keybind) => new(keybind);

        private static GlobalInputConfig DefaultFromLegacyConfig()
        {
            var keybinds = s_defaultKeybinds.Keys.ToDictionary(x => x,
                LegacyOrDefaultKeybindsFor);

            return new GlobalInputConfig(new GlobalInputConfigModel(keybinds));
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
            [GlobalKeybindActions.IncreaseRate] = new KeybindList(new Keybind([KeyModifiers.Ctrl, KeyModifiers.Free], Keys.OemPlus)),
            [GlobalKeybindActions.DecreaseRate] = new KeybindList(new Keybind([KeyModifiers.Ctrl, KeyModifiers.Free], Keys.OemMinus)),
            [GlobalKeybindActions.TogglePitch] = new KeybindList(new Keybind([KeyModifiers.Ctrl, KeyModifiers.Free], Keys.OemPipe)),
            [GlobalKeybindActions.RemoveMods] = new KeybindList(new Keybind([KeyModifiers.Ctrl, KeyModifiers.Free], Keys.D0)),
            [GlobalKeybindActions.ToggleMirror] = new KeybindList(new Keybind([KeyModifiers.Ctrl, KeyModifiers.Free], Keys.H)),
            [GlobalKeybindActions.IncreaseScrollSpeed] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.F4)),
            [GlobalKeybindActions.DecreaseScrollSpeed] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.F3)),
            [GlobalKeybindActions.IncreaseLocalScrollSpeed] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.F4)),
            [GlobalKeybindActions.DecreaseLocalScrollSpeed] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.F3)),
            [GlobalKeybindActions.IncreaseOffset] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.OemPlus)),
            [GlobalKeybindActions.DecreaseOffset] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.OemMinus)),
            [GlobalKeybindActions.IncreaseOffsetSmall] = new KeybindList(new Keybind([KeyModifiers.Ctrl, KeyModifiers.Free], Keys.OemPlus)),
            [GlobalKeybindActions.DecreaseOffsetSmall] = new KeybindList(new Keybind([KeyModifiers.Ctrl, KeyModifiers.Free], Keys.OemMinus)),
            [GlobalKeybindActions.GameplayPause] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Escape)),
            [GlobalKeybindActions.GameplayPauseUp] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Escape)),
            [GlobalKeybindActions.GameplayPauseDown] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Escape)),
            [GlobalKeybindActions.GameplayToggleScoreboard] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Tab)),
            [GlobalKeybindActions.GameplayToggleOverlay] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.F8)),
            [GlobalKeybindActions.GameplayRetry] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.OemTilde)),
            [GlobalKeybindActions.GameplayQuickExit] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.F1)),
            [GlobalKeybindActions.GameplaySkipIntro] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Space)),
            [GlobalKeybindActions.GameplayTogglePlaytestAutoplay] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Tab)),
            [GlobalKeybindActions.GameplayIncreaseOffset] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.OemPlus)),
            [GlobalKeybindActions.GameplayDecreaseOffset] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.OemMinus)),
            [GlobalKeybindActions.GameplayResetOffset] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.D0)),
            [GlobalKeybindActions.NavigateLeft] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Left)),
            [GlobalKeybindActions.NavigateRight] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Right)),
            [GlobalKeybindActions.NavigateUp] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Up)),
            [GlobalKeybindActions.NavigateDown] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Down)),
            [GlobalKeybindActions.NavigateSelect] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Enter)),
            [GlobalKeybindActions.EditorPausePlay] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Space)),
            [GlobalKeybindActions.EditorDecreaseRate] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.OemMinus)),
            [GlobalKeybindActions.EditorIncreaseRate] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.OemPlus)),
            [GlobalKeybindActions.ResultsRetry] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.OemTilde)),
            [GlobalKeybindActions.ResultsTab] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Tab)),
            [GlobalKeybindActions.DialogConfirm] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Enter)),
            [GlobalKeybindActions.DialogCancel] = new KeybindList(new Keybind(KeyModifiers.Free, Keys.Escape)),
        };
#pragma warning restore format // @formatter:on
    }
}
