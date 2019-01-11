using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.Timeline
{
    public class TimelineSnapLine : Sprite
    {
        /// <summary>
        ///     The timing point this snap line belongs to.
        /// </summary>
        public TimingPointInfo TimingPoint { get; }

        /// <summary>
        ///     The index of the timing point this snap line is.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="index"></param>
        public TimelineSnapLine(TimingPointInfo tp, int index)
        {
            TimingPoint = tp;
            Index = index;
        }

        public override void Draw(GameTime gameTime) =>  DrawToSpriteBatch();
    }
}