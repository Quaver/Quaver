using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using Wobble.Managers;

namespace Quaver.Shared.Localization
{
    public static class QuaverLocalization
    {
        public const string DefaultCultureName = "en";

        public static readonly IReadOnlyList<AvailableLanguage> AvailableLanguages = new List<AvailableLanguage>
        {
            new AvailableLanguage("en", "Language_English"),
            // new AvailableLanguage("bg-BG", "Language_Bulgarian"),
            // new AvailableLanguage("da-DK", "Language_Danish"),
            // new AvailableLanguage("de-DE", "Language_German"),
            // new AvailableLanguage("es-ES", "Language_Spanish"),
            new AvailableLanguage("fr-FR", "Language_French"),
            // new AvailableLanguage("id-ID", "Language_Indonesian"),
            // new AvailableLanguage("it-IT", "Language_Italian"),
            // new AvailableLanguage("ja-JP", "Language_Japanese"),
            // new AvailableLanguage("ko-KR", "Language_Korean"),
            // new AvailableLanguage("nl-NL", "Language_Dutch"),
            // new AvailableLanguage("no-NO", "Language_Norwegian"),
            // new AvailableLanguage("pl-PL", "Language_Polish"),
            // new AvailableLanguage("pt-BR", "Language_PortugueseBrazil"),
            // new AvailableLanguage("ru-RU", "Language_Russian"),
            // new AvailableLanguage("sv-SE", "Language_Swedish"),
            // new AvailableLanguage("th-TH", "Language_Thai"),
            // new AvailableLanguage("uk-UA", "Language_Ukrainian"),
            new AvailableLanguage("zh-CN", "Language_ChineseSimplified"),
            // new AvailableLanguage("zh-TW", "Language_ChineseTraditional"),
        };

        public const int NotoCjkJpFaceIndex = 0;
        public const int NotoCjkKrFaceIndex = 1;
        public const int NotoCjkScFaceIndex = 2;
        public const int NotoCjkTcFaceIndex = 3;
        public const int NotoCjkHkFaceIndex = 4;

        public static void Configure(string cultureName)
        {
            LocalizationManager.Configure(
                new ResourceManager("Quaver.Shared.Localization.Strings", typeof(QuaverLocalization).Assembly),
                CultureInfo.GetCultureInfo(DefaultCultureName),
                GetCulture(cultureName));
        }

        public static void SetCurrentCulture(string cultureName)
        {
            var culture = GetCulture(cultureName);
            LocalizationManager.SetCurrentCulture(culture);
        }

        public static AvailableLanguage GetLanguage(string cultureName) =>
            AvailableLanguages.FirstOrDefault(x => x.CultureName == cultureName) ??
            AvailableLanguages.FirstOrDefault(x => string.Equals(x.CultureName, cultureName, StringComparison.OrdinalIgnoreCase)) ??
            AvailableLanguages.First();

        private static CultureInfo GetCulture(string cultureName) => GetLanguage(cultureName).Culture;

        public static int GetNotoCjkFaceIndex(string cultureName)
        {
            var culture = GetCultureInfo(cultureName);

            switch (culture.Name)
            {
                case "ja":
                case "ja-JP":
                    return NotoCjkJpFaceIndex;
                case "ko":
                case "ko-KR":
                    return NotoCjkKrFaceIndex;
                case "zh-CN":
                case "zh-Hans":
                case "zh-SG":
                    return NotoCjkScFaceIndex;
                case "zh-HK":
                case "zh-MO":
                    return NotoCjkHkFaceIndex;
                case "zh":
                case "zh-TW":
                case "zh-Hant":
                    return NotoCjkTcFaceIndex;
                default:
                    return NotoCjkTcFaceIndex;
            }
        }

        private static CultureInfo GetCultureInfo(string cultureName)
        {
            if (string.IsNullOrWhiteSpace(cultureName))
                return CultureInfo.GetCultureInfo(DefaultCultureName);

            try
            {
                return CultureInfo.GetCultureInfo(cultureName);
            }
            catch (CultureNotFoundException)
            {
                return CultureInfo.GetCultureInfo(DefaultCultureName);
            }
        }
    }
}
