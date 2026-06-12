using Quaver.Shared.Discord;
using Quaver.Shared.Online;
using Steamworks;

namespace Quaver.Shared.Helpers
{
    public static class RichPresenceHelper
    {
        /// <summary>
        ///     Sets both discord and steam (if applicable)'s rich presence.
        ///
        ///     note: it might be a good idea to set your images and other properties in DiscordHelper.Presence first before calling this function, instead of
        ///     calling rpc twice.
        /// </summary>
        /// <param name="state">State of the game. It is assumed to be small enough to fit in the rich presence.</param>
        /// <param name="details">Details of the game. Strings too long gets automatically truncated.</param>
        public static void UpdateRichPresence(string state, string details)
        {
            static string Truncate(string s, int length)
            {
                if (string.IsNullOrEmpty(s) || s.Length <= length)
                    return s;

                return $"{s[..(length - 1)]}…";
            }

            SteamManager.SetRichPresence("State", Truncate(state, Constants.k_cchMaxRichPresenceValueLength));
            DiscordHelper.Presence.State = Truncate(state, 120);

            // This would be potentially problematic as the source specifies 'bytes', and .NET using UTF-16 could have
            // made things complicated. Fortunately however, through direct testing it appears that they consider
            // UTF-16 codepoints to be 1 byte long, even if the underlying memory layout would suggest otherwise.
            SteamManager.SetRichPresence("Details", Truncate(details, Constants.k_cchMaxRichPresenceValueLength));
            DiscordHelper.Presence.Details = Truncate(details, 120);

            DiscordHelper.UpdatePresence();
        }
    }
}
