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
    public class EditorPlayfieldTimelineTick : Sprite, IStartTime
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
        public float StartTime { get; set; }

        /// <summary>
        ///     The index of the timing point this snap line is.
        /// </summary>
        public int Index { get; }

        /// <summary>
        ///     Whether or not this line is for a measure.
        /// </summary>
        public bool IsMeasureLine { get; }

        private SpriteTextPlus Measure { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="playfield"></param>
        /// <param name="tp"></param>
        /// <param name="beatSnap"></param>
        /// <param name="startTimearam>
        /// <param name="index"></param>
        /// <param name="measureCount"></param>
        public EditorPlayfieldTimelineTick(EditorPlayfield playfield, TimingPointInfo tp, float startTime, int index, int measureCount, bool isMeasureLine)
        {
            Playfield = playfield;
            TimingPoint = tp;
            Index = index;
            StartTime = startTime;
            IsMeasureLine = isMeasureLine;

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
        public bool IsOnScreen() => StartTime * Playfield.TrackSpeed >= Playfield.TrackPositionY - Playfield.Height &&
                                         StartTime * Playfield.TrackSpeed <= Playfield.TrackPositionY + Playfield.Height;

        /// <summary>
        /// </summary>
        public void SetPosition()
        {
            var x = Playfield.AbsolutePosition.X + 2;
            var y = Playfield.HitPositionY - StartTime * Playfield.TrackSpeed - Height;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (X != x || Y != y)
                Position = new ScalableVector2(x, y);
        }
    }
}