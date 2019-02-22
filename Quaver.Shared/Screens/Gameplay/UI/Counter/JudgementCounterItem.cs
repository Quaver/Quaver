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
using Quaver.Shared.Skinning;
using Wobble.Graphics;
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
        private int _judgementCount;
        public int JudgementCount
        {
            get => _judgementCount;
            set
            {
                // Ignore if the judgement count is the same as the incoming value.
                if (_judgementCount == value)
                    return;

                _judgementCount = value;

                // Change the color to its active one.
                Tint = SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].JudgeColors[Judgement];

                SpriteText.Text = value == 0
                    ? JudgementHelper.JudgementToShortName(Judgement)
                    : JudgementCount.ToString();

                // Don't animate it if the user doesn't want to.
                if (!ConfigManager.AnimateJudgementCounter.Value)
                    return;

                // Make the size of the display look more pressed.
                var skin = SkinManager.Skin.Keys[ParentDisplay.Screen.Map.Mode];

                Width = skin.JudgementCounterSize - skin.JudgementCounterSize / 4;
                Height = Width;
                X = -skin.JudgementCounterSize / 16f;
            }
        }

        /// <summary>
        ///     The sprite text for this given judgement.
        /// </summary>
        public SpriteTextBitmap SpriteText { get; }

        /// <summary>
        ///     The inactive color for this.
        /// </summary>
        private Color InactiveColor { get; }

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

            var skin = SkinManager.Skin.Keys[parentDisplay.Screen.Map.Mode];

            SpriteText = new SpriteTextBitmap(FontsBitmap.AllerRegular, JudgementHelper.JudgementToShortName(j))
            {
                Alignment = Alignment.MidCenter,
                Parent = this,
                Tint = skin.JudgementCounterFontColor,
                X = 0,
                FontSize = (int)(Width * 0.35f)
            };

            InactiveColor = color;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Make sure the color is always tweening down back to its inactive one.
            FadeToColor(InactiveColor, gameTime.ElapsedGameTime.TotalMilliseconds, 360);

            base.Update(gameTime);
        }
    }
}
