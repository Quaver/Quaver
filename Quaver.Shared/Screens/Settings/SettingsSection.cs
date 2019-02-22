/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Settings
{
    public class SettingsSection
    {
        /// <summary>
        ///     Reference to the parent dialog screen
        /// </summary>
        private SettingsDialog Dialog { get; }

        /// <summary>
        ///     The specified icon for the section
        /// </summary>
        public Texture2D Icon { get; }

        /// <summary>
        ///     The name of the given options section
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The button to activate the section
        /// </summary>
        public SettingsSectionButton Button { get; private set; }

        /// <summary>
        ///     The container that holds all of the section's items.
        /// </summary>
        public ScrollContainer Container { get; private set; }

        /// <summary>
        ///     All of the options items displayed in the section.
        /// </summary>
        public List<Drawable> Items { get; }

        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="icon"></param>
        /// <param name="name"></param>
        /// <param name="items"></param>
        public SettingsSection(SettingsDialog dialog, Texture2D icon, string name, List<Drawable> items)
        {
            Items = items;
            Dialog = dialog;
            Icon = icon;
            Name = name;

            CreateSectionButton();
            CreateContainer();
        }

        /// <summary>
        ///     Creates the button the activate the options section.
        /// </summary>
        private void CreateSectionButton() => Button = new SettingsSectionButton(Dialog, this, Icon, Name);

        /// <summary>
        ///     Creates the ScrollContainer that holds all the settings items.
        /// </summary>
        private void CreateContainer()
        {
            var size = new ScalableVector2(Dialog.ContentContainer.Width - Dialog.DividerLine.X - 20, Dialog.DividerLine.Height);

            Container = new ScrollContainer(size, size)
            {
                Alignment = Alignment.MidLeft,
                X = Dialog.DividerLine.X + 10,
                Alpha = 0,
                InputEnabled = true,
                DestroyIfParentIsNull = false
            };

            Container.Scrollbar.Tint = Color.White;
            Container.Scrollbar.Width = 5;
            Container.Scrollbar.X += 8;
            Container.ScrollSpeed = 150;
            Container.EasingType = Easing.OutQuint;
            Container.TimeToCompleteScroll = 1500;

            var totalHeight = 0f;

            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                Container.AddContainedDrawable(item);
                totalHeight += item.Height;

                if (i == 0)
                    continue;

                const int spacing = 10;

                item.Y = Items[i - 1].Y + Items[i - 1].Height + spacing;
                totalHeight += spacing;
            }

            if (Container.ContentContainer.Height < totalHeight)
                Container.ContentContainer.Height = totalHeight;
        }
    }
}
