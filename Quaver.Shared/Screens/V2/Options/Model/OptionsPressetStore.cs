using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Quaver.Shared.Config;
using Quaver.Shared.Input;
using Wobble.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Quaver.Shared.Screens.V2.Options.Model
{
    internal static class OptionsPressetStore
    {
        private const int CurrentVersion = 1;

        private static readonly ISerializer Serializer = new SerializerBuilder()
            .WithNamingConvention(new CamelCaseNamingConvention())
            .WithEventEmitter(next => new KeybindListYamlFlowStyle(next))
            .WithTypeConverter(new KeybindYamlTypeConverter())
            .DisableAliases()
            .Build();

        private static readonly IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(new CamelCaseNamingConvention())
            .WithTypeConverter(new KeybindYamlTypeConverter())
            .IgnoreUnmatchedProperties()
            .Build();

        private static string PresetsDirectory
        {
            get
            {
                var gameDirectory = ConfigManager.GameDirectory?.Value;
                if (string.IsNullOrWhiteSpace(gameDirectory))
                    throw new InvalidOperationException("The game directory is not initialized.");

                return Path.Combine(gameDirectory, "Pressets");
            }
        }

        public static void Save(OptionsPreset preset)
        {
            if (preset == null)
                throw new ArgumentNullException(nameof(preset));
            if (!IsValid(preset))
                throw new ArgumentException("The preset is missing a valid ID, name, or values.", nameof(preset));

            var yaml = Serializer.Serialize(preset);
            var directory = PresetsDirectory;
            Directory.CreateDirectory(directory);

            var destination = Path.Combine(directory, $"{preset.Id:N}.yaml");
            var temporary = Path.Combine(directory, $"{preset.Id:N}.{Guid.NewGuid():N}.tmp");
            try
            {
                File.WriteAllText(temporary, yaml, new UTF8Encoding(false));
                File.Move(temporary, destination, true);
            }
            finally
            {
                if (File.Exists(temporary))
                    File.Delete(temporary);
            }
        }

        public static void Delete(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("A preset ID is required.", nameof(id));

            var path = Path.Combine(PresetsDirectory, $"{id:N}.yaml");
            if (!File.Exists(path))
                throw new FileNotFoundException("The preset file no longer exists.", path);

            var deletedDirectory = Path.Combine(PresetsDirectory, "Deleted",
                $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}");
            Directory.CreateDirectory(deletedDirectory);
            var destination = Path.Combine(deletedDirectory, $"{id:N}.yaml");
            File.Move(path, destination);
        }

        public static IReadOnlyList<OptionsPreset> LoadAll()
        {
            string[] paths;
            try
            {
                var directory = PresetsDirectory;
                if (!Directory.Exists(directory))
                    return Array.Empty<OptionsPreset>();

                paths = Directory.GetFiles(directory, "*.yaml", SearchOption.TopDirectoryOnly);
            }
            catch (Exception e)
            {
                Logger.Error($"Could not list options presets: {e}", LogType.Runtime);
                return Array.Empty<OptionsPreset>();
            }

            var presets = new List<OptionsPreset>();
            var loadedIds = new HashSet<Guid>();
            foreach (var path in paths)
            {
                try
                {
                    var preset = Deserializer.Deserialize<OptionsPreset>(File.ReadAllText(path));
                    if (!IsValid(preset) ||
                        !string.Equals(Path.GetFileNameWithoutExtension(path), preset.Id.ToString("N"), StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Error($"Ignoring invalid options preset: {path}", LogType.Runtime);
                        continue;
                    }

                    if (!loadedIds.Add(preset.Id))
                    {
                        Logger.Error($"Ignoring duplicate options preset: {path}", LogType.Runtime);
                        continue;
                    }

                    presets.Add(preset);
                }
                catch (Exception e)
                {
                    Logger.Error($"Could not load options preset {path}: {e}", LogType.Runtime);
                }
            }

            return presets.OrderBy(preset => preset.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static bool IsValid(OptionsPreset preset) =>
            preset != null && preset.Version == CurrentVersion && preset.Id != Guid.Empty &&
            !string.IsNullOrWhiteSpace(preset.Name) && preset.Values != null &&
            preset.GlobalKeybinds != null && preset.KeyLayouts != null;
    }
}
