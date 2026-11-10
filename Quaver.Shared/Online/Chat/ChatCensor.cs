using ComputerysProfanityFilter;

namespace Quaver.Shared.Online.Chat
{
    /// <summary>
    ///     Applies profanity censoring to chat messages.
    /// </summary>
    public static class ChatCensor
    {
        /// <summary>
        ///     Profanity lists are expensive to construct and are safe to reuse across threads.
        /// </summary>
        private static ProfanityList Filter { get; } = new ProfanityList();

        /// <summary>
        ///     Replaces detected profanity using Quaver's existing asterisk censor style.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Censor(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value ?? string.Empty;

            return Filter.Censor(value, '*');
        }
    }
}
