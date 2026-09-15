using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Quaver.API.Enums;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Profiles;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Localization;
using Wobble.Logging;
using Wobble.Managers;
using Wobble.Platform;
using ModeHelper = Quaver.API.Helpers.ModeHelper;

namespace Quaver.Shared.Screens.V2.Options.Catalog.Categories
{
    internal static class MiscellaneousOptions
    {
        private static readonly GameMode[] KeyCountOrder =
        {
            GameMode.Keys1, GameMode.Keys2, GameMode.Keys3, GameMode.Keys4, GameMode.Keys5,
            GameMode.Keys6, GameMode.Keys7, GameMode.Keys8, GameMode.Keys9, GameMode.Keys10
        };

        internal static OptionsRowGroup[] Create() => new[]
        {
            new OptionsRowGroup(OptionsCategoryId.Miscellaneous, "Screen_Options_Localization",
                CreateLanguageRow()),
            new OptionsRowGroup(OptionsCategoryId.Miscellaneous, "Screen_Options_NavigationMaintenance",
                OptionsRowDefinition.Button("Screen_Options_OpenGameFolder", "Screen_Options_OpenGameFolderButton",
                    OpenGameFolder),
                OptionsRowDefinition.Button("Screen_Options_UpdateMapRankedStatuses", "Screen_Options_UpdateButton",
                    MapMetadataUpdater.UpdateRankedStatuses),
                OptionsRowDefinition.Button("Screen_Options_UpdateMapOnlineOffsets", "Screen_Options_UpdateButton",
                    MapMetadataUpdater.UpdateOnlineOffsets)),
            new OptionsRowGroup(OptionsCategoryId.Miscellaneous, "Screen_Options_InstalledGames",
                OptionsRowDefinition.Toggle("Screen_Options_LoadSongsFromOtherInstalledGames",
                    ConfigManager.AutoLoadOsuBeatmaps),
                OptionsRowDefinition.Button("Screen_Options_DetectSongsFromOtherInstalledGames",
                    "Screen_Options_DetectButton", DetectOtherGames)),
            new OptionsRowGroup(OptionsCategoryId.Miscellaneous, "Screen_Options_Notifications",
                OptionsRowDefinition.Toggle("Screen_Options_DisplayNotificationsFromBottomToTop",
                    ConfigManager.DisplayNotificationsBottomToTop),
                OptionsRowDefinition.Toggle("Screen_Options_DisplayOnlineFriendNotifications",
                    ConfigManager.DisplayFriendOnlineNotifications)),
            new OptionsRowGroup(OptionsCategoryId.Miscellaneous, "Screen_Options_SongSelect",
                CreateSongSelectRows())
        };

        /// <summary>
        ///     Changing the language is handled where the language setting is watched.
        /// </summary>
        private static OptionsRowDefinition CreateLanguageRow()
        {
            var languages = QuaverLocalization.AvailableLanguages;
            var options = languages.Select(x => LocalizationManager.Get(x.DisplayNameKey)).ToList();

            int GetIndex()
            {
                var cultureName = QuaverLocalization.GetLanguage(ConfigManager.Language.Value).CultureName;

                for (var i = 0; i < languages.Count; i++)
                {
                    if (languages[i].CultureName == cultureName)
                        return i;
                }

                return 0;
            }

            bool OnChanged(string value)
            {
                var index = options.IndexOf(value);
                if (index < 0)
                    return false;

                ConfigManager.Language.Value = languages[index].CultureName;
                return true;
            }

            return OptionsRowDefinition.Dropdown("Screen_Options_Language", options, GetIndex, OnChanged);
        }

        private static OptionsRowDefinition[] CreateSongSelectRows()
        {
            var rows = new List<OptionsRowDefinition>
            {
                CreatePrioritizedGameModeRow(),
                OptionsRowDefinition.Button("Screen_Options_SuggestDifficultyfromOverallRating",
                    "Screen_Options_CalibrateButton", SuggestDifficulty)
            };

            foreach (var mode in KeyCountOrder)
            {
                rows.Add(OptionsRowDefinition.Slider("Screen_Options_PrioritizedDifficulty",
                    ConfigManager.PrioritizedMapDifficulty[mode],
                    value => (value / 10f).ToString("0.0", CultureInfo.InvariantCulture),
                    new object[] { ModeHelper.ToShortHand(mode) }));
            }

            return rows.ToArray();
        }

        /// <summary>
        ///     Picks the mode by its position in the list, not by its enum number,
        ///     because the enum numbers are not in list order (4K = 1, 7K = 2, 1K = 3, ...).
        /// </summary>
        private static OptionsRowDefinition CreatePrioritizedGameModeRow()
        {
            var modes = ModeHelper.AllModes;
            var options = modes.Select(ModeHelper.ToLongHand).ToList();

            bool OnChanged(string value)
            {
                var index = options.IndexOf(value);
                if (index < 0)
                    return false;

                ConfigManager.PrioritizedGameMode.Value = modes[index];
                return true;
            }

            return OptionsRowDefinition.Dropdown("Screen_Options_PrioritizedGameMode", options,
                () => Math.Max(0, Array.IndexOf(modes, ConfigManager.PrioritizedGameMode.Value)), OnChanged);
        }

        /// <summary>
        ///     Sets each mode's prioritized difficulty from the player's overall rating in that mode.
        /// </summary>
        private static void SuggestDifficulty()
        {
            foreach (var mode in ModeHelper.AllModes)
            {
                var profile = UserProfileDatabaseCache.Selected.Value;
                profile.PopulateStats();

                var rating = profile.Stats[mode].OverallRating;
                ConfigManager.PrioritizedMapDifficulty[mode].Value = (int) (rating / 20f * 10);
            }

            NotificationManager.Show(NotificationLevel.Info, "Suggested difficulties have been recalculated.");
        }

        private static void OpenGameFolder()
        {
            var dir = ConfigManager.GameDirectory.Value;

            if (!Directory.Exists(dir))
            {
                NotificationManager.Show(NotificationLevel.Warning, "That folder does not exist!");
                return;
            }

            Utils.NativeUtils.OpenNatively(dir);
        }

        /// <summary>
        ///     Looks for osu! and Etterna song databases and turns on loading songs from them when found.
        /// </summary>
        private static void DetectOtherGames()
        {
            OtherGameMapDatabaseCache.FindOsuStableInstallation();
            OtherGameMapDatabaseCache.FindEtternaInstallation();

            var count = 0;

            if (!string.IsNullOrEmpty(ConfigManager.OsuDbPath.Value))
                count++;

            if (!string.IsNullOrEmpty(ConfigManager.EtternaDbPath.Value))
                count++;

            if (count == 0)
            {
                const string noneMessage = "Could not find song databases for other installed games";
                NotificationManager.Show(NotificationLevel.Warning, noneMessage);
                Logger.Important(noneMessage, LogType.Runtime);
                return;
            }

            var message = $"Detected song databases for {count} other installed game" + (count > 1 ? "s." : ".");
            NotificationManager.Show(NotificationLevel.Success, message);
            Logger.Important(message, LogType.Runtime);
            ConfigManager.AutoLoadOsuBeatmaps.Value = true;
        }
    }
}
