/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Editor.UI.Details
{
    public class EditorDetailsPanel : Sprite
    {
        /// <summary>
        /// </summary>
        private EditorScreen Screen { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap ObjectCount { get; set; }

        /// <summary>
        /// </summary>
        private Sprite HeaderBackground { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap TextBeatSnap { get; set; }

        /// <summary>
        /// </summary>
        private int LastObjectCount { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap TextDifficultyRating { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap TextDifficultyRatingNumber { get; set; }

        /// <summary>
        /// </summary>
        private Sprite LoadingWheel { get; set; }

        /// <summary>
        /// </summary>
        private CancellationTokenSource Source { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap PlaybackRate { get; set; }

        /// <summary>
        /// </summary>
        private float PreviousPlaybackRate { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap Bpm { get; set; }

        /// <summary>
        /// </summary>
        private TimingPointInfo LastTimingPoint { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorDetailsPanel(EditorScreen screen)
        {
            Screen = screen;
            Size = new ScalableVector2(230, 194);
            Image = UserInterface.EditorDetailsPanel;

            CreateHeaderBackground();
            CreateTextObjectCount();
            CreateTextBeatSnap();
            CreatePlaybackRate();
            CreateTextDifficultyRating();
            CreateLoadingWheel();
            CreateTextBpm();

            Screen.BeatSnap.ValueChanged += OnBeatSnapChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleLoadingWheelAnimations();

            if (Screen.WorkingMap.HitObjects.Count != LastObjectCount)
            {
                ObjectCount.Text = $"Object Count: {Screen.WorkingMap.HitObjects.Count:n0}";
                RecalculateDifficulty();
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (AudioEngine.Track.Rate != PreviousPlaybackRate)
                PlaybackRate.Text = $"Playback Speed: {AudioEngine.Track.Rate:0.00}x";

            var timingPoint = Screen.WorkingMap.GetTimingPointAt(AudioEngine.Track.Time);

            if (timingPoint == null)
                Bpm.Text = "BPM: No Timing Point!";
            else if (timingPoint != LastTimingPoint)
                Bpm.Text = $"BPM: {timingPoint?.Bpm:0.00}";

            LastTimingPoint = timingPoint;
            LastObjectCount = Screen.WorkingMap.HitObjects.Count;
            PreviousPlaybackRate = AudioEngine.Track.Rate;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            Screen.BeatSnap.ValueChanged -= OnBeatSnapChanged;
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateHeaderBackground() => HeaderBackground = new Sprite
        {
            Parent = this,
            Alignment = Alignment.TopLeft,
            Size = new ScalableVector2(Width, 38),
            Tint = Color.Transparent,
        };

        /// <summary>
        /// </summary>
        private void CreateTextObjectCount() => ObjectCount = new SpriteTextBitmap(FontsBitmap.AllerRegular,
            $"Object Count: {Screen.WorkingMap.HitObjects.Count:n0}")
        {
            Parent = this,
            X = 10,
            Y = HeaderBackground.Height + 10,
            FontSize = 16
        };

        /// <summary>
        /// </summary>
        private void CreateTextBeatSnap() => TextBeatSnap = new SpriteTextBitmap(FontsBitmap.AllerRegular,
            $"Beat Snap: 1/{StringHelper.AddOrdinal(Screen.BeatSnap.Value)}")
        {
            Parent = this,
            X = 10,
            Y = ObjectCount.Y + ObjectCount.Height + 14,
            FontSize = 16
        };

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBeatSnapChanged(object sender, BindableValueChangedEventArgs<int> e)
            => TextBeatSnap.Text = $"Beat Snap: 1/{StringHelper.AddOrdinal(Screen.BeatSnap.Value)}";

        /// <summary>
        ///
        /// </summary>
        private void CreateTextDifficultyRating()
        {
            TextDifficultyRating = new SpriteTextBitmap(FontsBitmap.AllerRegular,
                $"Difficulty Rating: ")
            {
                Parent = this,
                X = 10,
                Y = PlaybackRate.Y + PlaybackRate.Height + 14,
                FontSize = 16
            };

            // Initially calculate the difficulty rating
            var rating = Screen.WorkingMap.SolveDifficulty().OverallDifficulty;

            TextDifficultyRatingNumber = new SpriteTextBitmap(FontsBitmap.AllerRegular,
                $"{rating:0.00}")
            {
                Parent = this,
                X = TextDifficultyRating.X + TextDifficultyRating.Width + 6,
                Y = TextDifficultyRating.Y,
                FontSize = 16,
                Tint = ColorHelper.DifficultyToColor(rating)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateLoadingWheel() => LoadingWheel = new Sprite
        {
            Parent = TextDifficultyRating,
            Alignment = Alignment.MidLeft,
            Size = new ScalableVector2(20, 20),
            Image = UserInterface.LoadingWheel,
            Tint = Color.White,
            X = TextDifficultyRating.Width + 6,
            Alpha = 0
        };

        /// <summary>
        /// </summary>
        private void HandleLoadingWheelAnimations()
        {
            if (LoadingWheel.Animations.Count != 0)
                return;

            var rotation = MathHelper.ToDegrees(LoadingWheel.Rotation);
            LoadingWheel.ClearAnimations();
            LoadingWheel.Animations.Add(new Animation(AnimationProperty.Rotation, Easing.Linear, rotation, rotation + 360, 1000));
        }

        /// <summary>
        ///     Recalculates the difficulty of the map.
        /// </summary>
        private void RecalculateDifficulty()
        {
            Source?.Cancel();
            Source = new CancellationTokenSource();

            Task.Run(() =>
            {
                try
                {
                    TextDifficultyRatingNumber.ClearAnimations();
                    TextDifficultyRatingNumber.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, TextDifficultyRatingNumber.Alpha, 0, 300));

                    LoadingWheel.ClearAnimations();
                    LoadingWheel.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, LoadingWheel.Alpha, 1, 300));

                    var rating = Screen.WorkingMap.SolveDifficulty().OverallDifficulty;
                    TextDifficultyRatingNumber.Text = $"{rating:0.00}";
                    TextDifficultyRatingNumber.Tint = ColorHelper.DifficultyToColor(rating);

                    LoadingWheel.ClearAnimations();
                    LoadingWheel.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, LoadingWheel.Alpha, 0, 300));

                    TextDifficultyRatingNumber.ClearAnimations();
                    TextDifficultyRatingNumber.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, TextDifficultyRatingNumber.Alpha, 1, 300));
                }
                catch (Exception)
                {
                }
            }, Source.Token);
        }

        /// <summary>
        /// </summary>
        private void CreatePlaybackRate() => PlaybackRate = new SpriteTextBitmap(FontsBitmap.AllerRegular,
            $"Playback Speed: 1.0x")
        {
            Parent = this,
            X = TextBeatSnap.X,
            Y = TextBeatSnap.Y + TextBeatSnap.Height + 14,
            FontSize = 16,
        };

        /// <summary>
        /// </summary>
        private void CreateTextBpm()
        {
            var timingPoint = Screen.WorkingMap.GetTimingPointAt(AudioEngine.Track.Time);

            Bpm = new SpriteTextBitmap(FontsBitmap.AllerRegular,
                $"BPM: 0")
            {
                Parent = this,
                X = TextBeatSnap.X,
                Y = TextDifficultyRating.Y + TextDifficultyRating.Height + 14,
                FontSize = 16,
            };

            if (timingPoint != null)
                Bpm.Text = $"BPM: {timingPoint.Bpm:0.00}";

            LastTimingPoint = timingPoint;
        }
    }
}