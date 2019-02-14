/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Editor.UI.Dialogs.Metadata;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Screens.Menu.UI.Navigation.User;
using Steamworks;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.SV
{
    public class EditorScrollVelocityChanger : Sprite
    {
        /// <summary>
        /// </summary>
        private EditorScrollVelocityDialog Dialog { get; }

        /// <summary>
        /// </summary>
        public Sprite HeaderBackground { get; private set; }

        /// <summary>
        /// </summary>
        public Sprite FooterBackground { get; private set; }

        /// <summary>
        /// </summary>
        private Button OkButton { get; set; }

        /// <summary>
        /// </summary>
        private Button CancelButton { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        public EditorScrollVelocityChanger(EditorScrollVelocityDialog dialog)
        {
            Dialog = dialog;
            Size = new ScalableVector2(400, 502);
            Image = UserInterface.EditorEditScrollVelocities;

            CreateHeader();
            CreateFooter();
            CreateOkButton();
            CreateCancelButton();;
        }

        /// <summary>
        /// </summary>
        private void CreateHeader()
        {
            HeaderBackground = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(Width, 38),
                Alpha = 0
            };

            var removeButton = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_times), (sender, args) =>
            {
            })
            {
                Parent = HeaderBackground,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(20, 20),
                Tint = Color.Crimson
            };

            removeButton.X -= removeButton.Width / 2f;

            var addButton = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_plus_black_symbol), (sender, args) =>
            {
            })
            {
                Parent = HeaderBackground,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(20, 20),
                Tint = Color.LimeGreen,
                X = removeButton.X - removeButton.Width - 12
            };
        }

        /// <summary>
        /// </summary>
        private void CreateFooter() => FooterBackground = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(Width, 38),
            Alignment = Alignment.BotLeft,
            Alpha = 0
        };

        /// <summary>
        /// </summary>
        private void CreateOkButton()
        {
            OkButton = new BorderedTextButton("OK", Color.LimeGreen, (sender, args) => Dialog.Close())
            {
                Parent = FooterBackground,
                Alignment = Alignment.MidRight,
                X = -20,
                Text =
                {
                    Font = Fonts.Exo2SemiBold,
                    FontSize = 13
                }
            };

            OkButton.Height -= 8;
        }

        /// <summary>
        /// </summary>
        private void CreateCancelButton()
        {
            CancelButton = new BorderedTextButton("Cancel", Color.Crimson, (sender, args) => Dialog.Close())
            {
                Parent = FooterBackground,
                Alignment = Alignment.MidRight,
                X = OkButton.X - OkButton.Width - 20,
                Text =
                {
                    Font = Fonts.Exo2SemiBold,
                    FontSize = 13
                }
            };

            CancelButton.Height -= 8;
        }
    }
}
