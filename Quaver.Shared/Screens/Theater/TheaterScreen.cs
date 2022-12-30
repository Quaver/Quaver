using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Replays;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Main;
using Quaver.Shared.Screens.Tournament;
using Wobble;
using Wobble.Input;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Theater
{
    public sealed class TheaterScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Theatre;

        /// <summary>
        ///     The replays to be used when watching
        /// </summary>
        public List<Replay> Replays { get; } = new List<Replay>();

        /// <summary>
        ///     Invoked when a replay has been loaded
        /// </summary>
        public event EventHandler<ReplayLoadedEventArgs> ReplayLoaded;

        /// <summary>
        /// </summary>
        public TheaterScreen()
        {
            GameBase.Game.Window.FileDropped += OnFileDropped;
            View = new TheaterScreenView(this);
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleInput();

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            GameBase.Game.Window.FileDropped -= OnFileDropped;
            ReplayLoaded = null;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void HandleInput()
        {
            if (Exiting)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
            {
                Exit(() => new MainMenuScreen());
                return;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFileDropped(object sender, string e)
        {
            if (Replays.Count >= 4)
            {
                NotificationManager.Show(NotificationLevel.Warning, "You cannot watch more than 4 replays at a time!");
                return;
            }

            if (!e.EndsWith(".qr"))
                return;

            ThreadScheduler.Run(() =>
            {
                try
                {
                    var replay = new Replay(e);

                    // Check to see if the map actually exists
                    var map = MapManager.FindMapFromMd5(replay.MapMd5);

                    if (map == null)
                    {
                        NotificationManager.Show(NotificationLevel.Warning, "You do not have the map the replay is for.");
                        return;
                    }

                    MapManager.Selected.Value = map;

                    lock (Replays)
                    {
                        // Check if the replays are all from the same map
                        if (Replays.Count != 0)
                        {
                            if (replay.MapMd5 != Replays.First().MapMd5)
                            {
                                NotificationManager.Show(NotificationLevel.Warning, "All replays must be from the same map!");
                                return;
                            }
                        }

                        // Add replay to the list
                        if (Replays.Count != 4)
                            Replays.Add(replay);

                        ReplayLoaded?.Invoke(this, new ReplayLoadedEventArgs(replay));
                        NotificationManager.Show(NotificationLevel.Info, $"Loaded replay from: {replay.PlayerName}");
                    }

                    // Start playing song
                    if (AudioEngine.Map != map)
                    {
                        AudioEngine.LoadCurrentTrack();
                        AudioEngine.Track.Play();
                    }

                    if (BackgroundHelper.Map != map)
                        BackgroundHelper.Load(map);
                }
                catch (Exception ex)
                {
                    NotificationManager.Show(NotificationLevel.Error, "The replay file you have given cannot be read.");
                    Logger.Error(ex, LogType.Runtime);
                }
            });
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.InMenus, -1, "-1", 1, "", 0);
    }
}