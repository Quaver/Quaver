using System;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Multiplayer
{
    public static class MultiplayerLocalization
    {
        private const string Prefix = "Screen_Multiplayer_";

        public static string Get(string label)
        {
            try
            {
                return LocalizationManager.Get($"{Prefix}{label}");
            }
            catch (ArgumentException)
            {
                return label;
            }
        }

        public static string Get(string label, params object[] args)
        {
            try
            {
                return LocalizationManager.Get($"{Prefix}{label}", args);
            }
            catch (ArgumentException)
            {
                return string.Format(label, args);
            }
        }
    }
}
