/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.UI.Counter
{
    public class JudgementCounterItem : Sprite
    {
        /// <summary>
        ///     The parent judgement display that controls the rest of them.
        /// </summary>
        private JudgementCounter ParentDisplay { get; }

        /// <summary>
        ///     The actual judgement this represents.
        /// </summary>
        public Judgement Judgement { get; }

        /// <summary>
        ///     The current judgement count for this
        /// </summary>
        private int _judgementCount = -1;
        public int JudgementCount
        {
            get => _judgementCount;
            set
            {
                // Ignore if the judgement count is the same as the incoming value.
                if (_judgementCount == value && value != 0)
                    return;

                _judgementCount = value;

                var skin = SkinManager.Skin.Keys[ParentDisplay.Screen.Map.Mode];
                var color = skin.JudgeColors[Judgement];

                // Change the color to its active one, respecting the skin settings.
                this.Tint = color;

                if (skin.UseJudgementColorForNumbers)
                {
                    SpriteLabel.Tint = color * 2;
                    Counter.Tint = color * 2;
                }
                else
                {
                    SpriteLabel.Tint = skin.JudgementCounterFontColor;
                    Counter.Tint = skin.JudgementCounterFontColor;
                }

                this.Alpha = DefaultAlpha;

                // If the value is 0, we want to display the label.
                if (value == 0)
                {
                    SpriteLabel.Visible = true;
                    Counter.Visible = false;
                }
                else
                {
                    SpriteLabel.Visible = false;
                    Counter.Visible = true;
                    Counter.Value = value.ToString();
                }
            }
        }

        /// <summary>
        ///     The sprite text for this given judgement.
        /// </summary>
        public Sprite SpriteLabel { get; }

        /// <summary>
        ///     The number display for the judgement count.
        /// </summary>
        public NumberDisplay Counter { get; }

        private Color InactiveColor { get; }

        /// <summary>
        ///     The inactive color for the overlay.
        /// </summary>
        private Color InactiveOverlayColor { get; }

        private float DefaultAlpha { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="parentDisplay"></param>
        /// <param name="j"></param>
        /// <param name="color"></param>
        /// <param name="size"></param>
        public JudgementCounterItem(JudgementCounter parentDisplay, Judgement j, Color color, Vector2 size)
        {
            Judgement = j;
            ParentDisplay = parentDisplay;

            Size = new ScalableVector2(size.X, size.Y);
            DefaultAlpha = this.Alpha;
            Tint = color;

            var skin = SkinManager.Skin.Keys[parentDisplay.Screen.Map.Mode];

            // Create the label sprite.
            SpriteLabel = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Image = SkinManager.Skin.JudgementCounterNames.ContainsKey(j) ? SkinManager.Skin.JudgementCounterNames[j] : null,
                Tint = color,
            };

            if (SpriteLabel.Image != null)
            {
                SpriteLabel.Size = new ScalableVector2(SpriteLabel.Image.Width * skin.JudgementCounterNumberScale,
                    SpriteLabel.Image.Height * skin.JudgementCounterNumberScale);
            }
            else
            {
                SpriteLabel.Size = new ScalableVector2(size.X * skin.JudgementCounterNumberScale, size.Y * skin.JudgementCounterNumberScale);
            }

            // Create the counter display.
            Counter = new NumberDisplay(NumberDisplayType.Judgement, "0", new Vector2(skin.JudgementCounterNumberScale))
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Tint = color,
                Visible = false
            };

            // Re-assign tint logic from existing code (UseJudgementColorForNumbers check handled in setter mostly, but here for init)
            if (!skin.UseJudgementColorForNumbers)
            {
                Counter.Tint = skin.JudgementCounterFontColor;
                SpriteLabel.Tint = skin.JudgementCounterFontColor;
            }
            else
            {
                // Double color intensity if using judgement colors (as per original code logic, though applied to Tint)
                // Original was: Tint = skin.UseJudgementColorForNumbers ? color * 2 : skin.JudgementCounterFontColor
                // We apply it here.
                Counter.Tint = color * 2;
                SpriteLabel.Tint = color * 2;
            }

            InactiveColor = Counter.Tint;
            InactiveOverlayColor = Tint;

            // Set initial state
            JudgementCount = 0;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Make sure the color is always tweening down back to its inactive one.
            if (SkinManager.Skin.Keys[ParentDisplay.Screen.Map.Mode].JudgementCounterFadeToAlpha)
                this.FadeTo(0, Wobble.Graphics.Animations.Easing.Linear, 360);
            else
            {
                // Fade the overlay itself.
                this.FadeToColor(InactiveOverlayColor, dt, 360);

                // We need to fade the color of whatever is visible
                if (SpriteLabel.Visible)
                    SpriteLabel.FadeToColor(InactiveColor, dt, 360);

                if (Counter.Visible)
                    Counter.FadeToColor(InactiveColor, dt, 360);
            }

            base.Update(gameTime);
        }
    }
}
