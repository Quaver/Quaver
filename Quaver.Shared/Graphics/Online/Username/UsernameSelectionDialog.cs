/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Online.Username
{
    public class UsernameSelectionDialog : DialogScreen
    {
        /// <summary>
        ///     The containing box for the dialog.
        /// </summary>
        private Sprite ContainingBox { get; set; }

        /// <summary>
        ///     The header text to create a username.
        /// </summary>
        private SpriteText Header { get; set; }

        /// <summary>
        ///     The text content of the dialog which displays the requirements for usernames.
        /// </summary>
        private SpriteText TextContent { get; set; }

        /// <summary>
        ///     Second line for text content.
        /// </summary>
        private SpriteText TextContent2 { get; set; }

        /// <summary>
        ///     The textbox to enter a username.
        /// </summary>
        private UsernameSelectionTextbox Textbox { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="view"></param>
        /// <param name="backgroundAlpha"></param>
        public UsernameSelectionDialog(float backgroundAlpha) : base(backgroundAlpha) => CreateContent();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            ContainingBox = new Sprite()
            {
                Parent = Container,
                Size = new ScalableVector2(WindowManager.Width, 200),
                Alignment = Alignment.MidCenter,
                Tint = Color.Black,
                Alpha = 0.95f
            };

            var line = new Sprite()
            {
                Parent = ContainingBox,
                Size = new ScalableVector2(ContainingBox.Width, 1),
                Tint = Colors.MainAccent
            };

            Header = new SpriteText(Fonts.Exo2Bold, "Create Username", 20)
            {
                Parent = ContainingBox,
                Alignment = Alignment.TopCenter,
                Y = 25
            };

            TextContent = new SpriteText(Fonts.Exo2SemiBold,
                "Usernames must be between 3 to 15 characters and may only contain", 13)
            {
                Parent = ContainingBox,
                Alignment = Alignment.TopCenter,
                Y = Header.Y + Header.Height + 5
            };


            TextContent2 = new SpriteText(Fonts.Exo2SemiBold,
                "letters (A-Z), numbers (0-9), hyphens (-), and spaces.", 13)
            {
                Parent = ContainingBox,
                Alignment = Alignment.TopCenter,
                Y = TextContent.Y + TextContent.Height + 5
            };


            Textbox = new UsernameSelectionTextbox()
            {
                Parent = ContainingBox,
                Alignment = Alignment.TopCenter,
                Y = TextContent2.Y + 35
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
        }
    }
}
