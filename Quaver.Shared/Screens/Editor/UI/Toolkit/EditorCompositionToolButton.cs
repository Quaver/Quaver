/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Drawing;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Editor.UI.Toolkit
{
    public class EditorCompositionToolButton : ImageButton
    {
        /// <summary>
        /// </summary>
        private EditorCompositionToolbox Toolbox { get; }

        /// <summary>
        /// </summary>
        private EditorCompositionTool Tool { get; }

        /// <summary>
        /// </summary>
        private Sprite Icon { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap Name { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="toolbox"></param>
        /// <param name="tool"></param>
        public EditorCompositionToolButton(EditorCompositionToolbox toolbox, EditorCompositionTool tool) : base(UserInterface.BlankBox)
        {
            Toolbox = toolbox;
            Tool = tool;
            Size = new ScalableVector2(Toolbox.Width, 39);

            Icon = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 12,
                Size = new ScalableVector2(20, 20)
            };

            Name = new SpriteTextBitmap(FontsBitmap.AllerRegular, "")
            {
                Parent = this,
                FontSize = 16,
                Alignment = Alignment.MidLeft,
                X = Icon.X + Icon.Width + 10,
            };

            switch (Tool)
            {
                case EditorCompositionTool.Select:
                    Icon.Image = UserInterface.EditorToolSelect;
                    Name.Text = "Select";
                    break;
                case EditorCompositionTool.Note:
                    Icon.Image = FontAwesome.Get(FontAwesomeIcon.fa_arrow_pointing_to_left);
                    Name.Text = "Note";
                    break;
                case EditorCompositionTool.LongNote:
                    Icon.Image = FontAwesome.Get(FontAwesomeIcon.fa_rounded_black_square_shape);
                    Icon.Size = new ScalableVector2(16, 20);
                    Icon.X += 2;
                    Name.Text = "Long Note";
                    break;
                case EditorCompositionTool.Mine:
                    Icon.Image = FontAwesome.Get(FontAwesomeIcon.fa_ban_circle_symbol);
                    Name.Text = "Mine";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Clicked += (sender, args) =>
            {
                var game = GameBase.Game as QuaverGame;
                var screen = game?.CurrentScreen as EditorScreen;

                if (screen.Ruleset is EditorRulesetKeys keys)
                {
                    keys.CompositionTool.Value = Tool;
                    return;
                }

                throw new NotImplementedException("Composition tool click action not implemented for this ruleset");
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var game = GameBase.Game as QuaverGame;
            var screen = game?.CurrentScreen as EditorScreen;
            var ruleset = screen?.Ruleset as EditorRulesetKeys;

            float targetAlpha;

            if (ruleset?.CompositionTool.Value == Tool)
                targetAlpha = 0.45f;
            else if (IsHovered)
                targetAlpha = 0.25f;
            else
                targetAlpha = 0f;

            Alpha = MathHelper.Lerp(Alpha, targetAlpha, (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 60, 1));
            base.Update(gameTime);
        }
    }
}
