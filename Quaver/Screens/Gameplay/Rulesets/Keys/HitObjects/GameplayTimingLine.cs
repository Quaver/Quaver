using Quaver.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Screens.Gameplay.Rulesets.Keys.Playfield;
using System;
using System.Collections.Generic;
using System.Text;
using Wobble.Graphics.Sprites;

namespace Quaver.Screens.Gameplay.Rulesets.Keys.HitObjects
{
    public class GameplayTimingLine
    {
        /// <summary>
        ///     Reference to the actual playfield.
        /// </summary>
        private GameplayPlayfieldKeys Playfield { get; set; }

        private float StartTime { get; set; }

        private GameplayRulesetKeys Ruleset { get; set; }

        private Sprite TimingLineSprite { get; set; }

        public GameplayTimingLine(GameplayRulesetKeys ruleset) => Ruleset = ruleset;

        public void InitializeSprite(IGameplayPlayfield playfield, float startTime)
        {
            Playfield = (GameplayPlayfieldKeys)playfield;
            StartTime = startTime;
            /*
            TimingLineSprite = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(PositionX, PositionY),
                SpriteEffect = Effects,
                Image = GetHitObjectTexture(),
            };*/
        }

        public void Destroy()
        {
            
        }
    }
}
