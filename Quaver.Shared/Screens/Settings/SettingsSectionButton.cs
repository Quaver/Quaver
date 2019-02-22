/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Online.Playercard;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Settings
{
    public class SettingsSectionButton : Button
    {
        /// <summary>
        ///     Reference to the parent dialog
        /// </summary>
        private SettingsDialog Dialog { get; }

        /// <summary>
        ///     Reference to the section this button belongs to
        /// </summary>
        private SettingsSection Section { get; }

        /// <summary>
        ///     Determines if the button is selected.
        /// </summary>
        public bool IsSelected { get; private set; }

        /// <summary>
        ///     Displays the icon for the secion
        /// </summary>
        private Sprite Icon { get; }

        /// <summary>
        ///     Displays the text of the button
        /// </summary>
        private SpriteText Text { get; }

        /// <summary>
        ///     For aesthetics when the button is selected
        /// </summary>
        private Sprite Flag { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="section"></param>
        /// <param name="icon"></param>
        /// <param name="name"></param>
        public SettingsSectionButton(SettingsDialog dialog, SettingsSection section, Texture2D icon, string name)
        {
            Dialog = dialog;
            Section = section;
            Size = new ScalableVector2(206, 35);
            Alpha = 0.65f;

            Icon = new Sprite
            {
                Parent = this,
                Image = icon,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(18, 18)
            };

            Text = new SpriteText(Fonts.Exo2SemiBold, name, 13)
            {
                Parent = Icon,
                Alignment = Alignment.MidLeft,
                X = Icon.Width + 4
            };

            Icon.X -= Icon.Width / 2f + Text.Width / 2f + 2;

            Flag = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(4, Height),
                Tint = Color.Yellow
            };

            Clicked += (o, e) =>
            {
                if (Dialog.SelectedSection == Section)
                    return;

                Dialog.SwitchSelected(Section);
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (!IsSelected)
            {
                FadeToColor(IsHovered ? ColorHelper.HexToColor("#1e3c72") : ColorHelper.HexToColor("#1e1e1e"), Easing.Linear, 50);
            }

            base.Update(gameTime);
        }

        /// <summary>
        ///     Displays the button as selected
        /// </summary>
        public void DisplayAsSelected()
        {
            IsSelected = true;
            Flag.Visible = true;
            Tint = ColorHelper.HexToColor("#1e3c72");
            Alpha = 1;
        }

        /// <summary>
        ///     Displays the button as deselected
        /// </summary>
        public void DisplayAsDeselected()
        {
            IsSelected = false;
            Flag.Visible = false;
            Tint = ColorHelper.HexToColor("#1e1e1e");
            Alpha = 1;
            Icon.Tint = Color.White;
            Text.Tint = Color.White;
        }
    }
}
