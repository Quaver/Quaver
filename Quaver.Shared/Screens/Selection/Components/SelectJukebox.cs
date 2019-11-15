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
        /// </summary>
        private TaskHandler<int, int> LoadTrackTask { get; set; }

        /// <summary>
        /// </summary>
        private bool AudioTrackStoppedInLastFrame { get; set; }

        /// <summary>
        /// </summary>
        public SelectJukebox(QuaverScreen screen = null)
        {
            Screen = screen;
            Size = new ScalableVector2(0, 0);

            LoadTrackTask = new TaskHandler<int, int>((i, token) =>
            {
                AudioEngine.PlaySelectedTrackAtPreview();
                AudioTrackStoppedInLastFrame = false;
                return 0;
            });

            LoadTrackTask.OnCancelled += (sender, args) => AudioTrackStoppedInLastFrame = false;

            if (MapManager.Selected != null)
            {
                MapManager.Selected.ValueChanged += OnMapChanged;

                if (AudioEngine.Map != MapManager.Selected.Value || AudioEngine.Track.IsStopped)
                    LoadTrackTask.Run(0);
            }
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

            if (AudioEngine.Track == null)
                return;

            if (AudioTrackStoppedInLastFrame && !LoadTrackTask.IsRunning)
            {
                LogLoadingTrack();
                LoadTrackTask.Run(0);
            }

            AudioTrackStoppedInLastFrame = AudioEngine.Track.HasPlayed && AudioEngine.Track.IsDisposed;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            if (Screen != null && Screen.Exiting)
                return;

            if (MapManager.GetAudioPath(e.Value) == MapManager.GetAudioPath(e.OldValue) && e.Value != null)
                return;

            if (AudioEngine.Track == null)
                return;

            // On map switch, we want to just stop the track.
            lock (AudioEngine.Track)
            {
                if (LoadTrackTask.IsRunning)
                    LoadTrackTask.Cancel();

                LogLoadingTrack();
                LoadTrackTask.Run(0);
            }
        }

        private void LogLoadingTrack() => Logger.Important($"Loading AudioTrack for: {MapManager.Selected.Value}", LogType.Runtime);
    }
}