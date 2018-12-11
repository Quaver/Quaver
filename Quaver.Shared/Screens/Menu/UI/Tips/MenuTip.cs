/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Menu.UI.Tips
{
    public class MenuTip : ScrollContainer
    {
        /// <summary>
        ///     The bolded text that says "Tip:"
        /// </summary>
        public SpriteText TextTip { get; private set; }

        /// <summary>
        ///     The actual content of the tip.
        /// </summary>
        public SpriteText TextTipContent { get; private set; }

        /// <summary>
        ///     The amount of time this tip has been active.
        /// </summary>
        private double TimeTipActive { get; set; }

        /// <summary>
        ///     Dictates if the Animation is going forward or backward.
        /// </summary>
        private bool IsGoingForward { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public MenuTip() : base(new ScalableVector2(0, 45), new ScalableVector2(0, 45))
        {
            Tint = Color.Black;
            Alpha = 0.25f;

            CreateTextTip();
            CreateTextTipContent();
            UpdateTip("If our knees were bent the other way, what would chairs look like?");
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            //
            if (Animations.Count == 0)
            {
                TimeTipActive += dt;

                if (!IsGoingForward)
                {
                    UpdateTip($"안녕하세요");
                }

                if (TimeTipActive >= 10000 && IsGoingForward)
                {
                    Animations.Add(new Animation(AnimationProperty.Width, Easing.Linear, Width, 0, 400));
                    TimeTipActive = 0;
                    IsGoingForward = false;
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        ///     Creates the text that says "TIP:"
        /// </summary>
        private void CreateTextTip()
        {
            TextTip = new SpriteText(BitmapFonts.Exo2BoldItalic, "TIP:", 13, false)
            {
                Alignment = Alignment.MidLeft,
                X = 5,
            };

            AddContainedDrawable(TextTip);
        }

        /// <summary>
        ///     Creates the text tip content.
        /// </summary>
        private void CreateTextTipContent()
        {
            TextTipContent = new SpriteText(BitmapFonts.Exo2SemiBold, " ", 12, false)
            {
                Alignment = Alignment.MidLeft
            };

            AddContainedDrawable(TextTipContent);
        }
        /// <summary>
        ///     Updates the tip with new text and performs an animation.
        /// </summary>
        /// <param name="tip"></param>
        public void UpdateTip(string tip)
        {
            TextTipContent.Text = tip;
            TextTipContent.X = TextTip.X + TextTip.Width + 1;

            ContentContainer.Size = new ScalableVector2(TextTip.Width + TextTipContent.Width + 10, 45);

            Animations.Add(new Animation(AnimationProperty.Width, Easing.Linear,
                Width, ContentContainer.Width, 400));

            IsGoingForward = true;
        }
    }
}
