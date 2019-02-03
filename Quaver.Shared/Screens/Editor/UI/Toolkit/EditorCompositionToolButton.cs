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
        private EditorCompositionTool Tool { get; }

        /// <summary>
        /// </summary>
        /// <param name="tool"></param>
        public EditorCompositionToolButton(EditorCompositionTool tool) : base(UserInterface.BlankBox)
        {
            Tool = tool;
            Size = new ScalableVector2(170, 38);

            switch (Tool)
            {
                case EditorCompositionTool.Select:
                    Image = UserInterface.EditorToolSelect;
                    break;
                case EditorCompositionTool.Note:
                    Image = UserInterface.EditorToolNote;
                    break;
                case EditorCompositionTool.LongNote:
                    Image = UserInterface.EditorToolLongNote;
                    break;
                case EditorCompositionTool.Mine:
                    Image = UserInterface.EditorToolMine;
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
                targetAlpha = 1;
            else if (IsHovered)
                targetAlpha = 0.85f;
            else
                targetAlpha = 0.65f;

            Alpha = MathHelper.Lerp(Alpha, targetAlpha, (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 60, 1));
            base.Update(gameTime);
        }
    }
}
