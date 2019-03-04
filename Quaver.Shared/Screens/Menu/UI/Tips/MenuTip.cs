/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
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

        /// <summary>
        ///     Random number generator for selecting tips.
        /// </summary>
        private Random RNG { get; }

        /// <summary>
        ///     List of menu tips to be randomly displayed.
        ///     Eventually we'll want to move this out to a localized file, but it's fine for now.
        /// </summary>
        private List<string> Tips { get; } = new List<string>
        {
            "You can import new maps by dragging them into the game window",
            "You can watch a replay by dragging a replay file into the window",
            "Pressing CTRL and +/- during song select will change the audio rate",
            "Any elements not covered by a custom skin will resort to the selected default skin",
            "You can quickly access the modifier menu by pressing F1 during song select",
            $"Pressing {ConfigManager.KeyToggleOverlay.Value} will open/close the in-game chat",
            "Mapsets are a grouping of individual map difficulties",
            "You can create a new mapset by dragging a mp3 file inside of the game window.",
            "Pausing during gameplay will invalidate your score from online ranking",
            "You can send replays to a friend by exporting them in the results screen",
            "You can send mapsets to a friend by exporting them in the song select screen",
            "If our knees were bent the other way, what do you think chairs would look like?",
            "This game is still incomplete. Some features have not been developed yet",
            "You can report any bugs you find by clicking the button in the top right",
            "Accuracy is a large part of Quaver. Make sure you're on beat!",
            $"Your audio offset can be changed in-game by pressing - or + during gameplay",
            $"You can change your scroll speed during gameplay by pressing {ConfigManager.KeyDecreaseScrollSpeed.Value} or {ConfigManager.KeyDecreaseScrollSpeed.Value}",
        };

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public MenuTip() : base(new ScalableVector2(0, 45), new ScalableVector2(0, 45))
        {
            RNG = new Random();
            Tint = Color.Black;
            Alpha = 0.25f;

            CreateTextTip();
            CreateTextTipContent();
            UpdateTip(Tips[SelectRandomTip()]);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            if (Animations.Count == 0)
            {
                TimeTipActive += dt;

                if (!IsGoingForward)
                {
                    UpdateTip(Tips[SelectRandomTip()]);
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
            TextTip = new SpriteText(Fonts.Exo2BoldItalic, "TIP:", 13)
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
            TextTipContent = new SpriteText(Fonts.Exo2SemiBold, " ", 12)
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

        /// <summary>
        ///     Gets the index of a random tip
        ///     <see cref="Tips"/>
        /// </summary>
        /// <returns></returns>
        private int SelectRandomTip() => RNG.Next(0, Tips.Count - 1);
    }
}
