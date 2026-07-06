using System;
using System.Linq;
using Quaver.Shared.Screens.Options.Sections;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Options
{
    public static class OptionsLocalization
    {
        private const string Prefix = "Screen_Options_";

        public static string Get(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                return label;

            try
            {
                return LocalizationManager.Get(GetKey(label));
            }
            catch (ArgumentException)
            {
                return label;
            }
        }

        public static string GetSectionSettings(OptionsSection section) =>
            Get($"{section.LocalizationLabel} Settings").ToUpper();

        public static string GetSearchResultCount(int count) =>
            LocalizationManager.Get($"{Prefix}SearchResult{(count == 1 ? "" : "s")}", count);

        private static string GetKey(string label) =>
            Prefix + string.Concat(label.Where(char.IsLetterOrDigit));
    }
}
