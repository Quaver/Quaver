using Quaver.Screens.Gameplay.Rulesets.Keys.HitObjects;
using System;
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
        ///     Track Position of this Timing Line
        /// </summary>
        public long TrackPosition { get; private set; }

        /// <summary>
        ///     Offset
        /// </summary>
        public static float GlobalTrackOffset { get; set; }

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
        /// <param name="offset"></param>
        public void UpdateSpritePosition(long offset)
        {
            var speed = GameplayRulesetKeys.IsDownscroll ? -HitObjectManagerKeys.ScrollSpeed : HitObjectManagerKeys.ScrollSpeed;
            TrackPosition = Info.TrackOffset - offset;
            Y = (TrackPosition * speed / HitObjectManagerKeys.TrackRounding) + GlobalTrackOffset;
            Console.WriteLine("offset: " + Info.TrackOffset  + ", position: " + TrackPosition + ", manager: " + offset);
        }
    }
}
