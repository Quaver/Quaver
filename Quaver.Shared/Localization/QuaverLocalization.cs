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
            AvailableLanguages.FirstOrDefault(x => x.CultureName == cultureName) ?? AvailableLanguages.First();

        private static CultureInfo GetCulture(string cultureName) => GetLanguage(cultureName).Culture;
    }
}
