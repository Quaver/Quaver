using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Wobble.Configuration;
using Wobble.Logging;

namespace Quaver.Shared.Config
{
    /// <summary>
    ///     Root of the versioned quaver.yml document. Game settings remain in quaver.cfg until the
    ///     Options V2 settings model is introduced.
    /// </summary>
    public sealed class QuaverYamlConfig
    {
        [ConfigRequired]
        [Range(1, 1)]
        public int FormatVersion { get; set; } = 1;

        [ConfigEditable]
        [Required]
        public string ActivePresetId { get; set; } = QuaverPresetCatalog.GraphicsId;

        [ConfigEditable]
        [Required]
        public List<QuaverUserPresetConfig> UserPresets { get; set; } = new List<QuaverUserPresetConfig>();
    }

    public sealed class QuaverUserPresetConfig
    {
        [Required]
        public string Id { get; set; } = "";

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public QuaverPresetOptionsConfig Options { get; set; } = new QuaverPresetOptionsConfig();
    }

    /// <summary>
    ///     Typed expansion point for the option snapshot added in the next Options V2 stage.
    /// </summary>
    public sealed class QuaverPresetOptionsConfig
    {
    }

    internal sealed class QuaverPresetDescriptor
    {
        internal string Id { get; }

        internal string NameOrLocalizationKey { get; }

        internal bool IsBuiltIn { get; }

        internal QuaverPresetDescriptor(string id, string nameOrLocalizationKey, bool isBuiltIn)
        {
            Id = id;
            NameOrLocalizationKey = nameOrLocalizationKey;
            IsBuiltIn = isBuiltIn;
        }
    }

    internal static class QuaverPresetCatalog
    {
        internal const string PerformanceId = "performance";

        internal const string GraphicsId = "graphics";

        internal static IReadOnlyList<QuaverPresetDescriptor> BuiltIns { get; } =
            new ReadOnlyCollection<QuaverPresetDescriptor>(new[]
            {
                new QuaverPresetDescriptor(PerformanceId, "Screen_Options_PresetPerformance", true),
                new QuaverPresetDescriptor(GraphicsId, "Screen_Options_PresetGraphics", true)
            });

        internal static bool IsBuiltIn(string id) =>
            BuiltIns.Any(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));

        internal static string CanonicalizeBuiltIn(string id)
        {
            var preset = BuiltIns.FirstOrDefault(x =>
                string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
            return preset?.Id;
        }
    }

    /// <summary>
    ///     Loads and atomically writes the preset-ready quaver.yml document.
    /// </summary>
    public static class QuaverYamlConfigManager
    {
        private static YamlConfig<QuaverYamlConfig> Source { get; set; }

        private static QuaverYamlConfig Current { get; set; } = new QuaverYamlConfig();

        internal static string ConfigPath { get; private set; }

        internal static string ActivePresetId => Current.ActivePresetId;

        public static void Initialize() => Initialize(Directory.GetCurrentDirectory());

        internal static void Initialize(string gameDirectory)
        {
            if (string.IsNullOrWhiteSpace(gameDirectory))
                throw new ArgumentException("A game directory is required.", nameof(gameDirectory));

            ConfigPath = Path.Combine(Path.GetFullPath(gameDirectory), "quaver.yml");
            var existed = File.Exists(ConfigPath);
            Source = YamlConfig<QuaverYamlConfig>.LoadOptional(ConfigPath);

            if (!existed)
            {
                Current = new QuaverYamlConfig();
                if (!TryWrite(Current, out var createErrors))
                    LogErrors("quaver.yml could not be created", createErrors);
                return;
            }

            if (!Source.Reload())
            {
                RecoverInvalidFile(Source.Warnings);
                return;
            }

            var loaded = Source.GetSnapshot();
            if (!TryValidateUserPresets(loaded, out var validationErrors))
            {
                RecoverInvalidFile(validationErrors);
                return;
            }

            LogWarnings(Source.Warnings);
            Current = loaded;
            NormalizeActivePreset();
        }

        internal static IReadOnlyList<QuaverPresetDescriptor> GetPresets()
        {
            var presets = new List<QuaverPresetDescriptor>(QuaverPresetCatalog.BuiltIns);
            presets.AddRange(Current.UserPresets.Select(x =>
                new QuaverPresetDescriptor(x.Id, x.Name, false)));
            return presets.AsReadOnly();
        }

