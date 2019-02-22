/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Screens.Menu.UI.Navigation.User;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Settings.Elements
{
    public class SettingsKeybindMultiple : SettingsItem
    {
        /// <summary>
        ///     The binded keybinds that'll be changed
        /// </summary>
        private List<Bindable<Keys>> Keybinds { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        /// <param name="keybinds"></param>
        public SettingsKeybindMultiple(SettingsDialog dialog, string name, List<Bindable<Keys>> keybinds) : base(dialog, name)
        {
            Keybinds = keybinds;

            var btn = new BorderedTextButton("Change", Color.White)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -20,
                UsePreviousSpriteBatchOptions = true,
                Text = { UsePreviousSpriteBatchOptions = true }
            };

            btn.Clicked += (sender, args) => DialogManager.Show(new SettingsKeybindMultipleDialog(keybinds));
            btn.Height -= 6;
        }
    }
}
