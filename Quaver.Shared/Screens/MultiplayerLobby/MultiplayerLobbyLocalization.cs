using System;
using Wobble.Managers;

namespace Quaver.Shared.Screens.MultiplayerLobby
{
    public static class MultiplayerLobbyLocalization
    {
        private const string Prefix = "Screen_MultiplayerLobby_";

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
