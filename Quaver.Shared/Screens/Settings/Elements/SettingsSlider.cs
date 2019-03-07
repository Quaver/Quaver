/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Form;

namespace Quaver.Shared.Screens.Settings.Elements
{
    public class SettingsSlider : SettingsItem
    {
        /// <summary>
        ///     The value that the slider is binded to.
        /// </summary>
        private Bindable<int> Bindable { get; }

        /// <summary>
        ///     Displays the value of the bindable
        /// </summary>
        private SpriteText Value { get; }

        /// <summary>
        ///     Converts the bindable value to string for displaying.
        /// </summary>
        private Func<int, string> Display { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        /// <param name="bindable"></param>
        public SettingsSlider(SettingsDialog dialog, string name, BindableInt bindable, Func<int, string> display = null) : base(dialog, name)
        {
            Display = display ?? (x => x.ToString());
            Bindable = bindable;
            bindable.ValueChanged += OnValueChanged;

            Value = new SpriteText(Fonts.Exo2Medium, Display(bindable.Value), 13)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -30,
                UsePreviousSpriteBatchOptions = true
            };

            var slider = new Slider(bindable, Vector2.One, FontAwesome.Get(FontAwesomeIcon.fa_circle))
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -110,
                UsePreviousSpriteBatchOptions = true,
                Width = 330,
                Height = 2,
                ProgressBall =
                {
                    UsePreviousSpriteBatchOptions = true
                }
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            Bindable.ValueChanged -= OnValueChanged;

            base.Destroy();
        }
        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnValueChanged(object sender, BindableValueChangedEventArgs<int> e) => Value.Text = Display(e.Value);
    }
}
