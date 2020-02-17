using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Wobble.Bindables;
using Wobble.Graphics.Sprites;

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
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime) => DrawToSpriteBatch();

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
            X = Playfield.AbsolutePosition.X + 2;
            Y = Playfield.HitPositionY - Time * Playfield.TrackSpeed - Height;
        }
    }
}