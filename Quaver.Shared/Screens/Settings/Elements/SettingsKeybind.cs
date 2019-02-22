/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Overlays.Chat.Components.Users;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Settings.Elements
{
    public class SettingsKeybind : SettingsItem
    {
        /// <summary>
        ///     Refeerence to the parent dialog screen.
        /// </summary>
        private SettingsDialog Dialog { get; }

        /// <summary>
        ///     The keybind that is being changed.
        /// </summary>
        private Bindable<Keys> Bindable { get; }

        /// <summary>
        ///     The button that displays the keybind and used to select a new one.
        /// </summary>
        private SelectableBorderedTextButton Button { get; }

        /// <summary>
        ///     Determines if we're currently waiting for the user to press a key to change the value
        /// </summary>
        private bool WaitingForKeyPress { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        /// <param name="bindable"></param>
        public SettingsKeybind(SettingsDialog dialog, string name, Bindable<Keys> bindable) : base(dialog, name)
        {
            Dialog = dialog;
            Bindable = bindable;

            Button = new SelectableBorderedTextButton(XnaKeyHelper.GetStringFromKey(bindable.Value), Color.White, Colors.MainAccent, false)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -20,
                UsePreviousSpriteBatchOptions = true,
                Border = {UsePreviousSpriteBatchOptions = true },
                Text = { UsePreviousSpriteBatchOptions = true }
            };

            Button.Height -= 6;

            Button.Clicked += (o, e) =>
            {
                Button.Text.Text = "Press Key";
                Button.Selected = true;
                WaitingForKeyPress = true;
            };

            Button.ClickedOutside += (o, e) =>
            {
                if (!dialog.IsOnTop)
                    return;

                var key = XnaKeyHelper.GetStringFromKey(Bindable.Value);

                if (Button.Text.Text != key)
                    Button.Text.Text = key;

                Button.Selected = false;
                WaitingForKeyPress = false;
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (WaitingForKeyPress && Dialog.IsOnTop)
            {
                var keys = KeyboardManager.CurrentState.GetPressedKeys();

                if (keys.Length != 0)
                {
                    if (KeyboardManager.IsUniqueKeyPress(keys.First()))
                    {
                        Bindable.Value = keys.First();

                        var key = XnaKeyHelper.GetStringFromKey(keys.First());

                        if (Button.Text.Text != key)
                            Button.Text.Text = key;

                        if (keys.First() == Keys.Escape)
                            Dialog.PreventExitOnEscapeKeybindPress = true;

                        Button.Selected = false;
                        WaitingForKeyPress = false;
                    }
                }
            }
            else
            {
                // Important. Makes it so the dialog doesn't close when the user presses escape as a keybind.
                Dialog.PreventExitOnEscapeKeybindPress = false;
            }

            base.Update(gameTime);
        }
    }
}
