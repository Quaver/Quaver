using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IniFileParser;
using IniFileParser.Model;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Transitions;
using Wobble;

namespace Quaver.Shared.Skinning
{
    internal static class SkinEditorManager
    {
        public const string DefaultSkinOption = "Default Skin";

        public static IReadOnlyList<SkinEditorProperty> EditableProperties { get; } = new List<SkinEditorProperty>
        {
            new("General", "General", "Name", "Name", "Skin display name", "Untitled Skin"),
            new("General", "General", "Author", "Author", "Creator name", "Unknown"),
            new("General", "General", "Version", "Version", "v1.0", "v1.0"),
            new("General", "General", "CenterCursor", "Center Cursor", "True or False", "False"),
            new("General", "General", "UseSkinBackgrounds", "Use Skin Backgrounds", "True or False", "False"),

            new("Main Menu", "MenuBorder", "ButtonTextColor", "Border Button Color", "R,G,B", "255,255,255"),
            new("Main Menu", "MenuBorder", "ButtonTextHoveredColor", "Border Hover Color", "R,G,B", "69,214,245"),
            new("Main Menu", "MenuBorder", "BackgroundLineColor", "Border Line Color", "R,G,B", "22,174,247"),
            new("Main Menu", "MenuBorder", "ForegroundLineColor", "Border Accent Color", "R,G,B", "255,255,255"),
            new("Main Menu", "MainMenu", "NavigationButtonTextColor", "Main Button Color", "R,G,B", "255,255,255"),
            new("Main Menu", "MainMenu", "NavigationQuitButtonTextColor", "Quit Button Color", "R,G,B", "249,100,93"),
            new("Main Menu", "MainMenu", "NavigationButtonHoveredAlpha", "Button Hover Alpha", "0.0 - 1.0", "0.35"),
            new("Main Menu", "MainMenu", "AudioVisualizerColor", "Visualizer Color", "R,G,B", "22,174,247"),
            new("Main Menu", "MainMenu", "AudioVisualizerOpacity", "Visualizer Opacity", "0.0 - 1.0", "0.85"),

            new("Song Select", "SongSelect", "DisplayMapBackground", "Display Map Background", "True or False", "True"),
            new("Song Select", "SongSelect", "MapBackgroundBrightness", "Background Brightness", "0 - 100", "15"),
            new("Song Select", "SongSelect", "MapsetPanelSongTitleColor", "Song Title Color", "R,G,B", "255,255,255"),
            new("Song Select", "SongSelect", "MapsetPanelSongArtistColor", "Song Artist Color", "R,G,B", "5,135,229"),
            new("Song Select", "SongSelect", "MapsetPanelCreatorColor", "Creator Color", "R,G,B", "255,255,255"),
            new("Song Select", "SongSelect", "MapsetPanelByColor", "Byline Color", "R,G,B", "117,117,117"),
            new("Song Select", "SongSelect", "MapsetPanelHoveringAlpha", "Mapset Hover Alpha", "0.0 - 1.0", "0.35"),
            new("Song Select", "SongSelect", "LeaderboardTitleColor", "Leaderboard Title Color", "R,G,B", "255,255,255"),
            new("Song Select", "SongSelect", "LeaderboardScoreRankColor", "Leaderboard Rank Color", "R,G,B", "255,255,255"),
            new("Song Select", "SongSelect", "LeaderboardScoreRatingColor", "Leaderboard Rating Color", "R,G,B", "233,183,54"),

            new("Results", "Results", "ResultsBackgroundType", "Background Type", "Header or Full", "Header"),
            new("Results", "Results", "ResultsBackgroundFilterAlpha", "Filter Alpha", "0.0 - 1.0", "0.35")
        };

        public static string GetCurrentLocalSkin()
        {
            var skin = ConfigManager.Skin?.Value;

            if (string.IsNullOrWhiteSpace(skin) || skin == DefaultSkinOption)
                return null;

            if (ConfigManager.UseSteamWorkshopSkin?.Value == true)
                return null;

            return Directory.Exists(GetSkinDirectory(skin)) ? skin : null;
        }

