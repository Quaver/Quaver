using System;
using System.Globalization;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.Panels
{
    public class EditorPanelDetails : EditorPanel
    {
        /// <summary>
        /// </summary>
        private Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        private BindableInt BeatSnap { get; }

        /// <summary>
        /// </summary>
        private IAudioTrack Track { get; }

        /// <summary>
        /// </summary>
        private EditorDetailsPanelKeyValue ObjectCount { get; set; }

        /// <summary>
        /// </summary>
        private EditorDetailsPanelKeyValue BeatSnapText { get; set; }

        /// <summary>
        /// </summary>
        private EditorDetailsPanelKeyValue PlaybackSpeed { get; set; }

        /// <summary>
        /// </summary>
        private EditorDetailsPanelKeyValue DifficultyRating { get; set; }

        /// <summary>
        /// </summary>
        private EditorDetailsPanelKeyValue Bpm { get; set; }

        /// <summary>
        /// </summary>
        private const int SpacingY = 14;

        /// <summary>
        /// </summary>
        /// <param name="workingMap"></param>
        /// <param name="beatSnap"></param>
        /// <param name="track"></param>
        public EditorPanelDetails(Qua workingMap, BindableInt beatSnap, IAudioTrack track) : base("Details")
        {
            WorkingMap = workingMap;
            BeatSnap = beatSnap;
            Track = track;

            CreateObjectCount();
            CreatePlaybackSpeed();
            CreateBpm();
            CreateBeatSnapText();
            CreateDifficultyRating();
            BeatSnap.ValueChanged += OnBeatSnapValueChanged;
            Track.RateChanged += OnTrackRateChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            BeatSnap.ValueChanged -= OnBeatSnapValueChanged;
            Track.RateChanged -= OnTrackRateChanged;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateObjectCount()
        {
            ObjectCount = new EditorDetailsPanelKeyValue("Object Count", $"{WorkingMap.HitObjects.Count:n0}", Width)
            {
                Parent = Content,
                Y = 12
            };
        }

        /// <summary>
        /// </summary>
        private void CreateBeatSnapText()
        {
            BeatSnapText = new EditorDetailsPanelKeyValue("Beat Snap", $"1/{StringHelper.AddOrdinal(BeatSnap.Value)}", Width)
            {
                Parent = Content,
                Y = Bpm.Y + Bpm.Height + SpacingY,
                Value =
                {
                    Tint = ColorHelper.BeatSnapToColor(BeatSnap.Value)
                }
            };
        }

        /// <summary>
        /// </summary>
        private void CreatePlaybackSpeed()
        {
            PlaybackSpeed = new EditorDetailsPanelKeyValue($"Playback Speed", $"{(int) (Track.Rate * 100)}%", Width)
            {
                Parent = Content,
                Y = ObjectCount.Y + ObjectCount.Height + SpacingY
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDifficultyRating()
        {
            DifficultyRating = new EditorDetailsPanelKeyValue("Difficulty Rating", "0.00", Width)
            {
                Parent = Content,
                Y = BeatSnapText.Y + BeatSnapText.Height + SpacingY
            };

            CalculateRating();
        }

        /// <summary>
        /// </summary>
        private void CreateBpm()
        {
            Bpm = new EditorDetailsPanelKeyValue("BPM", "0", Width)
            {
                Parent = Content,
                Y = PlaybackSpeed.Y + PlaybackSpeed.Height + SpacingY
            };

            UpdateBpm();
        }

        /// <summary>
        /// </summary>
        private void CalculateRating()
        {
            var rating = WorkingMap.SolveDifficulty().OverallDifficulty;

            DifficultyRating.Value.Text = StringHelper.RatingToString(rating);
            DifficultyRating.Value.Tint = ColorHelper.DifficultyToColor(rating);
        }

        /// <summary>
        /// </summary>
        private void UpdateBpm()
        {
            var tp = WorkingMap.GetTimingPointAt(Track.Time);

            if (tp == null)
            {
                Bpm.Value.Text = "0";
                return;
            }

            Bpm.Value.Text = tp.Bpm.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBeatSnapValueChanged(object sender, BindableValueChangedEventArgs<int> e)
        {
            BeatSnapText.Value.Text = $"1/{StringHelper.AddOrdinal(BeatSnap.Value)}";
            BeatSnapText.Value.Tint = ColorHelper.BeatSnapToColor(BeatSnap.Value);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTrackRateChanged(object sender, TrackRateChangedEventArgs e)
            => PlaybackSpeed.Value.Text = $"{(int) (Track.Rate * 100)}%";
    }

    public class EditorDetailsPanelKeyValue : Container
    {
        /// <summary>
        /// </summary>
        public SpriteTextPlus Key { get; }

        /// <summary>
        /// </summary>
        public SpriteTextPlus Value { get; }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="width"></param>
        public EditorDetailsPanelKeyValue(string key, string value, float width)
        {
            const int padding = 18;

            Key = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), key, 21)
            {
                Parent = this,
                X = padding
            };

            Value = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), value, 21)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -padding
            };

            Size = new ScalableVector2(width, Math.Max(Key.Height, Value.Height));
        }
    }
}