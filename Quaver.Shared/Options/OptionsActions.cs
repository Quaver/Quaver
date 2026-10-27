using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Profiles;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Dialogs;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Online.API.Offsets;
using Quaver.Shared.Online.API.Ranked;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;
using Wobble.Platform;

namespace Quaver.Shared.Options
{
    /// <summary>
    /// Behaviors shared by the legacy and V2 options buttons
    /// </summary>
    internal static class OptionsActions
    {
        private static bool RankedUpdateRunning { get; set; }
        private static bool OffsetUpdateRunning { get; set; }

        public static void SetCustomFps(Action<bool> setDialogFocused = null, Action onChanged = null)
        {
            setDialogFocused?.Invoke(true);
            DialogManager.Show(new YesNoTextDialog("Custom FPS", "Enter a custom FPS value.", ConfigManager.CustomFpsLimit.Value.ToString(), "", text =>
                {
                    if (!int.TryParse(text, out var fps))
                    {
                        NotificationManager.Show(NotificationLevel.Error, "Please enter a valid FPS value.");
                        return;
                    }

                    var previous = ConfigManager.CustomFpsLimit.Value;
                    ConfigManager.CustomFpsLimit.Value = fps;
                    if (ConfigManager.CustomFpsLimit.Value != previous)
                        onChanged?.Invoke();
                    NotificationManager.Show(NotificationLevel.Success, $"Custom FPS set to {ConfigManager.CustomFpsLimit.Value}.");
                    setDialogFocused?.Invoke(false);

                }, () => setDialogFocused?.Invoke(false)));
        }

        public static void CalibrateOffset()
        {
            if (!(GameBase.Game is QuaverGame game))
                return;

            if (game.CurrentScreen?.Type != QuaverScreenType.Menu && game.CurrentScreen?.Type != QuaverScreenType.Select)
            {
                NotificationManager.Show(NotificationLevel.Warning, "Finish what you're doing before calibrating a new offset");
                return;
            }

            const string path = "Quaver.Resources/Maps/Offset/offset.qua";
            var qua = Qua.Parse(GameBase.Game.Resources.Get(path));

            if (AudioEngine.Track != null && !AudioEngine.Track.IsDisposed && AudioEngine.Track.IsPlaying)
                AudioEngine.Track.Pause();

            game.CurrentScreen.Exit(() =>
            {
                MapManager.Selected.Value = Map.FromQua(qua, path, true);
                MapManager.Selected.Value.Qua = qua;
                ModManager.RemoveAllMods();
                ModManager.AddMod(ModIdentifier.NoFail);
                BackgroundHelper.Load(MapManager.Selected.Value);
                DialogManager.Dismiss(DialogManager.Dialogs.Last());
                return new GameplayScreen(qua, "", new List<Score>(), null, false, 0, true);
            });
        }

        public static void OpenSkinFolder()
        {
            if (ConfigManager.SkinDirectory == null)
                return;

            if (string.IsNullOrEmpty(ConfigManager.Skin.Value))
            {
                NotificationManager.Show(NotificationLevel.Warning, "You currently do not have a skin selected!");
                return;
            }

            var dir = ConfigManager.UseSteamWorkshopSkin.Value ? $"{ConfigManager.SteamWorkshopDirectory.Value}/{ConfigManager.Skin.Value}" : $"{ConfigManager.SkinDirectory.Value}/{ConfigManager.Skin.Value}";
            if (!Directory.Exists(dir))
            {
                NotificationManager.Show(NotificationLevel.Warning, "Your skin folder does not exist!");
                return;
            }

            Utils.NativeUtils.OpenNatively(dir);
        }

        public static void GenerateSkinConfig(Action<bool> setDialogFocused = null)
        {
            var store = SkinManager.SkinV2;
            if (store == null)
            {
                ShowSkinConfigUnavailable();
                return;
            }
            if (!File.Exists(store.ConfigPath))
            {
                GenerateSkinConfigFile();
                return;
            }

            setDialogFocused?.Invoke(true);
            DialogManager.Show(new YesNoDialog("Overwrite skin.yml?", "This will keep only required metadata and properties marked ConfigEditable.", () =>
                {
                    setDialogFocused?.Invoke(false);
                    GenerateSkinConfigFile();
                },
                () => setDialogFocused?.Invoke(false)));
        }

