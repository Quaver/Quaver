using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Xna.Framework;
using Quaver.Resources;
using Quaver.Graphics;
using Quaver.Screens.Options.Keybindings;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Window;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Quaver.Screens.Options
{
    public class KeybindDialog : DialogScreen
    {
        /// <summary>
        ///    The keys that are to be edited.
        /// </summary>
        private List<KeybindingOptionStore> Keybinds { get; }

        /// <summary>
        ///     The index of the keybind that is currently being changed.
        /// </summary>
        private int CurrentChangingKeybind { get; set; }

        /// <summary>
        ///     Header SpriteText that explains what to do.
        /// </summary>
        private SpriteText Header { get; set;  }

        /// <summary>
        ///     The name of the current keybind.
        /// </summary>
        private SpriteText BindingName { get; set; }

        /// <summary>
        ///     The list of keybind sprites that display the current key.
        /// </summary>
        private List<KeybindSprite> KeybindSprites { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="backgroundAlpha"></param>
        public KeybindDialog(List<KeybindingOptionStore> keys, float backgroundAlpha): base(backgroundAlpha)
        {
            Keybinds = keys;
            CreateContent();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            Header = new SpriteText(BitmapFonts.Exo2Regular, "Press any key to change the binding for it.", 22)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
            };

            Header.Y = Header.Height + 100;

            BindingName = new SpriteText(BitmapFonts.Exo2Regular, Keybinds[CurrentChangingKeybind].Name, 22)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Tint = Colors.MainAccent
            };

            BindingName.Y = Header.Y + BindingName.Height + 60;

            // Create list of keybind sprites.
            KeybindSprites = new List<KeybindSprite>();

            for (var i = 0; i < Keybinds.Count; i++)
            {
                var keybindSprite = new KeybindSprite(Keybinds[i].Key)
                {
                    Parent = this,
                    Alignment = Alignment.TopCenter,
                    Y = BindingName.Y + 60
                };

                if (i == 0)
                    keybindSprite.Selected = true;

                keybindSprite.X = i * (keybindSprite.Width + 20);
                KeybindSprites.Add(keybindSprite);
            }
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

            Keybinds[CurrentChangingKeybind].Key.Value = firstKey;

            if (CurrentChangingKeybind + 1 < Keybinds.Count)
            {
                KeybindSprites[CurrentChangingKeybind].Selected = false;

                CurrentChangingKeybind++;
                BindingName.Text = Keybinds[CurrentChangingKeybind].Name;
                KeybindSprites[CurrentChangingKeybind].Selected = true;
            }
            else
            {
                DialogManager.Dismiss();
            }
        }
    }
}