using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Enums;
using Quaver.API.Replays;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Loading;
using Wobble;

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
        public Replay Replay { get; private set; }

        /// <summary>
        ///     The map that the user is currently playing
        /// </summary>
        public Map Map { get; private set; }

        /// <summary>
        ///     The list of frames for the current map/play session
        /// </summary>
        private List<SpectatorReplayFramesEventArgs> Frames { get; } = new List<SpectatorReplayFramesEventArgs>();

        /// <summary>
        /// </summary>
        /// <param name="player"></param>
        public SpectatorClient(User player) => Player = player;

        /// <summary>
        ///     Handles when the client is beginning to play a new map
        /// </summary>
        /// <param name="map"></param>
        /// <param name="mods"></param>
        private void PlayNewMap()
        {
            // Try to find the new map from the player
            Map = MapManager.FindMapFromMd5(Player.CurrentStatus.MapMd5);

            // Create the new replay first, when playing a new map, we always want to start off with a fresh replay
            Replay = new Replay(Map.Mode, Player.OnlineUser.Username, (ModIdentifier) Player.CurrentStatus.Modifiers, Map.Md5Checksum);

            // We don't have the map...
            if (Map == null)
            {
                NotificationManager.Show(NotificationLevel.Error,"You do not have the map the spectating player is playing!");
                return;
            }

            MapManager.Selected.Value = Map;

            // Load the map up and start the spectating session.
            // TODO: Handle for multiple spectating clients (probably will want a base tourney screen & return the new gameplay screen for each client)
            var game = (QuaverGame) GameBase.Game;
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
            if (Replay == null)
                PlayNewMap();

            if (Replay == null)
                return;

            lock (Replay.Frames)
            {
                switch (e.Status)
                {
                    case SpectatorClientStatus.SelectingSong:
                        Replay.Frames.Clear();
                        break;
                    case SpectatorClientStatus.NewSong:
                        Replay.Frames.Clear();
                        break;
                    case SpectatorClientStatus.Playing:
                    case SpectatorClientStatus.Paused:
                        // Add frames to the replay
                        foreach (var f in e.Frames.OrderBy(x => x.Time))
                            AddFrame(f);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Frames.Add(e);
        }
    }
}