        private static void GenerateSkinConfigFile()
        {
            var store = SkinManager.SkinV2;
            if (store == null)
            {
                ShowSkinConfigUnavailable();
                return;
            }
            if (store.TrySaveEditableConfig(out var errors))
            {
                NotificationManager.Show(NotificationLevel.Success, "Generated skin.yml with the skin-author editable properties.");
                return;
            }

            var message = errors.Count == 0 ? "The Skin V2 configuration could not be generated." : string.Join(" ", errors.Take(3));
            NotificationManager.Show(NotificationLevel.Error, message);
        }

        private static void ShowSkinConfigUnavailable() => NotificationManager.Show(NotificationLevel.Error, "The selected skin's V2 configuration is not loaded.");

        public static void ExportSkin() => SkinManager.Export();

        public static void UploadSkin()
        {
            if (string.IsNullOrEmpty(ConfigManager.Skin.Value) || ConfigManager.Skin.Value == "Default Skin")
            {
                NotificationManager.Show(NotificationLevel.Warning, "You currently do not have a selected custom skin!");
                return;
            }

            var skin = new SteamWorkshopItem(ConfigManager.Skin.Value, SkinManager.Skin.Dir.Replace("\\", "/"));
            if (!skin.HasUploaded)
                DialogManager.Show(new UploadWorkshopSkinDialog(skin));
        }

        public static void OpenGameFolder()
        {
            var dir = ConfigManager.GameDirectory.Value;
            if (!Directory.Exists(dir))
            {
                NotificationManager.Show(NotificationLevel.Warning, "That folder does not exist!");
                return;
            }

            Utils.NativeUtils.OpenNatively(dir);
        }

        public static void DetectOtherGames(Action onChanged = null)
        {
            var previousOsuDbPath = ConfigManager.OsuDbPath.Value;
            var previousEtternaDbPath = ConfigManager.EtternaDbPath.Value;
            var previousAutoLoad = ConfigManager.AutoLoadOsuBeatmaps.Value;

            OtherGameMapDatabaseCache.FindOsuStableInstallation();
            OtherGameMapDatabaseCache.FindEtternaInstallation();
            var count = 0;
            if (!string.IsNullOrEmpty(ConfigManager.OsuDbPath.Value)) count++;
            if (!string.IsNullOrEmpty(ConfigManager.EtternaDbPath.Value)) count++;

            if (count > 0)
            {
                var message = $"Detected song databases for {count} other installed game{(count > 1 ? "s" : "")}.";
                NotificationManager.Show(NotificationLevel.Success, message);
                Logger.Important(message, LogType.Runtime);
                ConfigManager.AutoLoadOsuBeatmaps.Value = true;
            }
            else
            {
                const string message = "Could not find song databases for other installed games";
                NotificationManager.Show(NotificationLevel.Warning, message);
                Logger.Important(message, LogType.Runtime);
            }

            if (ConfigManager.OsuDbPath.Value != previousOsuDbPath ||
                ConfigManager.EtternaDbPath.Value != previousEtternaDbPath ||
                ConfigManager.AutoLoadOsuBeatmaps.Value != previousAutoLoad)
                onChanged?.Invoke();
        }

