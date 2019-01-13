/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2019 Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.Timeline
{
    public class TimelineTickLine : Sprite
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

        /// <summary>
        ///     The text that displays the measure in the song.
        /// </summary>
        private SpriteTextBitmap TextMeasure { get; set; }

        /// <summary>
        ///     Determines if this line is for a measure.
        /// </summary>
        private bool IsMeasureLine => Index / Container.Ruleset.Screen.BeatSnap.Value % 4 == 0
                                      && Index % Container.Ruleset.Screen.BeatSnap.Value == 0 && Time >= TimingPoint.StartTime;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="tp"></param>
        /// <param name="time"></param>
        /// <param name="index"></param>
        /// <param name="measureCount"></param>
        public TimelineTickLine(EditorScrollContainerKeys container, TimingPointInfo tp, float time, int index, int measureCount)
        {
            Container = container;
            TimingPoint = tp;
            Index = index;
            Time = time;

            if (!IsMeasureLine)
                return;

            TextMeasure = new SpriteTextBitmap(FontsBitmap.MuliBold, measureCount.ToString())
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                FontSize = 28
            };

            TextMeasure.X = -TextMeasure.Width - 15;
            Y = -2;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            DrawToSpriteBatch();

            if (!IsMeasureLine)
                return;

            TextMeasure.DrawToSpriteBatch();
        }

        /// <summary>
        ///     Checks if the timing line is on-screen.
        /// </summary>
        /// <returns></returns>
        public bool CheckIfOnScreen() => Time * Container.TrackSpeed >= Container.TrackPositionY - Container.Height &&
                                         Time * Container.TrackSpeed <= Container.TrackPositionY + Container.Height;
    }
}