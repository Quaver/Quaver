/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using Quaver.Shared.Config;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield.Lines
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
        ///     Track Position of this Timing Line. >0 = this object hasnt passed receptors.
        /// </summary>
        public float CurrentTrackPosition { get; private set; }

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
            CurrentTrackPosition = offset - Info.TrackOffset;
            Y = GlobalTrackOffset + (CurrentTrackPosition * (GameplayRulesetKeys.ScrollDirection.Equals(ScrollDirection.DownScroll) ? HitObjectManagerKeys.ScrollSpeed : -HitObjectManagerKeys.ScrollSpeed) / HitObjectManagerKeys.TrackRounding);
        }
    }
}