        public static void UpdateRankedStatuses(bool fromOptions = true)
        {
            if (fromOptions && RankedUpdateRunning)
            {
                NotificationManager.Show(NotificationLevel.Warning, "Your maps are already being updated! Please wait until it has completed!");
                return;
            }
            if (fromOptions && MapManager.Mapsets.Count == 0)
            {
                NotificationManager.Show(NotificationLevel.Warning, "You do not have any maps loaded!");
                return;
            }
            if (!OnlineManager.Connected)
            {
                NotificationManager.Show(NotificationLevel.Warning, "Cannot update ranked statuses when not online.");
                return;
            }

            if (fromOptions)
                RankedUpdateRunning = true;
            NotificationManager.Show(NotificationLevel.Info, "Your maps' ranked statuses are now being updated in the background...");
            var mapsets = new List<Mapset>(MapManager.Mapsets);
            ThreadScheduler.Run(() =>
            {
                try
                {
                    var count = 0;
                    var response = new APIRequestRankedMapsets().ExecuteRequest();
                    var rankedMapsets = response.Mapsets.ToHashSet();
                    Logger.Important($"There are currently {response.Mapsets.Count} ranked mapsets to check.", LogType.Runtime);
                    foreach (var mapset in mapsets)
                    {
                        if (mapset.Maps.Count == 0 || mapset.Maps.First().Game != MapGame.Quaver)
                            continue;
                        foreach (var map in mapset.Maps)
                        {
                            if (map.MapId == -1 || !rankedMapsets.Contains(map.MapSetId) || map.RankedStatus == RankedStatus.Ranked)
                                continue;
                            map.RankedStatus = RankedStatus.Ranked;
                            MapDatabaseCache.UpdateMap(map);
                            count++;
                        }
                    }

                    NotificationManager.Show(NotificationLevel.Success, $"Successfully updated the ranked statuses of {count:n0} maps!");
                    Logger.Important($"Finished updating statuses of: {count} maps", LogType.Runtime);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, "There was an issue while updating your maps' ranked statuses");
                }
                finally
                {
                    if (fromOptions)
                        RankedUpdateRunning = false;
                }
            });
        }

        public static void UpdateOnlineOffsets()
        {
            if (OffsetUpdateRunning)
            {
                NotificationManager.Show(NotificationLevel.Warning, "Your maps are already being updated! Please wait until it has completed!");
                return;
            }
            if (MapManager.Mapsets.Count == 0)
            {
                NotificationManager.Show(NotificationLevel.Warning, "You do not have any maps loaded!");
                return;
            }

            OffsetUpdateRunning = true;
            NotificationManager.Show(NotificationLevel.Info, "Your maps' online offsets are now being updated in the background...");
            var mapsets = new List<Mapset>(MapManager.Mapsets);
            ThreadScheduler.Run(() =>
            {
                try
                {
                    var count = 0;
                    var response = new APIRequestOnlineOffsets().ExecuteRequest();
                    var offsets = response.Maps.ToDictionary(x => x.Id, x => x.Offset);
                    Logger.Important($"There are currently {offsets.Count} maps to check.", LogType.Runtime);
                    foreach (var mapset in mapsets)
                    {
                        if (mapset.Maps.Count == 0 || mapset.Maps.First().Game != MapGame.Quaver)
                            continue;
                        foreach (var map in mapset.Maps)
                        {
                            if (map.MapId == -1 || !offsets.TryGetValue(map.MapId, out var offset))
                                continue;
                            map.OnlineOffset = offset;
                            MapDatabaseCache.UpdateMap(map);
                            count++;
                        }
                    }

                    NotificationManager.Show(NotificationLevel.Success, $"Successfully updated the online offsets of {count:n0} maps!");
                    Logger.Important($"Finished updating offsets of: {count} maps", LogType.Runtime);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, "There was an issue while updating your maps' offsets.");
                }
                finally
                {
                    OffsetUpdateRunning = false;
                }
            });
        }

        public static void SuggestDifficulty(Action onChanged = null)
        {
            var changed = false;
            foreach (GameMode mode in ModeHelper.AllModes)
            {
                var profile = UserProfileDatabaseCache.Selected.Value;
                profile.PopulateStats();
                var rating = profile.Stats[mode].OverallRating;
                var previous = ConfigManager.PrioritizedMapDifficulty[mode].Value;
                ConfigManager.PrioritizedMapDifficulty[mode].Value = (int)(rating / 20f * 10);
                changed |= ConfigManager.PrioritizedMapDifficulty[mode].Value != previous;
            }
            if (changed)
                onChanged?.Invoke();
            NotificationManager.Show(NotificationLevel.Info, "Suggested difficulties have been recalculated.");
        }
    }
}
