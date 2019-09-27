using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Quaver.API.Replays;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Loading;
using Quaver.Shared.Screens.Result;
using Quaver.Shared.Screens.Selection.UI.Leaderboard.Dialogs;
using Quaver.Shared.Screens.Selection.UI.Mapsets.Maps;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Platform;
using Wobble.Platform.Linux;

namespace Quaver.Shared.Screens.Selection.UI.Leaderboard.Components
{
    public class LeaderboardScoreRightClickOptions : RightClickOptions
    {
        /// <summary>
        /// </summary>
        private Score Score { get; }

        private const string ViewResults = "View Results";

        private const string WatchReplay = "Watch Replay";

        private const string ExportReplay = "Export Replay";

        private const string PlayerProfile = "Player Profile";

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
                        game.CurrentScreen.Exit(() => new ResultScreen(Score));
                        break;
                    case WatchReplay:
                        // Download Online Replay
                        if (Score.IsOnline)
                            return;

                        if (!File.Exists(replayPath))
                        {
                            NotificationManager.Show(NotificationLevel.Error, "The replay file could not be found!");
                            return;
                        }

                        game.CurrentScreen.Exit(() => new MapLoadingScreen(new List<Score>(), new Replay(replayPath)));
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
                        BrowserHelper.OpenURL($"https://quavergame.com/profile/{Score.Name}");
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
            var options = new Dictionary<string, Color>()
            {
                {ViewResults, Color.White},
                {WatchReplay, ColorHelper.HexToColor("#9B51E0")},
            };

            if (score.IsOnline)
            {
                options.Add(PlayerProfile, ColorHelper.HexToColor("#27B06E"));
                return options;
            }

            options.Add(ExportReplay, ColorHelper.HexToColor("#0787E3"));
            options.Add(Delete, ColorHelper.HexToColor($"#FF6868"));

            return options;
        }
    }
}