        public static string GetSuggestedFolderName()
        {
            var current = ConfigManager.Skin?.Value;

            if (string.IsNullOrWhiteSpace(current) || current == DefaultSkinOption)
                current = $"{ConfigManager.DefaultSkin?.Value.ToString() ?? DefaultSkins.Bar.ToString()} Skin";

            return MakeUniqueFolderName($"{current} Copy");
        }

        public static IReadOnlyList<string> GetEditableSkins()
        {
            if (ConfigManager.SkinDirectory == null || string.IsNullOrWhiteSpace(ConfigManager.SkinDirectory.Value) ||
                !Directory.Exists(ConfigManager.SkinDirectory.Value))
                return Array.Empty<string>();

            return Directory.GetDirectories(ConfigManager.SkinDirectory.Value)
                .Select(dir => new DirectoryInfo(dir).Name)
                .OrderBy(x => x)
                .ToList();
        }

        public static string MakeUniqueFolderNameForDisplay(string folderName) => MakeUniqueFolderName(folderName);

        public static Dictionary<SkinEditorProperty, string> LoadPropertyValues(string skin)
            => LoadPropertyValues(skin, EditableProperties);

        public static Dictionary<SkinEditorProperty, string> LoadPropertyValues(string skin,
            IEnumerable<SkinEditorProperty> properties)
        {
            var config = LoadConfigOrDefault(skin);
            var values = new Dictionary<SkinEditorProperty, string>();

            foreach (var property in properties)
                values[property] = ReadValue(config, property);

            return values;
        }

        public static string CreateEditableSkin(string folderName, IReadOnlyDictionary<SkinEditorProperty, string> values)
        {
            var folder = SanitizeFolderName(folderName);

            if (string.IsNullOrWhiteSpace(folder))
                throw new InvalidOperationException("Enter a valid skin folder name first.");

            var target = GetSkinDirectory(folder);

            EnsurePathInsideSkinDirectory(target);

            if (Directory.Exists(target))
                throw new InvalidOperationException("A skin with that folder name already exists.");

            var source = GetCurrentSkinSourceDirectory();

            if (!string.IsNullOrWhiteSpace(source) && Directory.Exists(source))
                CopyDirectory(source, target);
            else
                Directory.CreateDirectory(target);

            if (!File.Exists(GetSkinIniPath(folder)))
                SaveConfig(folder, CreateDefaultConfig());

            SaveProperties(folder, values);
            return folder;
        }

        public static void SaveProperties(string skin, IReadOnlyDictionary<SkinEditorProperty, string> values)
        {
            if (string.IsNullOrWhiteSpace(skin))
                throw new InvalidOperationException("Create or select a local skin before saving.");

            var directory = GetSkinDirectory(skin);

            EnsurePathInsideSkinDirectory(directory);
            Directory.CreateDirectory(directory);

            var config = LoadConfigOrDefault(skin);

            foreach (var value in values)
                SetValue(config, value.Key, value.Value);

            SaveConfig(skin, config);
        }

        public static void SelectAndReload(string skin)
        {
            if (string.IsNullOrWhiteSpace(skin))
                throw new InvalidOperationException("Create or select a local skin before reloading.");

            if (!Directory.Exists(GetSkinDirectory(skin)))
                throw new InvalidOperationException("The selected skin folder does not exist.");

            ConfigManager.UseSteamWorkshopSkin.Value = false;
            ConfigManager.Skin.Value = skin;
            SkinManager.NewQueuedSkin = skin;

            Transitioner.FadeIn();
            SkinManager.TimeSkinReloadRequested = GameBase.Game.TimeRunning;
        }

        public static string GetSkinDirectory(string skin) => Path.Combine(ConfigManager.SkinDirectory.Value, skin);

        public static string GetSkinIniPath(string skin) => Path.Combine(GetSkinDirectory(skin), "skin.ini");

        public static string SanitizeFolderName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
                return "";

            var invalidCharacters = Path.GetInvalidFileNameChars();
            var sanitized = new string(rawName.Select(c => invalidCharacters.Contains(c) ? '-' : c).ToArray());
            sanitized = Regex.Replace(sanitized, @"\s+", " ").Trim().Trim('.');

            return sanitized;
        }

