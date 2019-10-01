using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Scheduling;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Logging;
using Wobble.Scheduling;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Selection.Components
{
    /// <summary>
    ///     Responsible for loading and playing music throughout song select
    /// </summary>
    public class SelectJukebox : Container
    {
        private QuaverScreen Screen { get; }

        /// <summary>
        ///     If we're currently in the process of loading a track
        /// </summary>
        private bool IsLoadingTrack { get; set; }

        /// <summary>
        /// </summary>
        private TaskHandler<int, int> LoadTrackTask { get; set; }

        /// <summary>
        /// </summary>
        public SelectJukebox(QuaverScreen screen = null)
        {
            Screen = screen;
            Size = new ScalableVector2(0, 0);

            LoadTrackTask = new TaskHandler<int, int>((i, token) =>
            {
                LogLoadingTrack();
                AudioEngine.PlaySelectedTrackAtPreview();
                IsLoadingTrack = false;
                return 0;
            });

            LoadTrackTask.OnCancelled += (sender, args) => IsLoadingTrack = false;

            if (MapManager.Selected != null)
                MapManager.Selected.ValueChanged += OnMapChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            KeepPlayingAudioTrackAtPreview();
            base.Update(gameTime);
        }

        /// <summary>
        ///
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            if (MapManager.Selected != null)
                MapManager.Selected.ValueChanged -= OnMapChanged;

            LoadTrackTask.Dispose();

            base.Destroy();
        }

        /// <summary>
        ///     Plays the audio track at the preview time if it has stopped
        /// </summary>
        private void KeepPlayingAudioTrackAtPreview()
        {
            if (Screen != null && Screen.Exiting)
                return;

            // No map is currently selected
            if (MapManager.Selected == null || MapManager.Selected.Value == null)
                return;

            // Loading the first AudioTrack ever.
            if (AudioEngine.Track == null)
            {
                LogLoadingTrack();
                AudioEngine.PlaySelectedTrackAtPreview();
            }
            // Loading further AudioTracks, run under a separate thread
            else if (AudioEngine.Track.HasPlayed && AudioEngine.Track.IsStopped && !IsLoadingTrack)
            {
                if (LoadTrackTask.IsRunning)
                    LoadTrackTask.Cancel();

                IsLoadingTrack = true;
                LoadTrackTask.Run(0);
            }
            else if (!AudioEngine.Track.HasPlayed)
            {
                try
                {
                    AudioEngine.Track.Seek(MapManager.Selected.Value.AudioPreviewTime);
                }
                catch (Exception e)
                {
                    // ignored
                }

                if (!AudioEngine.Track.IsPlaying)
                    AudioEngine.Track.Play();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            if (MapManager.GetAudioPath(e.Value) == MapManager.GetAudioPath(e.OldValue) && e.Value != null)
                return;

            if (AudioEngine.Track == null)
                return;

            // On map switch, we want to just stop the track.
            // KeepPlayingAudioTrackAtPreview() will automatically load and play the track again at its preview point
            lock (AudioEngine.Track)
            {
                if (AudioEngine.Track.IsPlaying)
                    AudioEngine.Track.Stop();

                if (LoadTrackTask.IsRunning)
                    LoadTrackTask.Cancel();
            }
        }

        private void LogLoadingTrack() => Logger.Important($"Loading AudioTrack for: {MapManager.Selected.Value}", LogType.Runtime);
    }
}