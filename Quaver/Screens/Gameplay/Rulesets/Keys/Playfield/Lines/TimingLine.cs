using Quaver.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Screens.Gameplay.Rulesets.Keys.Playfield.Lines
{
    public class TimingLine : Sprite
    {
        /// <summary>
        ///     Reference to the Keys ruleset.
        /// </summary>
        private GameplayRulesetKeys Ruleset { get; }

        /// <summary>
        ///     Timing Line position and time information
        /// </summary>
        public TimingLineInfo Info { get; set; }

        /// <summary>
        ///     Track offset of the current Timing Line
        /// </summary>
        public float TrackOffset { get; private set; }

        /// <summary>
        ///     Position of the current Timing Line
        /// </summary>
        public float PositionY { get; private set; }

        /// <summary>
        ///     Offset
        /// </summary>
        public static float GlobalYOffset { get; set; } = 0;

        /// <inheritdoc />
        /// <summary>
        ///     Creates and initializes a new Timing Line Object
        /// </summary>
        /// <param name="ruleset"></param>
        /// <param name="info"></param>
        public TimingLine(GameplayRulesetKeys ruleset, TimingLineInfo info)
        {
            var playfield = (GameplayPlayfieldKeys)ruleset.Playfield;
            Ruleset = ruleset;
            Info = info;

            // Initialize Sprite
            Alignment = Alignment.TopLeft;
            Width = playfield.Width;
            Height = 2;
            Parent = playfield.Stage.TimingLineContainer;
        }

        /// <summary>
        ///     Update the current Timing Line Sprite position
        /// </summary>
        /// <param name="trackPosition"></param>
        public void UpdateSpritePosition(long trackPosition)
        {
            // Calculate Sprite Position
            var manager = (HitObjectManagerKeys) Ruleset.HitObjectManager;

            var speed = GameplayRulesetKeys.IsDownscroll ? -HitObjectManagerKeys.ScrollSpeed : HitObjectManagerKeys.ScrollSpeed;
            TrackOffset = Info.TrackOffset - trackPosition;
            PositionY = manager.HitPositionOffset + TrackOffset * speed + GlobalYOffset;

            // Update Sprite Position
            Y = PositionY;
        }
    }
}
