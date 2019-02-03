/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Settings.Elements
{
    public class SettingsKeybindSprite : Sprite
    {
         /// <summary>
        ///     The binded key.
        /// </summary>
        public Bindable<Keys> Key { get; }

        /// <summary>
        ///     The text that displays the current keybind
        /// </summary>
        private SpriteText KeyText { get; }

        /// <summary>
        ///     If the keybind sprite is selected.
        /// </summary>
        public bool Selected { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        public SettingsKeybindSprite(Bindable<Keys> key)
        {
            Key = key;
            Image = UserInterface.BlankBox;
            Tint = Color.Transparent;
            AddBorder(Color.White, 2);
            Size = new ScalableVector2(54, 54);

            KeyText = new SpriteText(Fonts.Exo2Regular, XnaKeyHelper.GetStringFromKey(Key.Value), 13)
            {
                Parent = this,
                Alignment = Alignment.MidCenter
            };

            Key.ValueChanged += OnKeybindChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Border.FadeToColor(Selected ? Colors.MainAccent : Color.White, gameTime.ElapsedGameTime.TotalMilliseconds, 60);
            KeyText.FadeToColor(Selected ? Colors.MainAccent : Color.White, gameTime.ElapsedGameTime.TotalMilliseconds, 60);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            Key.ValueChanged -= OnKeybindChanged;
            base.Destroy();
        }

        /// <summary>
        ///     Updates the text when the key is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnKeybindChanged(object sender, BindableValueChangedEventArgs<Keys> args) => KeyText.Text = XnaKeyHelper.GetStringFromKey(args.Value);
    }
}
