using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quaver.API.Maps;
using Quaver.Server.Client.Structures;
using Quaver.Server.Client.Structures.v2;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Wobble.Logging;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorUploadingMapsetDialog : LoadingDialog
    {
        private static Dictionary<MapsetSubmissionStatusCode, string> StatusCodeMessages { get; } = new Dictionary<MapsetSubmissionStatusCode, string>()
        {
            {MapsetSubmissionStatusCode.ErrorInternalServer, LocalizationManager.Get("Screen_Editor_UploadErrorInternalServer")},
            {MapsetSubmissionStatusCode.ErrorUnauthorized, LocalizationManager.Get("Screen_Editor_UploadErrorUnauthorized")},
            {MapsetSubmissionStatusCode.ErrorBanned, LocalizationManager.Get("Screen_Editor_UploadErrorBanned")},
            {MapsetSubmissionStatusCode.ErrorNoMapsetSent, LocalizationManager.Get("Screen_Editor_UploadErrorNoMapsetSent")},
            {MapsetSubmissionStatusCode.ErrorRequestTooLarge, LocalizationManager.Get("Screen_Editor_UploadErrorRequestTooLarge")},
            {MapsetSubmissionStatusCode.ErrorInvalidMapsetFileExtension, LocalizationManager.Get("Screen_Editor_UploadErrorInvalidMapsetFileExtension")},
            {MapsetSubmissionStatusCode.ErrorMapsetNotZipFile, LocalizationManager.Get("Screen_Editor_UploadErrorMapsetNotZipFile")},
            {MapsetSubmissionStatusCode.ErrorInvalidFileType, LocalizationManager.Get("Screen_Editor_UploadErrorInvalidFileType")},
            {MapsetSubmissionStatusCode.ErrorNoContainingQuaFiles, LocalizationManager.Get("Screen_Editor_UploadErrorNoContainingQuaFiles")},
            {MapsetSubmissionStatusCode.ErrorInvalidMapFile, LocalizationManager.Get("Screen_Editor_UploadErrorInvalidMapFile")},
            {MapsetSubmissionStatusCode.ErrorInvalidMapMetadata, LocalizationManager.Get("Screen_Editor_UploadErrorInvalidMapMetadata")},
            {MapsetSubmissionStatusCode.ErrorUserNotCreator, LocalizationManager.Get("Screen_Editor_UploadErrorUserNotCreator")},
            {MapsetSubmissionStatusCode.ErrorConflictingMapIds, LocalizationManager.Get("Screen_Editor_UploadErrorConflictingMapIds")},
            {MapsetSubmissionStatusCode.ErrorConflictingMapsetIds, LocalizationManager.Get("Screen_Editor_UploadErrorConflictingMapsetIds")},
            {MapsetSubmissionStatusCode.ErrorConflictingDifficultyNames, LocalizationManager.Get("Screen_Editor_UploadErrorConflictingDifficultyNames")},
            {MapsetSubmissionStatusCode.ErrorNoExistingMapsetFound, LocalizationManager.Get("Screen_Editor_UploadErrorNoExistingMapsetFound")},
            {MapsetSubmissionStatusCode.ErrorAlreadyRanked, LocalizationManager.Get("Screen_Editor_UploadErrorAlreadyRanked")},
            {MapsetSubmissionStatusCode.ErrorContainsNonUploadedNotNewMapId, LocalizationManager.Get("Screen_Editor_UploadErrorContainsNonUploadedNotNewMapId")},
            {MapsetSubmissionStatusCode.SuccessUploaded, LocalizationManager.Get("Screen_Editor_UploadSuccessUploaded")},
            {MapsetSubmissionStatusCode.SuccessUpdated, LocalizationManager.Get("Screen_Editor_UploadSuccessUpdated")},
            {MapsetSubmissionStatusCode.ErrorExceededLimit, LocalizationManager.Get("Screen_Editor_UploadErrorExceededLimit")}
        };

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public EditorUploadingMapsetDialog(EditScreen screen) : base(
            LocalizationManager.Get("Screen_Editor_UploadingMapset"),
            LocalizationManager.Get("Screen_Editor_UploadingMapsetMessage"), () =>
        {
            Logger.Important($"Beginning to upload mapset...", LogType.Network);

            try
            {
                screen.Save(true);
                NotificationManager.Show(NotificationLevel.Success,
                    LocalizationManager.Get("Screen_Editor_MapsetSavedSuccessfully"));
                Logger.Important("Successfully saved the current map", LogType.Network);

                var path = MapManager.Selected.Value.Mapset.ExportToZip(false);
                var response = OnlineManager.Client.UploadMapsetV2(path);

                Logger.Important(response.ToString(), LogType.Network);

                var folderPath = $"{ConfigManager.SongDirectory.Value}/{screen.Map.Directory}";

                var json = JObject.Parse(response.Content);
                var responseParsed = JsonConvert.DeserializeObject<V2UploadMapsetResponse>(json.ToString());

                // Successful upload
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var newMaps = new List<Map>();
                    Map sameDifficultyMap = null;

                    foreach (var map in responseParsed.Mapset.Maps)
                    {
                        if (map == null)
                            continue;

                        var filePath = $"{folderPath}/{map.Id}.qua";

                        try
                        {
                            Logger.Important($"Commencing download for map: {map.Id}", LogType.Network);
                            OnlineManager.Client.DownloadMap(filePath, map.Id);
                            Logger.Important($"Successfully downloaded map: {map.Id}", LogType.Network);

                            var databaseMap = Map.FromQua(Qua.Parse(filePath, false), filePath);
                            newMaps.Add(databaseMap);

                            // Make sure map gets added to the db upon the next time going to select
                            if (!MapDatabaseCache.MapsToUpdate.Contains(databaseMap))
                                MapDatabaseCache.MapsToUpdate.Add(databaseMap);

                            // The map the editor will be reloaded with
                            if (databaseMap.DifficultyName == screen.WorkingMap.DifficultyName)
                                sameDifficultyMap = databaseMap;
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e, LogType.Network);
                        }
                    }

                    var mapset = screen.Map.Mapset;

                    // Remove all of the current mapsets inside of the DB/Mapset
                    foreach (var map in mapset.Maps)
                        MapDatabaseCache.RemoveMap(map);

                    mapset.Maps.Clear();

                    // Make sure the new mapset is set for the maps
                    foreach (var map in newMaps)
                    {
                        mapset.Maps.Add(map);
                        map.Mapset = mapset;
                    }

                    MapDatabaseCache.ForceUpdateMaps(false);

                    mapset.Maps = mapset.Maps.OrderBy(x => x.Difficulty10X).ToList();

                    // Delete old .qua files
                    foreach (var file in Directory.GetFiles(folderPath, "*.qua"))
                    {
                        var name = Path.GetFileNameWithoutExtension(file);

                        // .qua files will always be the id of the file
                        if (!int.TryParse(name, out _))
                            File.Delete(file);
                    }

                    NotificationManager.Show(NotificationLevel.Success,
                        LocalizationManager.Get("Screen_Editor_MapsetUploadedSuccessfully"));

                    // If for some reason the map with the same difficulty
                    if (sameDifficultyMap == null)
                        sameDifficultyMap = mapset.Maps.First();

                    var track = AudioEngine.LoadMapAudioTrack(sameDifficultyMap);

                    // Reload the editor
                    screen.Exit(() =>
                    {
                        MapManager.Selected.Value = sameDifficultyMap;
                        return new EditScreen(sameDifficultyMap, track);
                    });

                    return;
                }

                Logger.Important($"Error uploading mapset: {response.ResponseStatus} | {responseParsed.Error}", LogType.Network);
                NotificationManager.Show(NotificationLevel.Error,
                    LocalizationManager.Get("Screen_Editor_UploadMapsetErrorWithReason", responseParsed.Error));
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Network);
                NotificationManager.Show(NotificationLevel.Error,
                    LocalizationManager.Get("Screen_Editor_UploadMapsetError"));
            }
        })
        {
        }
    }
}
