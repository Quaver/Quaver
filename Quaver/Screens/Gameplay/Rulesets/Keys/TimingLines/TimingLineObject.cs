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
        private GameplayRulesetKeys Ruleset { get; set; }

        public TimingLineInfo Info { get; set; }

        private Sprite TimingLineSprite { get; set; }

        public float PositionY { get; private set; }

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

        public void UpdateSpritePosition(long trackPosition)
        {
            // Calculate Sprite Position
            var manager = (HitObjectManagerKeys)Ruleset.HitObjectManager;
            var speed = GameplayRulesetKeys.IsDownscroll ? -HitObjectManagerKeys.ScrollSpeed : HitObjectManagerKeys.ScrollSpeed;
            PositionY = (float)(manager.HitPositionOffset + (Info.OffsetYFromReceptor - trackPosition) * speed);

            // Update Sprite Position
            TimingLineSprite.Y = PositionY;
        }

        public void Destroy()
        {
            TimingLineSprite.Destroy();
        }
    }
}
