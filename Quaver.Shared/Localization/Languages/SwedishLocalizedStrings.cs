using System;
using System.Collections.Generic;
using System.Text;

namespace Quaver.Shared.Localization.Languages
{
    public class SwedishLocalizedStrings : LocalizedStrings
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override Dictionary<LocalizedString, string> Templates => new Dictionary<LocalizedString, string>()
        {
            { LocalizedString.FPS_IS_NOW_UNLIMITED, "FPS är nu oändlig." },
            { LocalizedString.FPS_IS_NOW_LIMITED_TO_240_FPS, "FPS är nu begränsad till: 240 FPS" },
            { LocalizedString.VSYNC_ENABLED, "Vsync Aktiverad" },
            { LocalizedString.FPS_IS_NOW_CUSTOM_LIMITED_TO, "FPS är nu anpassnings begränsad till: {0} FPS" }
        };
    }
}
