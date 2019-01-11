using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.Timeline
{
    public class TimelineSnapLine : Sprite
    {
        private EditorScrollContainerKeys Container { get; }

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
        ///     If the object is currently on-screen.
        /// </summary>
        public bool IsInView { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="tp"></param>
        /// <param name="time"></param>
        /// <param name="index"></param>
        public TimelineSnapLine(EditorScrollContainerKeys container, TimingPointInfo tp, float time, int index)
        {
            Container = container;
            TimingPoint = tp;
            Index = index;
            Time = time;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime) =>  DrawToSpriteBatch();

        /// <summary>
        ///     Checks if the timing line is on-screen.
        /// </summary>
        /// <returns></returns>
        public bool CheckIfOnScreen() => Time * Container.TrackSpeed >= Container.TrackPositionY - Container.Height &&
                                         Time * Container.TrackSpeed <= Container.TrackPositionY + Container.Height;
    }
}