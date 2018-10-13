using Quaver.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Screens.Gameplay.Rulesets.Keys.Playfield;
using System;
using System.Collections.Generic;
using System.Text;
using Wobble.Graphics.Sprites;

namespace Quaver.Screens.Gameplay.Rulesets.Keys.TimingLines
{
    public class TimingLineObject
    {
        /// <summary>
        ///     Reference to the actual playfield.
        /// </summary>
        private GameplayPlayfieldKeys Playfield { get; set; }

        private GameplayRulesetKeys Ruleset { get; set; }

        private TimingLineInfo Info { get; set; }

        private Sprite TimingLineSprite { get; set; }

        public TimingLineObject(GameplayRulesetKeys ruleset) => Ruleset = ruleset;

        public void InitializeSprite(IGameplayPlayfield playfield, TimingLineInfo info)
        {
            Playfield = (GameplayPlayfieldKeys)playfield;
            Info = info;
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
