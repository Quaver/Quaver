/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Overlays.Chat.Components.Users;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Result.UI
{
    public class ResultButtonContainer : Sprite
    {
        /// <summary>
        ///     Reference to the parent screen
        /// </summary>
        private ResultScreen Screen { get; }

        /// <summary>
        ///     Reference to the currently selected button in the container
        /// </summary>
        public SelectableBorderedTextButton SelectedButton => Buttons[SelectedButtonIndex];

        /// <summary>
        ///     The inex of the currently selected button
        /// </summary>
        private int SelectedButtonIndex { get; set; }

        /// <summary>
        /// </summary>
        private List<SelectableBorderedTextButton> Buttons { get; set; }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        public ResultButtonContainer(ResultScreen screen)
        {
            Screen = screen;
            Size = new ScalableVector2(WindowManager.Width - 56, 54);
            Tint = Color.Black;
            Alpha = 0;

            // AddBorder(Color.White, 2);
            InitializeButtons();
        }

        /// <summary>
        ///     Initializes all the buttons in the container
        /// </summary>
        private void InitializeButtons()
        {
            Buttons = new List<SelectableBorderedTextButton>
            {
                CreateButton("Back", true, (o, e) => Screen.ExitToMenu()),
                CreateButton("Export Replay", false, (o, e) => Screen.ExportReplay()),
                CreateButton("Watch Replay", false, (o, e) => Screen.ExitToWatchReplay()),
                CreateButton("Retry", false, (o, e) => Screen.ExitToRetryMap()),
            };

            // Go through each button and initialize the sprite further.
            for (var i = 0; i < Buttons.Count; i++)
            {
                var btn = Buttons[i];
                btn.Parent = this;
                btn.Alignment = Alignment.MidLeft;

                var sizePer = Width / Buttons.Count;
                btn.X = sizePer * i + sizePer / 2f - btn.Width / 2f;
            }
        }

        /// <summary>
        ///     Changes the selected button in a given direction
        /// </summary>
        /// <param name="direction"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void ChangeSelectedButton(Direction direction)
        {
            var prevSelected = SelectedButtonIndex;

            switch (direction)
            {
                case Direction.Forward:
                    if (SelectedButtonIndex + 1 < Buttons.Count)
                        SelectedButtonIndex = SelectedButtonIndex + 1;
                    break;
                case Direction.Backward:
                    if (SelectedButtonIndex - 1 >= 0)
                        SelectedButtonIndex = SelectedButtonIndex - 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            if (SelectedButtonIndex == prevSelected)
                return;

            Buttons[prevSelected].Selected = false;
            Buttons[SelectedButtonIndex].Selected = true;
            SkinManager.Skin.SoundHover.CreateChannel().Play();
        }

        /// <summary>
        ///     Creates a SelectableBorderedTextButton according to specific
        ///     requirements the container needs
        /// </summary>
        /// <returns></returns>
        private SelectableBorderedTextButton CreateButton(string text, bool selected, EventHandler onClick)
        {
            var btn = new SelectableBorderedTextButton(text, Color.White, Colors.MainAccent, selected, onClick)
            {
                Height = Height,
                Tint = Color.Black,
                Alpha = 0.45f,
                Text =
                {
                    FontSize = 14,
                    Font = Fonts.Exo2SemiBold
                }
            };

            btn.Width++;

            return btn;
        }
    }
}
