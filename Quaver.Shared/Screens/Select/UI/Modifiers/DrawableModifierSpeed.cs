/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Modifiers.Mods;
using Wobble.Graphics;
using Wobble.Graphics.UI.Form;

namespace Quaver.Shared.Screens.Select.UI.Modifiers
{
    public class DrawableModifierSpeed : DrawableModifier
    {
        /// <summary>
        ///     Horizontal selector to change the rate of the audio
        /// </summary>
        private HorizontalSelector RateChanger { get; }

        /// <summary>
        ///     All of the available speeds to play
        /// </summary>
        private List<string> Speeds { get; } = new List<string>()
        {
            "0.5x",
            "0.55x",
            "0.6x",
            "0.65x",
            "0.7x",
            "0.75x",
            "0.8x",
            "0.85x",
            "0.9x",
            "0.95x",
            "1.0x",
            "1.1x",
            "1.2x",
            "1.3x",
            "1.4x",
            "1.5x",
            "1.6x",
            "1.7x",
            "1.8x",
            "1.9x",
            "2.0x"
        };

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        public DrawableModifierSpeed(ModifiersDialog dialog) : base(dialog, new ModSpeed(ModIdentifier.Speed05X))
            => RateChanger = new HorizontalSelector(Speeds, new ScalableVector2(200, 32), Fonts.Exo2SemiBold, 13,
            UserInterface.HorizontalSelectorLeft,
            UserInterface.HorizontalSelectorRight, new ScalableVector2(32, 32), 0, OnSelected, GetSelectedIndex())
        {
            Parent = this,
            Alignment = Alignment.MidRight,
            X = -54,
            UsePreviousSpriteBatchOptions = true,
            ButtonSelectLeft = { UsePreviousSpriteBatchOptions = true },
            ButtonSelectRight = { UsePreviousSpriteBatchOptions = true },
            SelectedItemText = { UsePreviousSpriteBatchOptions = true }
        };

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void ChangeSelectedOptionButton()
        {
        }

        /// <summary>
        ///     Gets the selected index of the speeds based on the audio rate
        /// </summary>
        /// <returns></returns>
        private int GetSelectedIndex()
        {
            var rate = ModHelper.GetRateFromMods(ModManager.Mods);
            string rateString;

            switch (rate)
            {
                case 1f:
                    rateString = "1.0x";
                    break;
                case 2:
                    rateString = "2.0x";
                    break;
                default:
                    rateString = $"{rate}x";
                    break;
            }

            return Speeds.FindIndex(x => x == rateString);
        }

        /// <summary>
        ///     Called when the selected value changes
        /// </summary>
        /// <param name="val"></param>
        /// <param name="index"></param>
        private static void OnSelected(string val, int index)
        {
            if (val == "1.0x")
            {
                ModManager.RemoveSpeedMods();
                return;
            }

            ModManager.AddMod(ModHelper.GetModsFromRate(float.Parse(val.Replace("x", ""))));
        }
    }
}