        private static string MakeUniqueFolderName(string folderName)
        {
            var sanitized = SanitizeFolderName(folderName);

            if (string.IsNullOrWhiteSpace(sanitized))
                sanitized = "New Skin";

            var candidate = sanitized;
            var index = 2;

            while (Directory.Exists(GetSkinDirectory(candidate)))
                candidate = $"{sanitized} {index++}";

            return candidate;
        }

        private static IniData LoadConfigOrDefault(string skin)
        {
            if (!string.IsNullOrWhiteSpace(skin))
            {
                var path = GetSkinIniPath(skin);

                if (File.Exists(path))
                    return CreateParser().ReadFile(path, Encoding.UTF8);
            }

            return CreateDefaultConfig();
        }

        private static IniData CreateDefaultConfig()
        {
            var defaultSkin = ConfigManager.DefaultSkin?.Value.ToString() ?? DefaultSkins.Bar.ToString();

            try
            {
                using var stream = new StreamReader(GameBase.Game.Resources.GetStream(
                    $"Quaver.Resources/Textures/Skins/{defaultSkin}/skin.ini"));
                return CreateParser().ReadData(stream);
            }
            catch (Exception)
            {
                var data = new IniData();
                EnsureSection(data, "General");
                EnsureSection(data, "4K");
                EnsureSection(data, "7K");
                EnsureSection(data, "SHAREDK");
                SetValue(data, new SkinEditorProperty("General", "General", "Name", "Name", "", "Untitled Skin"), "Untitled Skin");
                SetValue(data, new SkinEditorProperty("General", "General", "Author", "Author", "", "Unknown"), "Unknown");
                SetValue(data, new SkinEditorProperty("General", "General", "Version", "Version", "", "v1.0"), "v1.0");
                SetDefaultSkin(data, "4K", defaultSkin);
                SetDefaultSkin(data, "7K", defaultSkin);
                SetDefaultSkin(data, "SHAREDK", defaultSkin);
                return data;
            }
        }

        private static string ReadValue(IniData config, SkinEditorProperty property)
        {
            if (!config.Sections.ContainsSection(property.Section))
                return property.DefaultValue;

            var section = config[property.Section];
            return section.ContainsKey(property.Key) ? section[property.Key] ?? property.DefaultValue : property.DefaultValue;
        }

        private static void SetValue(IniData config, SkinEditorProperty property, string value)
        {
            EnsureSection(config, property.Section);
            config[property.Section][property.Key] = value?.Trim() ?? "";
        }

        private static void SetDefaultSkin(IniData config, string section, string defaultSkin)
        {
            EnsureSection(config, section);
            config[section]["DefaultSkin"] = defaultSkin;
        }

        private static void EnsureSection(IniData config, string section)
        {
            if (!config.Sections.ContainsSection(section))
                config.Sections.AddSection(section);
        }

        private static void SaveConfig(string skin, IniData config)
        {
            var path = GetSkinIniPath(skin);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            CreateParser().WriteFile(path, config, Encoding.UTF8);
        }

        private static IniFileParser.IniFileParser CreateParser() =>
            new(new ConcatenateDuplicatedKeysIniDataParser());

        private static string GetCurrentSkinSourceDirectory()
        {
            var skin = SkinManager.Skin?.Skin;

            if (string.IsNullOrWhiteSpace(skin) || skin == DefaultSkinOption)
                return null;

            var directory = SkinManager.Skin?.Dir;
            return !string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory) ? directory : null;
        }

        private static void CopyDirectory(string source, string target)
        {
            var sourcePath = Path.GetFullPath(source).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var targetPath = Path.GetFullPath(target).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (sourcePath.Equals(targetPath, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Cannot create a skin from the same folder.");

            if (targetPath.StartsWith(sourcePath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Cannot create a skin inside the source skin folder.");

            Directory.CreateDirectory(targetPath);

            foreach (var directory in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(sourcePath, directory);
                Directory.CreateDirectory(Path.Combine(targetPath, relative));
            }

            foreach (var file in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(sourcePath, file);
                File.Copy(file, Path.Combine(targetPath, relative), true);
            }
        }

        private static void EnsurePathInsideSkinDirectory(string path)
        {
            var root = Path.GetFullPath(ConfigManager.SkinDirectory.Value)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var target = Path.GetFullPath(path);

            if (!target.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
                !target.Equals(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Skin paths must stay inside the configured skin directory.");
        }
    }
}
