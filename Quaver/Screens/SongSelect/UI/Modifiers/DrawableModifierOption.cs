using System;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Graphics;
using Quaver.Helpers;
using Quaver.Screens.Select.UI.Mods;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Screens.SongSelect.UI.Modifiers
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
            : base(UserInterface.BlankBox, BitmapFonts.Exo2Regular, text, 13, clickAction)
        {
            Modifier = modifier;
            Parent = Modifier;
            Tint = Colors.MainAccent;
            Alpha = 0.75f;
            UsePreviousSpriteBatchOptions = true;
            Text.UsePreviousSpriteBatchOptions = true;
            Size = new ScalableVector2(60, Modifier.Height * 0.75f);

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