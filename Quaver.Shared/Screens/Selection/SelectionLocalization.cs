using System;
using System.Linq;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection
{
    public static class SelectionLocalization
    {
        private const string Prefix = "Screen_Selection_";

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

        public static string Get(string label, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(label))
                return label;

            try
            {
                return LocalizationManager.Get(GetKey(label), args);
            }
            catch (ArgumentException)
            {
                return string.Format(label, args);
            }
        }

        public static string GetKey(string label) =>
            Prefix + string.Concat(label.Where(char.IsLetterOrDigit));
    }
}
