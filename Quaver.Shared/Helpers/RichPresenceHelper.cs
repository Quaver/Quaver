using Quaver.Shared.Discord;
using Quaver.Shared.Online;
using System;
using System.Collections.Generic;
using System.Text;

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
        /// <param name="state">state of the game</param>
        /// <param name="details">details of the game.</param>
        public static void UpdateRichPresence(string state, string details) {
            // might be a good idea to check k_cchMaxRichPresenceValueLength and the return value in the future.
            SteamManager.SetRichPresence("State", state);
            SteamManager.SetRichPresence("Details", details);

            DiscordHelper.Presence.Details = details;
            DiscordHelper.Presence.State = state;
            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);
        }
    }
}
