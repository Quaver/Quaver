/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Select.UI.Modifiers
{
    public class DrawableModifierOption : TextButton
    {
        /// <summary>
        ///     Reference to the parent modifier
        /// </summary>
        private DrawableModifier Modifier { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="modifier"></param>
        /// <param name="text"></param>
        /// <param name="clickAction"></param>
        public DrawableModifierOption(DrawableModifier modifier, string text, EventHandler clickAction)
            : base(UserInterface.BlankBox, Fonts.Exo2SemiBold, text, 13, clickAction)
        {
            Modifier = modifier;
            Parent = Modifier;
            Tint = Colors.MainAccent;
            Alpha = 0.75f;
            UsePreviousSpriteBatchOptions = true;
            Text.UsePreviousSpriteBatchOptions = true;
            Size = new ScalableVector2(Width + 20, Modifier.Height * 0.75f);

            Deselect();
        }

        /// <summary>
        ///     Selects the button
        /// </summary>
        public void Select() => Alpha = 0.75f;

        /// <summary>
        ///     Deselects the button.
        /// </summary>
        public void Deselect() => Alpha = 0;
    }
}
