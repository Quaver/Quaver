using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Wobble.Audio.Tracks;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;

namespace Quaver.Shared.Graphics.Graphs
{
    public class DifficultySeekBar : ImageButton
    {
        /// <summary>
        /// </summary>
        private Qua Map { get; }

        /// <summary>
        /// </summary>
        private ModIdentifier Mods { get; }

        /// <summary>
        /// </summary>
        private DifficultyProcessorKeys Processor { get; }

        /// <summary>
        /// </summary>
        private IAudioTrack Track { get; }

        /// <summary>
        ///     The maximum amount of bars that will be displayed on the graph
        /// </summary>
        private int MaxBars { get; }

        /// <summary>
        ///     The size/height of each bar
        /// </summary>
        private int BarSize { get; }

        /// <summary>
        ///     If the bars should be aligned from right to left instead of left to right
        /// </summary>
        private bool AlignRightToLeft { get; }

        /// <summary>
        ///     The scale of the bars in relation to the width
        /// </summary>
        private float BarWidthScale { get; }

        /// <summary>
        ///     The time for each sample in the graph
        /// </summary>
        private int SampleTime => (int) Math.Ceiling(Track.Length / MaxBars);

        /// <summary>
        /// </summary>
        private Sprite SeekBarLine { get; set; }

        /// <summary>
        ///     Event invoked when the audio has seeked
        /// </summary>
        public event EventHandler<SeekBarAudioSeekedEventArgs> AudioSeeked;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="mods"></param>
        /// <param name="size"></param>
        /// <param name="maxBars"></param>
        /// <param name="barSize"></param>
        /// <param name="track"></param>
        /// <param name="alignRightToLeft"></param>
        /// <param name="barWidthScale"></param>
        public DifficultySeekBar(Qua map, ModIdentifier mods, ScalableVector2 size, int maxBars = 120, int barSize = 3, IAudioTrack track = null,
            bool alignRightToLeft = false, float barWidthScale = 1)
            : base(UserInterface.BlankBox)
        {
            Map = map;
            Mods = mods;
            Size = size;
            MaxBars = maxBars;
            BarSize = barSize;
            AlignRightToLeft = alignRightToLeft;
            BarWidthScale = barWidthScale;

            Processor = (DifficultyProcessorKeys) Map.SolveDifficulty(Mods);

            Track = AudioEngine.Track ?? track ?? new AudioTrackVirtual(Map.Length + 5000);

            CreateBars();
            CreateProgressSeekBar();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Handle dragging in the song
            if (IsHeld && MouseManager.CurrentState.LeftButton == ButtonState.Pressed)
            {
                if (!Track.IsDisposed)
                {
                    var percentage = (MouseManager.CurrentState.Y - AbsolutePosition.Y) / AbsoluteSize.Y;
                    var targetPos = (1 - percentage) * Track.Length;

                    if ((int) targetPos != (int) Track.Time && targetPos >= 0 && targetPos <= Track.Length)
                    {
                        if (Math.Abs(Track.Time - targetPos) < 500)
                            return;

                        Track.Seek(targetPos);
                        AudioSeeked?.Invoke(this, new SeekBarAudioSeekedEventArgs());
                    }
                }
            }

            if (SeekBarLine != null)
                SeekBarLine.Y = Height - (float) (Track.Time  / Track.Length) * Height;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            AudioSeeked = null;
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateBars()
        {
            var groupedSamples = Map.HitObjects.GroupBy(u => u.StartTime / SampleTime)
                .Select(grp => grp.ToList())
                .ToList();

            var calculators = new List<DifficultyProcessorKeys>();

            foreach (var s in groupedSamples)
            {
                var qua = new Qua { Mode = Map.Mode };

                if (s.Count != 0)
                    s.ForEach(x => qua.HitObjects.Add(x));

                var diff = (DifficultyProcessorKeys) qua.SolveDifficulty(Mods);

                if (s.Count != 0 && diff.StrainSolverData.Count == 0)
                    diff.StrainSolverData.Add(new StrainSolverData(new StrainSolverHitObject(s.First())));

                calculators.Add(diff);
            }

            var highestDiff = calculators.Max(x => x.OverallDifficulty);

            foreach (var calculator in calculators)
            {
                var width = MathHelper.Clamp(calculator.OverallDifficulty / highestDiff * Width, 6, Width);

                if (calculator.StrainSolverData.Count == 0)
                    continue;

                // ReSharper disable once ObjectCreationAsStatement
                new Sprite
                {
                    Parent = this,
                    Alignment = AlignRightToLeft ? Alignment.BotRight : Alignment.BotLeft,
                    Size = new ScalableVector2((int) (width * BarWidthScale), BarSize),
                    Y = -Height * (float) (calculator.StrainSolverData.First().StartTime / SampleTime * SampleTime / (Track.Length / Track.Rate)) - 2,
                    Tint = ColorHelper.DifficultyToColor(calculator.OverallDifficulty)
                };
            }
        }

        /// <summary>
        /// </summary>
        private void CreateProgressSeekBar() => SeekBarLine = new Sprite()
        {
            Parent = this,
            Size = new ScalableVector2(Width, 4),
            Tint = Color.White,
            Y = (float) (Track.Time / Track.Length) * Height
        };
    }
}