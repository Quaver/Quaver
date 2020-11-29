using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Timeline
{
    public class EditorPlayfieldTimelineTick : Sprite
    {
        /// <summary>
        /// </summary>
        private EditorPlayfield Playfield { get; }

        /// <summary>
        ///     The timing point this snap line belongs to.
        /// </summary>
        public TimingPointInfo TimingPoint { get; }

        /// <summary>
        ///     The time in the song the line is located.
        /// </summary>
        public float Time { get; }

        /// <summary>
        ///     The index of the timing point this snap line is.
        /// </summary>
        public int Index { get; }

        /// <summary>
        ///     Determines if this line is for a measure.
        /// </summary>
        public bool IsMeasureLine => Index / BeatSnap.Value % 4 == 0
                                      && Index % BeatSnap.Value == 0 && Time >= TimingPoint.StartTime;

        /// <summary>
        /// </summary>
        private BindableInt BeatSnap { get; }

        private SpriteTextPlus Measure { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="playfield"></param>
        /// <param name="tp"></param>
        /// <param name="beatSnap"></param>
        /// <param name="time"></param>
        /// <param name="index"></param>
        /// <param name="measureCount"></param>
        public EditorPlayfieldTimelineTick(EditorPlayfield playfield, TimingPointInfo tp, BindableInt beatSnap, float time, int index, int measureCount)
        {
            Playfield = playfield;
            TimingPoint = tp;
            Index = index;
            Time = time;
            BeatSnap = beatSnap;

            if (!IsMeasureLine)
                return;

            Y = -2;

            Measure = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), measureCount.ToString(), 24, false)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
            };

            Measure.X = -Measure.Width - 16;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            DrawToSpriteBatch();
            Measure?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Measure?.Destroy();
            base.Destroy();
        }

        /// <summary>
        ///     Checks if the timing line is on-screen.
        /// </summary>
        /// <returns></returns>
        public bool IsOnScreen() => Time * Playfield.TrackSpeed >= Playfield.TrackPositionY - Playfield.Height &&
                                         Time * Playfield.TrackSpeed <= Playfield.TrackPositionY + Playfield.Height;

        /// <summary>
        /// </summary>
        public void SetPosition()
        {
            var x = Playfield.AbsolutePosition.X + 2;
            var y = Playfield.HitPositionY - Time * Playfield.TrackSpeed - Height;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (X != x || Y != y)
                Position = new ScalableVector2(x, y);
        }
    }
}