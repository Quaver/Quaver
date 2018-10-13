using Quaver.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Quaver.Screens.Gameplay.Rulesets.Keys.Playfield;
using System;
using System.Collections.Generic;
using System.Text;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Screens.Gameplay.Rulesets.Keys.TimingLines
{
    public class TimingLineObject
    {
        /// <summary>
        ///     Reference to the Keys ruleset.
        /// </summary>
        private GameplayRulesetKeys Ruleset { get; set; }

        /// <summary>
        ///     Timing Line position and time information
        /// </summary>
        public TimingLineInfo Info { get; set; }

        /// <summary>
        ///     The Timing Line Sprite
        /// </summary>
        private Sprite TimingLineSprite { get; set; }

        /// <summary>
        ///     Position of the current Timing Line
        /// </summary>
        public float PositionY { get; private set; }

        /// <summary>
        ///     Creates and initializes a new Timing Line Object
        /// </summary>
        /// <param name="ruleset"></param>
        /// <param name="info"></param>
        public TimingLineObject(GameplayRulesetKeys ruleset, TimingLineInfo info)
        {
            Ruleset = ruleset;
            Info = info;

            // todo: implement sprite position, size, and parent
            TimingLineSprite = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(0, PositionY)
            };
        }

        /// <summary>
        ///     Update the current Timing Line Sprite position
        /// </summary>
        /// <param name="trackPosition"></param>
        public void UpdateSpritePosition(long trackPosition)
        {
            // Calculate Sprite Position
            var manager = (HitObjectManagerKeys)Ruleset.HitObjectManager;
            var speed = GameplayRulesetKeys.IsDownscroll ? -HitObjectManagerKeys.ScrollSpeed : HitObjectManagerKeys.ScrollSpeed;
            PositionY = (float)(manager.HitPositionOffset + (Info.OffsetYFromReceptor - trackPosition) * speed);

            // Update Sprite Position
            TimingLineSprite.Y = PositionY;
        }

        /// <summary>
        ///     Destroy this object
        /// </summary>
        public void Destroy() => TimingLineSprite.Destroy();
    }
}
