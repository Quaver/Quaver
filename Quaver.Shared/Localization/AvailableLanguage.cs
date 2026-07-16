using System.Globalization;

namespace Quaver.Shared.Localization
{
    public class AvailableLanguage
    {
        public string CultureName { get; }

        public string DisplayNameKey { get; }

        public CultureInfo Culture => CultureInfo.GetCultureInfo(CultureName);

        public AvailableLanguage(string cultureName, string displayNameKey)
        {
            CultureName = cultureName;
            DisplayNameKey = displayNameKey;
        }
    }
}
