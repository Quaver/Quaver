using System;
using System.Collections.Generic;
using System.Text;

namespace Quaver.Shared.Localization.Languages
{
    public class EnglishLocalizedStrings : LocalizedStrings
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override Dictionary<LocalizedString, string> Templates => new Dictionary<LocalizedString, string>()
        {
            { LocalizedString.FPS_IS_NOW_UNLIMITED, "FPS is now unlimited." },
            { LocalizedString.FPS_IS_NOW_LIMITED_TO_240_FPS, "FPS is now limited to: 240 FPS" },
            { LocalizedString.VSYNC_ENABLED, "Vsync Enabled" },
            { LocalizedString.FPS_IS_NOW_CUSTOM_LIMITED_TO, "FPS is now custom limited to: {0} FPS" },
            { LocalizedString.HOME, "Home" },
        };
    }
}
