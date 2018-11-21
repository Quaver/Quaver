using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Assets;
using Quaver.Graphics;
using Quaver.Helpers;
using Quaver.Modifiers;
using Quaver.Modifiers.Mods.Gameplay;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.UI.Form;

namespace Quaver.Screens.SongSelect.UI.Modifiers
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
            "0.6x",
            "0.7x",
            "0.8x",
            "0.9x",
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
        public DrawableModifierSpeed(ModifiersDialog dialog) : base(dialog, new ManiaModSpeed(ModIdentifier.Speed05X))
            => RateChanger = new HorizontalSelector(Speeds, new ScalableVector2(200, 32), BitmapFonts.Exo2SemiBold, 13,
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
        /// <returns></returns>
        public override List<DrawableModifierOption> CreateModsDialogOptions() => new List<DrawableModifierOption>();

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