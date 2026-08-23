using System;
using System.Linq;
using System.Text.RegularExpressions;
using Quaver.Shared.Screens.Options.Sections;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Options
{
    public static class OptionsLocalization
    {
        private const string Prefix = "Screen_Options_";

        private static readonly Regex KeyCountRegex = new Regex(@"\b(?<count>\d+)K\b",
            RegexOptions.Compiled);

        public static string Get(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                return label;

            var keyCountMatch = KeyCountRegex.Match(label);

            if (keyCountMatch.Success)
            {
                var genericLabel = KeyCountRegex.Replace(label, string.Empty);

                try
                {
                    return LocalizationManager.Get(GetKey($"Key Count {genericLabel}"),
                        keyCountMatch.Groups["count"].Value);
                }
                catch (ArgumentException)
                {
                    // Fall back to the full label for numbered options without a generic resource.
                }
            }

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
