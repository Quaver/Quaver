using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Quaver.API.Maps;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorUploadingMapsetDialog : LoadingDialog
    {
        private static Dictionary<MapsetSubmissionStatusCode, string> StatusCodeMessages { get; } = new Dictionary<MapsetSubmissionStatusCode, string>()
        {
            {MapsetSubmissionStatusCode.ErrorInternalServer, "An internal server error has occurred. Please try again!"},
            {MapsetSubmissionStatusCode.ErrorUnauthorized, "Please re-log and try again!"},
            {MapsetSubmissionStatusCode.ErrorBanned, "You cannot upload a mapset while banned!"},
            {MapsetSubmissionStatusCode.ErrorNoMapsetSent, "No mapset was sent with the request!"},
            {MapsetSubmissionStatusCode.ErrorRequestTooLarge, "Your mapset is too large! The max file size is 50mb"},
            {MapsetSubmissionStatusCode.ErrorInvalidMapsetFileExtension, "File extension error. Please report this to a developer!"},
            {MapsetSubmissionStatusCode.ErrorMapsetNotZipFile, "Your mapset was not a zip file. Please report this to a developer!"},
            {MapsetSubmissionStatusCode.ErrorInvalidFileType, "Your mapset contains files of an invalid type!"},
            {MapsetSubmissionStatusCode.ErrorNoContainingQuaFiles, "Your mapset does not contain any .qua files!"},
            {MapsetSubmissionStatusCode.ErrorInvalidMapFile, "One or more of your map files are invalid! (No objects, timing points?)!"},
            {MapsetSubmissionStatusCode.ErrorInvalidMapMetadata, "One or more of your map files do not have completed metadata!"},
            {MapsetSubmissionStatusCode.ErrorUserNotCreator, "You are not the creator of this mapset!"},
            {MapsetSubmissionStatusCode.ErrorConflictingMapIds, "One or more of your maps have conflicting map ids!"},
            {MapsetSubmissionStatusCode.ErrorConflictingMapsetIds, "One or more of your maps have conflicting mapset ids!"},
            {MapsetSubmissionStatusCode.ErrorConflictingDifficultyNames, "One or more of your maps have the same difficulty name!"},
            {MapsetSubmissionStatusCode.ErrorNoExistingMapsetFound, "You're trying to update a mapset, but this mapset isn't uploaded online!"},
            {MapsetSubmissionStatusCode.ErrorAlreadyRanked, "You cannot update a mapset that is already ranked!"},
            {MapsetSubmissionStatusCode.ErrorContainsNonUploadedNotNewMapId, "One or more of youir maps contains a non-uploaded map id!"},
            {MapsetSubmissionStatusCode.SuccessUploaded, "Success! Your mapset has been uploaded!"},
            {MapsetSubmissionStatusCode.SuccessUpdated, "Success! Your mapset has been updated!"},
            {MapsetSubmissionStatusCode.ErrorExceededLimit, "You have exceeded the amount of maps you can upload at this time!"}
        };

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public EditorUploadingMapsetDialog(EditScreen screen) : base("UPLOADING MAPSET",
            "Please wait while your mapset is being uploaded...", () =>
        {
            Logger.Important($"Beginning to upload mapset...", LogType.Network);

            try
            {
                screen.Save(true);
                NotificationManager.Show(NotificationLevel.Success, "Your mapset has been successfully saved!");
                Logger.Important("Successfully saved the current map", LogType.Network);

                var path = MapManager.Selected.Value.Mapset.ExportToZip(false);
                var response = OnlineManager.Client.UploadMapset(path);
                
                Logger.Important(response.ToString(), LogType.Network);

                var folderPath = $"{ConfigManager.SongDirectory.Value}/{screen.Map.Directory}";

                // Successful upload
                if (response.Code == MapsetSubmissionStatusCode.SuccessUpdated || response.Code == MapsetSubmissionStatusCode.SuccessUploaded)
                {
                    var newMaps = new List<Map>();
                    Map sameDifficultyMap = null;

                    foreach (var map in response.Maps)
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

                    var uploadStr = response.Code == MapsetSubmissionStatusCode.SuccessUploaded ? "uploaded" : "updated";
                    NotificationManager.Show(NotificationLevel.Success, $"Your mapset has been successfully {uploadStr}!");

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

                Logger.Important($"Error uploading mapset: {response.Code} | {response.Status} | {StatusCodeMessages[response.Code]}", LogType.Network);
                NotificationManager.Show(NotificationLevel.Error, StatusCodeMessages[response.Code]);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Network);
                NotificationManager.Show(NotificationLevel.Error, "There was an issue while uploading your mapset");
            }
        })
        {
        }
    }
}
