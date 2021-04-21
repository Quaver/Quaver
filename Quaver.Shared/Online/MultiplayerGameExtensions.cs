using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Multiplayer;
using Wobble.Logging;

namespace Quaver.Shared.Online
{
    public static class MultiplayerGameExtensions
    {
        /// <summary>
        ///     Retrieves only the map name
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public static string GetMapName(this MultiplayerGame game) => game.Map.Replace($"[{GetDifficultyName(game)}]",
            "");

        /// <summary>
        ///     Gets the name of the difficulty from the map
        /// </summary>
        /// <returns></returns>
        public static string GetDifficultyName(this MultiplayerGame game)
        {
            if (game.Map.LastOrDefault() != ']')
            {
                // Huh?
                Logger.Warning($"Multiplayer map name not ending in ']': {game.Map}", LogType.Runtime);
                return "";
            }

            // The map string has a form of "artist - title [difficulty]". Look for the first " [" after the first " - ".
            //
            // This isn't quite perfect (it's possible trick it by having [, ] or - in a particular order in
            // artist, name or difficulty name), but it works decently well and handles [something] inside
            // a difficulty name.
            var dash = game.Map.IndexOf(" - ");
            if (dash == -1)
            {
                // Shouldn't happen.
                Logger.Warning($"Multiplayer map name doesn't contain \" - \": {game.Map}", LogType.Runtime);
                return "";
            }

            var spaceBracket = game.Map.IndexOf(" [", dash + 3);
            if (spaceBracket == -1)
            {
                // Also shouldn't happen.
                Logger.Warning($"Multiplayer map name doesn't contain \" [\" after \" - \": {game.Map}", LogType.Runtime);
                return "";
            }

            return game.Map.Substring(spaceBracket + 2, game.Map.Length - spaceBracket - 3);
        }
    }
}