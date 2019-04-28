using Quaver.Shared.Config;
using Quaver.Shared.Localization.Languages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quaver.Shared.Localization
{
    public static class LocalizationManager
    {
        /// <summary>
        ///     Dictionary of language -> localization strings
        /// </summary>
        private static Dictionary<LocalizationLanguage, LocalizedStrings> localizationLanguages => new Dictionary<LocalizationLanguage, LocalizedStrings>()
        {
            { LocalizationLanguage.EN, new EnglishLocalizedStrings() },
            { LocalizationLanguage.SV, new SwedishLocalizedStrings() }
        };

        /// <summary>
        ///     Default language when string is not available in <see cref="CurrentLanguage"/>
        /// </summary>
        public static LocalizationLanguage DefaultLanguage = LocalizationLanguage.EN;
        /// <summary>
        ///     Currently selected language
        /// </summary>
        public static LocalizationLanguage CurrentLanguage => ConfigManager.Language.Value;

        /// <summary>
        ///     Default strings if the string has yet to be translated in <see cref="CurrentLocalizedStrings"/>
        /// </summary>
        private static LocalizedStrings DefaultLocalizedStrings => localizationLanguages[DefaultLanguage];
        /// <summary>
        ///     Localized strings for the language set in <see cref="CurrentLanguage"/>
        /// </summary>
        private static LocalizedStrings CurrentLocalizedStrings => localizationLanguages[CurrentLanguage];

        /// <summary>
        ///     Get localized string.
        /// </summary>
        /// <param name="str">string to localize</param>
        /// <param name="args">additional parameters to the string, for example a number to be displayed in the string</param>
        /// <returns>Localized string</returns>
        public static string Get(LocalizedString str, params object[] args)
        {
            if (CurrentLocalizedStrings.Templates.ContainsKey(str))
                return string.Format(CurrentLocalizedStrings.Templates[str], args);
            else
                return string.Format(DefaultLocalizedStrings.Templates[str], args);
        }
    }
}
