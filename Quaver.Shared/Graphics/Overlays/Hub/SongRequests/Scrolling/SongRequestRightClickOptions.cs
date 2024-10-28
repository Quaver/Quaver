using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Helpers;
using Quaver.Server.Client.Objects.Twitch;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Importing;
using Quaver.Shared.Screens.Selection;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Graphics.Overlays.Hub.SongRequests.Scrolling
{
    public class SongRequestRightClickOptions : RightClickOptions
    {
        /// <summary>
        /// </summary>
        private SongRequestScrollContainer Container { get; }

        /// <summary>
        /// </summary>
        private SongRequest Request { get; }

        private const string Play = "Play";

        private const string RequesterProfile = "Requester Profile";

        private const string OnlineListing = "Online Listing";

        private const string Delete = "Delete";

        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="request"></param>
        public SongRequestRightClickOptions(SongRequestScrollContainer container, SongRequest request) : base(new Dictionary<string, Color>()
        {
            {Play, Color.White},
            {RequesterProfile, ColorHelper.HexToColor("#0787E3")},
            {OnlineListing, ColorHelper.HexToColor("#9B51E0")},
            {Delete, ColorHelper.HexToColor($"#FF6868")}
        }, new ScalableVector2(200, 40), 22)
        {
            Container = container;
            Request = request;

            ItemSelected += (sender, args) =>
            {
                switch (args.Text)
                {
                    case Play:
                        PlayRequest();
                        break;
                    case RequesterProfile:
                        VisitRequesterProfile();
                        break;
                    case OnlineListing:
                        VisitOnlineListing();
                        break;
                    case Delete:
                        Container.Remove(Request);
                        break;
                }
            };
        }

        /// <summary>
        /// </summary>
        private void VisitRequesterProfile() => BrowserHelper.OpenURL($"https://twitch.tv/{Request.TwitchUsername}");

        /// <summary>
        /// </summary>
        private void VisitOnlineListing()
        {
            switch ((MapGame) Request.Game)
            {
                case MapGame.Quaver:
                    BrowserHelper.OpenURL($"https://quavergame.com/mapsets/{Request.MapsetId}");
                    break;
                case MapGame.Osu:
                    BrowserHelper.OpenURL($"https://osu.ppy.sh/beatmapsets/{Request.MapsetId}");
                    break;
            }
        }

        /// <summary>
        /// </summary>
        private void PlayRequest()
        {
            // First check if we're in song select
            var game = (QuaverGame) GameBase.Game;

            if (game.CurrentScreen.Type != QuaverScreenType.Select)
            {
                NotificationManager.Show(NotificationLevel.Warning, "You must be in the song select screen to play song requests!");
                return;
            }

            var item = Container.Pool.Find(x => x.Item == Request) as DrawableSongRequest;
            item?.MarkAsPlayed();

            // Check if we have the map installed
            if (!string.IsNullOrEmpty(Request.MapMd5))
            {
                var map = MapManager.FindMapFromMd5(Request.MapMd5);

                if (map != null)
                {
                    MapManager.PlaySongRequest(Request, map);
                    return;
                }
            }

            switch ((MapGame) Request.Game)
            {
                case MapGame.Quaver:
                    // Try to find the mapset by its mapset first.
                    if (MapManager.Mapsets.Count != 0)
                    {
                        var mapset = MapManager.Mapsets.Find(x => x.Maps.First().Game == MapGame.Quaver
                                                                  && x.Maps.First().MapSetId == Request.MapsetId);

                        if (mapset != null)
                        {
                            MapManager.PlaySongRequest(Request, mapset.Maps.First());
                            return;
                        }
                    }

                    // User doesn't have the map, so download it for them
                    if (MapsetDownloadManager.CurrentDownloads.All(x => x.MapsetId != Request.MapsetId))
                    {
                        var download = MapsetDownloadManager.Download(Request.MapsetId, Request.Artist, Request.Title);
                        game.OnlineHub.SelectSection(OnlineHubSectionType.ActiveDownloads);

                        // Auto import
                        download.Status.ValueChanged += (sender, args) =>
                        {
                            if (args.Value.Status != FileDownloaderStatus.Complete)
                                return;
                            if (game.CurrentScreen.Type == QuaverScreenType.Select)
                            {
                                var selectScreen = (SelectionScreen) game.CurrentScreen;
                                game.CurrentScreen.Exit(() => new ImportingScreen(null, true));

                                var dialog = DialogManager.Dialogs.Find(x => x is OnlineHubDialog) as OnlineHubDialog;
                                dialog?.Close();
                            }
                        };
                    }
                    break;
                // If an osu! map, send them directly to the download page
                case MapGame.Osu:
                    BrowserHelper.OpenURL($"https://osu.ppy.sh/beatmapsets/{Request.MapsetId}", true);
                    break;
            }
        }
    }
}