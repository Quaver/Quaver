using System;
using System.Collections.Generic;
using System.Linq;
using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online.API.Offsets;
using Quaver.Shared.Online.API.Ranked;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemUpdateOnlineOffsets : OptionsItem
    {
        /// <summary>
        /// </summary>
        private IconButton Button { get; }

        /// <summary>
        ///     If the task is currently running
        /// </summary>
        private static bool IsRunning { get; set; }

        public OptionsItemUpdateOnlineOffsets(RectangleF containerRect, string name) : base(containerRect, name)
        {
            const float scale = 0.85f;

            Button = new IconButton(UserInterface.OptionsUpdateButton)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(215 * scale, 36 * scale),
                UsePreviousSpriteBatchOptions = true
            };

            Button.Clicked += (sender, args) =>
            {
                if (IsRunning)
                {
                    NotificationManager.Show(NotificationLevel.Warning, "Your maps are already being updated! " +
                                                                        "Please wait until it has completed!");
                    return;
                }

                if (MapManager.Mapsets.Count == 0)
                {
                    NotificationManager.Show(NotificationLevel.Warning, "You do not have any maps loaded!");
                    return;
                }

                IsRunning = true;

                NotificationManager.Show(NotificationLevel.Info, "Your maps' online offsets are now being updated in the background...");

                var mapsets = new List<Mapset>(MapManager.Mapsets);

                ThreadScheduler.Run(() =>
                {
                    try
                    {
                        var count = 0;

                        var response = new APIRequestOnlineOffsets().ExecuteRequest();
                        var mapDict = response.Maps.ToDictionary(x => x.Id, y => y.Offset);

                        Logger.Important($"There are currently {mapDict.Count} maps to check.", LogType.Runtime);

                        foreach (var mapset in mapsets)
                        {
                            if (mapset.Maps.Count == 0)
                                continue;

                            if (mapset.Maps.First().Game != MapGame.Quaver)
                                continue;

                            foreach (var map in mapset.Maps)
                            {
                                if (map.MapId == -1 || !mapDict.ContainsKey(map.MapId))
                                    continue;

                                map.OnlineOffset = mapDict[map.MapId];
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
                        IsRunning = false;
                    }
                });
            };
        }
    }
}