using System;
using System.Linq;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Scheduling;
using Wobble.Logging;

namespace Quaver.Shared.Screens.V2.Main.UI
{
    /// <summary>
    ///     Owns menu music for the replacement main menu without bringing back the legacy footer UI.
    /// </summary>
    internal sealed class MainMenuJukeboxController
    {
        private MainMenuScreen Screen { get; }

        private Random Random { get; } = new Random();

        private bool LoadingTrack { get; set; }

        private bool IsDestroyed { get; set; }

        private int LoadFailures { get; set; }

        public MainMenuJukeboxController(MainMenuScreen screen)
        {
            Screen = screen;

            if (AudioEngine.Track != null && AudioEngine.Track.IsPaused)
            {
                AudioEngine.Track.Play();
                AudioEngine.Track.Fade(100, 300);
            }
        }

        public void Update()
        {
            if (IsDestroyed || Screen.Exiting || LoadingTrack || LoadFailures >= 3)
                return;

            var track = AudioEngine.Track;
            if (track == null || track.IsDisposed || track.HasPlayed && track.IsStopped)
            {
                SelectRandomTrack();
                return;
            }

            if (!track.HasPlayed)
                track.Play();
        }

        public void Destroy() => IsDestroyed = true;

        private void SelectRandomTrack()
        {
            var mapsets = MapManager.Mapsets.Where(mapset => mapset.Maps.Count != 0).ToList();
            if (mapsets.Count == 0)
                return;

            var mapset = mapsets[Random.Next(mapsets.Count)];
            var map = mapset.Maps[Random.Next(mapset.Maps.Count)];
            MapManager.Selected.Value = map;
            LoadingTrack = true;

            ThreadScheduler.Run(() =>
            {
                try
                {
                    if (IsDestroyed)
                        return;

                    AudioEngine.LoadCurrentTrack();
                    if (AudioEngine.Track != null && !AudioEngine.Track.IsDisposed)
                    {
                        AudioEngine.Track.Play();
                        LoadFailures = 0;
                    }

                    Logger.Debug($"Selected new v2 menu track: {map.Artist} - {map.Title} " +
                                 $"[{map.DifficultyName}]", LogType.Runtime);
                }
                catch (Exception exception)
                {
                    LoadFailures++;
                    Logger.Error(exception, LogType.Runtime);
                    Logger.Error($"Failed to load v2 menu track ({LoadFailures}/3).", LogType.Runtime);
                }
                finally
                {
                    LoadingTrack = false;
                }
            });
        }
    }
}
