using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens;
using Wobble;
using Wobble.Audio.Tracks;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

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
        public IAudioTrack Track { get; set; }

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
        /// </summary>
        private Sprite SeekBarLine { get; set; }

        /// <summary>
        ///     Event invoked when the audio has seeked
        /// </summary>
        public event EventHandler<SeekBarAudioSeekedEventArgs> AudioSeeked;

        /// <summary>
        ///     The acive bars with their appropriate sample time
        /// </summary>
        protected List<Sprite> Bars { get; set; }

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

            Track = track ?? AudioEngine.Track ?? new AudioTrackVirtual(Map.Length + 5000);

            CreateBars();
            CreateProgressSeekBar();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var game = GameBase.Game as QuaverGame;

            // Handle dragging in the song
            if (IsHeld && MouseManager.CurrentState.LeftButton == ButtonState.Pressed)
            {
                var percentage = (MouseManager.CurrentState.Y - AbsolutePosition.Y) / AbsoluteSize.Y;
                var targetPos = (1 - percentage) * Track.Length;

                SeekToPos(targetPos);
            }
            else if (!KeyboardManager.IsAltDown() && IsHovered &&
                    (game?.CurrentScreen?.Type == QuaverScreenType.Select ||
                     game?.CurrentScreen?.Type == QuaverScreenType.Multiplayer))
            {
                if (MouseManager.CurrentState.ScrollWheelValue < MouseManager.PreviousState.ScrollWheelValue)
                    SeekInDirection(Direction.Forward);
                else if (MouseManager.CurrentState.ScrollWheelValue > MouseManager.PreviousState.ScrollWheelValue)
                    SeekInDirection(Direction.Backward);
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
        protected void CreateBars()
        {
            Bars?.ForEach(x => x.Destroy());
            Bars = new List<Sprite>();

            if (Map.HitObjects.Count == 0)
                return;

            var rate = ModHelper.GetRateFromMods(Mods);
            var sampleTime = (int) Math.Ceiling(Track.Length / rate / MaxBars);
            var regularLength = Track.Length / rate;
            var diff = (DifficultyProcessorKeys)Map.SolveDifficulty(Mods);

            var bins = new List<(float, List<StrainSolverData>)>();
            // times are not scaled to rate
            for (var time = 0; time < regularLength; time += sampleTime)
            {
                var valuesInBin = diff.StrainSolverData.Where(s => s.StartTime >= time && s.StartTime < time + sampleTime);
                var pos = (float) (time / regularLength);
                bins.Add((pos, valuesInBin.ToList()));
            }

            if (bins.Count == 0)
                return;

            var highestDiff = bins.Max(grp =>
                grp.Item2.Any() ? grp.Item2.Average(s => s.TotalStrainValue) : 0
            );

            AddScheduledUpdate(() =>
            {
                foreach (var (pos, group) in bins)
                {
                    var rating = group.Any() ? group.Average(s => s.TotalStrainValue) : 0;
                    if (rating < 0.05)
                        continue;
                    var width = MathHelper.Clamp(rating / highestDiff * Width, 6, Width);

                    var bar = new Sprite
                    {
                        Parent = this,
                        Alignment = AlignRightToLeft ? Alignment.BotRight : Alignment.BotLeft,
                        Size = new ScalableVector2((int) (width * BarWidthScale), BarSize),
                        Y = -Height * pos - 2,
                        Tint = ColorHelper.DifficultyToColor(rating)
                    };

                    Bars.Add(bar);
                }

                SeekBarLine.Parent = this;
            });
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

        /// <summary>
        ///     Seeks to the specified position, and invokes the respective event.
        /// </summary>
        private void SeekToPos(double targetPos)
        {
            if(Track.IsDisposed)
                return;

            if ((int) targetPos != (int) Track.Time && targetPos >= 0 && targetPos <= Track.Length)
            {
                if (Math.Abs(Track.Time - targetPos) < 500)
                    return;

                Track.Seek(targetPos);
                AudioSeeked?.Invoke(this, new SeekBarAudioSeekedEventArgs());
            }
        }

        /// <summary>
        ///     Seeks in the specified direction.
        /// </summary>
        private void SeekInDirection(Direction direction)
        {
            if (Track.IsDisposed)
                return;

            var time = Track.Time;

            if (direction == Direction.Forward)
                time += 1000;
            else if (direction == Direction.Backward)
                time -= 1000;

            if (time < 0)
                time = 0;

            if (time > Track.Length)
                time = Track.Length;

            Track.Seek(time);
            AudioSeeked?.Invoke(this, new SeekBarAudioSeekedEventArgs());
        }
    }
}