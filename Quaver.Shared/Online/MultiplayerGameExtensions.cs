using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Multiplayer;

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
            var diffName = "";
            var pattern = @"\[(.*?)\]";
            var matches = Regex.Matches(game.Map, pattern);

            foreach (Match match in matches)
            {
                if (match != matches.Last())
                    continue;

                diffName = match.Groups[1].ToString();
            }

            return diffName;
        }
    }
}