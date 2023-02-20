using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Quaver.API.Replays;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Loading;
using Quaver.Shared.Screens.Results;
using Quaver.Shared.Screens.Selection.UI.Leaderboard.Dialogs;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Platform;

namespace Quaver.Shared.Screens.Selection.UI.Leaderboard.Components
{
    public class LeaderboardScoreRightClickOptions : RightClickOptions
    {
        /// <summary>
        /// </summary>
        private Score Score { get; }

        private const string ViewResults = "View Results";

        private const string WatchReplay = "Watch Replay";

        private const string DownloadReplay = "Download Replay";

        private const string ExportReplay = "Export Replay";

        private const string PlayerProfile = "Player Profile";

        private const string SteamProfile = "Steam Profile";

        private const string AddFriend = "Add Friend";

        private const string RemoveFriend = "Remove Friend";

        private const string Delete = "Delete";

        /// <summary>
        /// </summary>
        public LeaderboardScoreRightClickOptions(Score score) : base(GetOptions(score), new ScalableVector2(200, 40), 22)
        {
            Score = score;

            ItemSelected += (sender, args) =>
            {
                var game = (QuaverGame) GameBase.Game;
                var replayPath = $"{ConfigManager.DataDirectory.Value}/r/{Score.Id}.qr";

                switch (args.Text)
                {
                    case ViewResults:
                        game.CurrentScreen.Exit(() => new ResultsScreen(MapManager.Selected.Value, Score));
                        break;
                    case WatchReplay:
                        // Download Online Replay
                        if (Score.IsOnline)
                        {
                            var dialog = new LoadingDialog("Downloading Replay",
                                "Fetching online replay! Please wait...", () =>
                                {
                                    var replay = Score.DownloadOnlineReplay();

                                    if (replay == null)
                                    {
                                        NotificationManager.Show(NotificationLevel.Error, "The replay you have tried to download failed. It may be unavailable.");
                                        return;
                                    }

                                    game.CurrentScreen.Exit(() => new MapLoadingScreen(new List<Score>(), replay));
                                });

                            DialogManager.Show(dialog);
                            return;
                        }

                        if (!File.Exists(replayPath))
                        {
                            NotificationManager.Show(NotificationLevel.Error, "The replay file could not be found!");
                            return;
                        }

                        game.CurrentScreen.Exit(() => new MapLoadingScreen(new List<Score>(), new Replay(replayPath)));
                        break;
                    case DownloadReplay:
                        if (!Score.IsOnline)
                            return;

                        // Download Online Replay
                        if (Score.IsOnline)
                        {
                            var dialog = new LoadingDialog("Downloading Replay",
                                "Fetching online replay! Please wait...", () =>
                                {
                                    var replay = Score.DownloadOnlineReplay(false);

                                    if (replay == null)
                                    {
                                        NotificationManager.Show(NotificationLevel.Error, "The replay you have tried to download failed. It may be unavailable.");
                                        return;
                                    }

                                    var dir = $"{ConfigManager.DataDirectory}/Downloads";
                                    var downloadPath = $"{dir}/{Score.Id}.qr";

                                    Utils.NativeUtils.HighlightInFileManager(downloadPath);
                                });

                            DialogManager.Show(dialog);
                            return;
                        }
                        break;
                    case ExportReplay:
                        var path = $"{ConfigManager.DataDirectory.Value}/r/{Score.Id}.qr";

                        if (!File.Exists(path))
                        {
                            NotificationManager.Show(NotificationLevel.Error, "The replay file could not be found!");
                            return;
                        }

                        Utils.NativeUtils.HighlightInFileManager(path);
                        break;
                    case Delete:
                        DialogManager.Show(new DeleteScoreDialog(Score));
                        break;
                    case PlayerProfile:
                        BrowserHelper.OpenURL($"https://quavergame.com/profile/{Score.PlayerId}");
                        break;
                    case SteamProfile:
                        BrowserHelper.OpenURL($"https://steamcommunity.com/profiles/{Score.SteamId}");
                        break;
                    case AddFriend:
                        OnlineManager.AddFriend(GetUserFromScore(Score));
                        break;
                    case RemoveFriend:
                        OnlineManager.RemoveFriend(GetUserFromScore(Score));
                        break;
                }
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        private static Dictionary<string, Color> GetOptions(Score score)
        {
            var options = new Dictionary<string, Color>();

            if (OnlineManager.CurrentGame == null)
            {
                options.Add(ViewResults, Color.White);
                options.Add(WatchReplay, ColorHelper.HexToColor("#9B51E0"));
            }

            if (score.IsOnline)
            {
                options.Add(DownloadReplay, ColorHelper.HexToColor("#0FBAE5"));
                options.Add(PlayerProfile, ColorHelper.HexToColor("#27B06E"));
                options.Add(SteamProfile, ColorHelper.HexToColor("#0787E3"));

                if (OnlineManager.FriendsList != null && OnlineManager.FriendsList.Contains(score.PlayerId))
                    options.Add(RemoveFriend, ColorHelper.HexToColor($"#FF6868"));
                else
                    options.Add(AddFriend, ColorHelper.HexToColor("#27B06E"));

                return options;
            }

            options.Add(ExportReplay, ColorHelper.HexToColor("#0787E3"));
            options.Add(Delete, ColorHelper.HexToColor($"#FF6868"));

            return options;
        }

        private static User GetUserFromScore(Score score) => new User(new OnlineUser()
        {
            Id = score.PlayerId,
            SteamId = score.SteamId,
            Username = score.Name
        });
    }
}