        internal static bool TrySelectPreset(string presetId, out IReadOnlyList<string> errors)
        {
            var canonicalId = ResolvePresetId(presetId);
            if (canonicalId == null)
            {
                errors = new[] { $"Unknown preset id '{presetId}'." };
                return false;
            }

            if (string.Equals(Current.ActivePresetId, canonicalId, StringComparison.Ordinal))
            {
                errors = Array.Empty<string>();
                return true;
            }

            var edited = Source.GetSnapshot();
            edited.ActivePresetId = canonicalId;
            if (!TryWrite(edited, out errors))
                return false;

            Current = Source.GetSnapshot();
            return true;
        }

        private static bool TryWrite(QuaverYamlConfig config, out IReadOnlyList<string> errors)
        {
            if (!TryValidateUserPresets(config, out errors))
                return false;

            return Source.TrySaveMain(config, out errors);
        }

        private static bool TryValidateUserPresets(QuaverYamlConfig config,
            out IReadOnlyList<string> errors)
        {
            var results = new List<string>();
            if (config?.UserPresets == null)
                results.Add("UserPresets cannot be null.");
            else
            {
                var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var preset in config.UserPresets)
                {
                    if (preset == null)
                    {
                        results.Add("UserPresets cannot contain null entries.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(preset.Id))
                        results.Add("Every user preset must have a non-empty Id.");
                    else if (QuaverPresetCatalog.IsBuiltIn(preset.Id))
                        results.Add($"User preset id '{preset.Id}' is reserved by a built-in preset.");
                    else if (!ids.Add(preset.Id))
                        results.Add($"Duplicate user preset id '{preset.Id}'.");

                    if (string.IsNullOrWhiteSpace(preset.Name))
                        results.Add($"User preset '{preset.Id}' must have a non-empty Name.");
                    if (preset.Options == null)
                        results.Add($"User preset '{preset.Id}' must have an Options object.");
                }
            }

            errors = results.AsReadOnly();
            return results.Count == 0;
        }

        private static void NormalizeActivePreset()
        {
            var canonicalId = ResolvePresetId(Current.ActivePresetId);
            if (canonicalId != null)
            {
                if (!string.Equals(canonicalId, Current.ActivePresetId, StringComparison.Ordinal))
                {
                    Current.ActivePresetId = canonicalId;
                    TryWrite(Current, out _);
                }
                return;
            }

            Logger.Warning($"quaver.yml preset '{Current.ActivePresetId}' does not exist; " +
                           $"falling back to '{QuaverPresetCatalog.GraphicsId}'.", LogType.Runtime, false);
            Current.ActivePresetId = QuaverPresetCatalog.GraphicsId;
            if (!TryWrite(Current, out var errors))
                LogErrors("The quaver.yml active preset fallback could not be saved", errors);
        }

        private static string ResolvePresetId(string presetId)
        {
            if (string.IsNullOrWhiteSpace(presetId))
                return null;

            var builtIn = QuaverPresetCatalog.CanonicalizeBuiltIn(presetId);
            if (builtIn != null)
                return builtIn;

            return Current.UserPresets.FirstOrDefault(x =>
                string.Equals(x.Id, presetId, StringComparison.OrdinalIgnoreCase))?.Id;
        }

        private static void RecoverInvalidFile(IReadOnlyList<string> errors)
        {
            LogErrors("quaver.yml is invalid", errors);
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var directory = Path.GetDirectoryName(ConfigPath) ?? Directory.GetCurrentDirectory();
                    var backup = Path.Combine(directory,
                        $"quaver.corrupted.{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.yml");
                    File.Copy(ConfigPath, backup, false);
                    Logger.Warning($"The invalid quaver.yml file was backed up to '{backup}'.",
                        LogType.Runtime, false);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"The invalid quaver.yml file could not be backed up: {e}", LogType.Runtime);
            }

            Current = new QuaverYamlConfig();
            if (!TryWrite(Current, out var writeErrors))
                LogErrors("The default quaver.yml file could not be written", writeErrors);
        }

        private static void LogWarnings(IReadOnlyList<string> warnings)
        {
            foreach (var warning in warnings)
                Logger.Warning($"quaver.yml: {warning}", LogType.Runtime, false);
        }

        private static void LogErrors(string context, IReadOnlyList<string> errors)
        {
            if (errors == null || errors.Count == 0)
            {
                Logger.Error(context + ".", LogType.Runtime);
                return;
            }

            foreach (var error in errors)
                Logger.Error($"{context}: {error}", LogType.Runtime);
        }
    }
}
