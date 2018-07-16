using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quaver.API.Enums;
using Quaver.Main;
using Quaver.Modifiers;

namespace Quaver.Helpers
{
    internal static class ModHelper
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="mods"></param>
        /// <returns></returns>
        internal static string GetModsString(ModIdentifier mods)
        {
            if (mods == 0)
                return "None";

            var modStrings = new List<string>();

            foreach (ModIdentifier mod in Enum.GetValues(typeof(ModIdentifier)))
            {
                if (!mods.HasFlag(mod))
                    continue;

                switch (mod)
                {
                    case ModIdentifier.NoSliderVelocity:
                        modStrings.Add("NSV");
                        break;
                    case ModIdentifier.Speed05X:
                        modStrings.Add("0.5x");
                        break;
                    case ModIdentifier.Speed06X:
                        modStrings.Add("0.6x");
                        break;
                    case ModIdentifier.Speed07X:
                        modStrings.Add("0.7x");
                        break;
                    case ModIdentifier.Speed08X:
                        modStrings.Add("0.8x");
                        break;
                    case ModIdentifier.Speed09X:
                        modStrings.Add("0.9x");
                        break;
                    case ModIdentifier.Speed11X:
                        modStrings.Add("1.1x");
                        break;
                    case ModIdentifier.Speed12X:
                        modStrings.Add("1.2x");
                        break;
                    case ModIdentifier.Speed13X:
                        modStrings.Add("1.3x");
                        break;
                    case ModIdentifier.Speed14X:
                        modStrings.Add("1.4x");
                        break;
                    case ModIdentifier.Speed15X:
                        modStrings.Add("1.5x");
                        break;
                    case ModIdentifier.Speed16X:
                        modStrings.Add("1.6x");
                        break;
                    case ModIdentifier.Speed17X:
                        modStrings.Add("1.7x");
                        break;
                    case ModIdentifier.Speed18X:
                        modStrings.Add("1.8x");
                        break;
                    case ModIdentifier.Speed19X:
                        modStrings.Add("1.9x");
                        break;
                    case ModIdentifier.Speed20X:
                        modStrings.Add("2.0x");
                        break;
                    case ModIdentifier.Strict:
                        modStrings.Add("Strict");
                        break;
                    case ModIdentifier.Chill:
                        modStrings.Add("Chill");
                        break;
                    case ModIdentifier.NoPause:
                        modStrings.Add("No Pause");
                        break;
                    case ModIdentifier.Autoplay:
                        modStrings.Add("Autoplay");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Short string for ModIdentifier: {mod} does not exist.");
                }
            }

            return string.Join(", ", modStrings);
        }
    }
}