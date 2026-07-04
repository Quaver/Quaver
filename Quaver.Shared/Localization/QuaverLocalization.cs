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
            new AvailableLanguage("bg", "Language_Bulgarian"),
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
