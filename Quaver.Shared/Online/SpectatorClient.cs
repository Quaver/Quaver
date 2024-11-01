using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Enums;
using Quaver.API.Replays;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Helpers;
using Quaver.Server.Client.Structures;
using Quaver.Server.Client.Enums;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Online.API.Maps;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Importing;
using Quaver.Shared.Screens.Loading;
using Quaver.Shared.Screens.Main;
using Wobble;
using Wobble.Audio.Tracks;
using Wobble.Logging;

namespace Quaver.Shared.Online
{
    public class SpectatorClient
    {
        /// <summary>
        ///     The player that is currently being spectated
        /// </summary>
        public User Player { get; }

        /// <summary>
        ///     The user's current replay that is being received.
        /// </summary>
        public Replay Replay { get; set; }

        /// <summary>
        ///     The map that the user is currently playing
        /// </summary>
        public Map Map { get; private set; }

        /// <summary>
        ///     The list of frames for the current map/play session
        /// </summary>
        private List<SpectatorReplayFramesEventArgs> Frames { get; } = new List<SpectatorReplayFramesEventArgs>();

        /// <summary>
        ///     Returns if the client has notified the user if they don't have the map
        /// </summary>
        private bool HasNotifiedForThisMap { get; set; }

        /// <summary>
        ///     If the user has finished playing the map that they were playing
        /// </summary>
        public bool FinishedPlayingMap { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="player"></param>
        public SpectatorClient(User player) => Player = player;

        /// <summary>
        ///     Goes to spectate the user immediately
        /// </summary>
        public void WatchUserImmediately() => PlayNewMap(new List<ReplayFrame>(), false, true);

        /// <summary>
        ///     Handles when the client is beginning to play a new map
        /// </summary>
        public void PlayNewMap(List<ReplayFrame> frames, bool createNewReplay = true, bool forceIfImporting = false)
        {
            var game = (QuaverGame)GameBase.Game;

            if (createNewReplay)
            {
                FinishedPlayingMap = false;

                var mods = (ModIdentifier)Player.CurrentStatus.Modifiers;

                // Get correct modifiers if in the tournament viewer
                if (OnlineManager.CurrentGame != null)
                    mods = OnlineManager.GetUserActivatedMods(Player.OnlineUser.Id, OnlineManager.CurrentGame);

                // Create the new replay first, when playing a new map, we always want to start off with a fresh replay
                Replay = new Replay((GameMode)Player.CurrentStatus.GameMode, Player.OnlineUser.Username,
                    mods, Player.CurrentStatus.MapMd5);

                // Add all existing frames
                if (frames != null)
                {
                    Logger.Important($"Adding existing {frames.Count} replay frames", LogType.Runtime);

                    foreach (var frame in frames)
                        Replay.Frames.Add(frame);
                }
            }

            // This is handled elsewhere
            if (OnlineManager.CurrentGame != null)
                return;

            // Try to find the new map from the player
            Map = MapManager.FindMapFromMd5(Player.CurrentStatus.MapMd5);

            // Not in possession of the map
            if (Map == null)
            {
                if (!HasNotifiedForThisMap)
                {
                    NotificationManager.Show(NotificationLevel.Error, "You do not have the map the host is playing!");
                    HasNotifiedForThisMap = true;

                    DownloadMap();
                }

                if (game.CurrentScreen.Type == QuaverScreenType.Gameplay ||
                    game.CurrentScreen.Type == QuaverScreenType.Loading)
                {
                    game.GlobalUserInterface.Cursor.Alpha = 1;

                    // Exit out to the main menu
                    game.IsMouseVisible = false;
                    game.GlobalUserInterface.Cursor.Show(1);
                    game.CurrentScreen.Exit(() => new MainMenuScreen());
                    AudioTrack.AllowPlayback = true;
                }

                return;
            }

            MapManager.Selected.Value = Map;

            if (Map != BackgroundHelper.Map)
                BackgroundHelper.Load(Map);

            // Don't interrupt importing
            if (game.CurrentScreen.Type == QuaverScreenType.Importing && !forceIfImporting)
                return;

            game.CurrentScreen.Exit(() => new MapLoadingScreen(new List<Score>(), Replay, this));
        }

        /// <summary>
        ///     Adds a single replay frame to the spectating replay
        /// </summary>
        /// <param name="f"></param>
        public void AddFrame(ReplayFrame f) => Replay.Frames.Add(f);

        /// <summary>
        ///     Adds a bundle of replay frames to the spectating replay
        /// </summary>
        public void AddFrames(SpectatorReplayFramesEventArgs e)
        {
            if (e.Status == SpectatorClientStatus.NewSong)
                HasNotifiedForThisMap = false;

            if (OnlineManager.CurrentGame == null && e.Status == SpectatorClientStatus.NewSong || Replay == null)
                PlayNewMap(e.Frames);

            // A second null check is required in this case
            // because PlayNewMap() may not create a new replay instance depending on what the player is doing.
            if (Replay == null)
                return;

            lock (Replay.Frames)
            {
                switch (e.Status)
                {
                    // Do nothing for now
                    case SpectatorClientStatus.SelectingSong:
                        break;
                    case SpectatorClientStatus.FinishedSong:
                        FinishedPlayingMap = true;
                        break;
                    case SpectatorClientStatus.NewSong:
                        Replay.Frames.Clear();
                        break;
                    case SpectatorClientStatus.Playing:
                    case SpectatorClientStatus.Paused:
                        // Add frames to the replay
                        foreach (var f in e.Frames)
                            AddFrame(f);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Frames.Add(e);
        }

        /// <summary>
        ///     Starts the download for the map if it is available for downloading
        /// </summary>
        private void DownloadMap()
        {
            if (Player.CurrentStatus.MapId == -1)
                return;

            ThreadScheduler.Run(() =>
            {
                try
                {
                    // Grab complete info for the map
                    var response = new APIRequestMapInformation(Player.CurrentStatus.MapId).ExecuteRequest();

                    // If we're already downloading it, don't restart
                    if (MapsetDownloadManager.CurrentDownloads.Any(x => x.MapsetId == response.Map.MapsetId))
                        return;

                    // The mapset is currently being imported
                    if (MapsetImporter.Queue.Contains($"{ConfigManager.DataDirectory.Value}/Downloads/{response.Map.MapsetId}.qp"))
                        return;

                    var download = MapsetDownloadManager.Download(response.Map.MapsetId, response.Map.Artist, response.Map.Title);

                    var game = (QuaverGame)GameBase.Game;

                    // Automatically start importing
                    download.Status.ValueChanged += (sender, args) =>
                    {
                        if (args.Value.Status != FileDownloaderStatus.Complete)
                            return;
                        if (OnlineManager.IsSpectatingSomeone && !game.CurrentScreen.Exiting)
                        {
                            switch (game.CurrentScreen.Type)
                            {
                                case QuaverScreenType.Gameplay:
                                case QuaverScreenType.Loading:
                                case QuaverScreenType.Importing:
                                    break;
                                default:
                                    game.CurrentScreen.Exit(() => new ImportingScreen());
                                    break;
                            }
                        }
                    };

                    MapsetDownloadManager.OpenOnlineHub();
                    game.OnlineHub.SelectSection(OnlineHubSectionType.ActiveDownloads);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            });
        }
    }
}