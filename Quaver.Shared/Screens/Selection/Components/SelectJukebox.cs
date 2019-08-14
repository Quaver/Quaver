using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Scheduling;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Selection.Components
{
    /// <summary>
    ///     Responsible for loading and playing music throughout song select
    /// </summary>
    public class SelectJukebox : Container
    {
        /// <summary>
        ///     If we're currently in the process of loading a track
        /// </summary>
        private bool IsLoadingTrack { get; set; }

        /// <summary>
        /// </summary>
        public SelectJukebox()
        {
            Size = new ScalableVector2(0, 0);

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

            base.Destroy();
        }

        /// <summary>
        ///     Plays the audio track at the preview time if it has stopped
        /// </summary>
        private void KeepPlayingAudioTrackAtPreview()
        {
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
                LogLoadingTrack();

                IsLoadingTrack = true;

                ThreadScheduler.Run(() =>
                {
                    AudioEngine.PlaySelectedTrackAtPreview();
                    IsLoadingTrack = false;
                });
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

                IsLoadingTrack = false;
            }
        }

        private void LogLoadingTrack() => Logger.Important($"Loading AudioTrack for: {MapManager.Selected.Value}", LogType.Runtime);
    }
}