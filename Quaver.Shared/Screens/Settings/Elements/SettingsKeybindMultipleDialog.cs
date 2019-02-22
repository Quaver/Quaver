/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Window;

namespace Quaver.Shared.Screens.Settings.Elements
{
    public class SettingsKeybindMultipleDialog : DialogScreen
    {
        /// <summary>
        ///     The list of bindable keys
        /// </summary>
        private List<Bindable<Keys>> Keybinds { get; }

        /// <summary>
        /// </summary>
        private Sprite ContainingBox { get; set; }

        /// <summary>
        /// </summary>
        private Line TopLine { get; set; }

        /// <summary>
        ///     The list of keybind sprites that display the current key.
        /// </summary>
        private List<SettingsKeybindSprite> KeybindSprites { get; set; }

        /// <summary>
        ///     The index of the keybind that is currently being changed.
        /// </summary>
        private int CurrentChangingKeybind { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public SettingsKeybindMultipleDialog(List<Bindable<Keys>> keybinds) : base(0.75f)
        {
            Keybinds = keybinds;
            CreateContent();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            CreateContainingBox();
            CreateTopLine();
            CreateHeader();
            CreateKeybindSprites();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            // Get the current pressed keys
            var pressedKeys = KeyboardManager.CurrentState.GetPressedKeys();

            // Don't bother continuing if the user hasn't pressed any keys.
            if (pressedKeys.Length == 0)
                return;

            // Grab only the first key that was pressed.
            var firstKey = pressedKeys[0];

            // If the key wasn't uniquely pressed then don't handle anything else.
            if (!KeyboardManager.IsUniqueKeyPress(firstKey))
                return;

            KeybindSprites[CurrentChangingKeybind].Key.Value = firstKey;

            if (CurrentChangingKeybind + 1 < KeybindSprites.Count)
            {
                KeybindSprites[CurrentChangingKeybind].Selected = false;

                CurrentChangingKeybind++;
                KeybindSprites[CurrentChangingKeybind].Selected = true;
            }
            else
            {
                DialogManager.Dismiss();
            }
        }

        /// <summary>
        /// </summary>
        private void CreateContainingBox() => ContainingBox = new Sprite
        {
            Parent = Container,
            Size = new ScalableVector2(WindowManager.Width, 130),
            Alignment = Alignment.MidCenter,
            Tint = Color.Black,
            Alpha = 0.85f,
        };

        /// <summary>
        /// </summary>
        private void CreateTopLine() => TopLine = new Line(new Vector2(WindowManager.Width, ContainingBox.AbsolutePosition.Y),
            Colors.MainAccent, 2)
        {
            Parent = ContainingBox,
            Alpha = 0.75f,
            UsePreviousSpriteBatchOptions = true
        };

        /// <summary>
        ///     Creates the heading text
        /// </summary>
        private void CreateHeader()
        {
            var icon = new Sprite
            {
                Parent = ContainingBox,
                Alignment = Alignment.TopLeft,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_keyboard),
                Size = new ScalableVector2(24, 24),
                Y = 18
            };

            var header = new SpriteText(Fonts.Exo2SemiBold, "Press a key to change the binding", 16)
            {
                Parent = icon,
                Alignment = Alignment.MidLeft,
                X = icon.Width + 10
            };

            icon.X = WindowManager.Width / 2f - header.Width / 2f - 10 - icon.Width / 2f;
        }

        /// <summary>
        /// </summary>
        private void CreateKeybindSprites()
        {
            KeybindSprites = new List<SettingsKeybindSprite>();

            var totalWidth = 0f;

            for (var i = 0; i < Keybinds.Count; i++)
            {
                var keybindSprite = new SettingsKeybindSprite(Keybinds[i])
                {
                    Parent = ContainingBox,
                    Alignment = Alignment.TopCenter,
                };

                if (i == 0)
                {
                    totalWidth += keybindSprite.Width;
                    keybindSprite.Selected = true;
                }
                else
                {
                    keybindSprite.Parent = KeybindSprites.First();
                    keybindSprite.Alignment = Alignment.TopLeft;
                    keybindSprite.X = i * keybindSprite.Width + i * 20;
                    totalWidth += keybindSprite.Width + 20;
                }

                KeybindSprites.Add(keybindSprite);
            }

            KeybindSprites.First().Y = 58;
            KeybindSprites.First().X = -totalWidth / 2f + 20;
        }
    }
}
