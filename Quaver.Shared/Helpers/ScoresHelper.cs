using System.Collections.Generic;
using System.Linq;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Rating;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Modifiers;

namespace Quaver.Shared.Helpers
{
    public static class ScoresHelper
    {
        /// <summary>
        ///     Sets the rating processors (if applicable) for a list of scores.
        ///     This is used to calculate scores in realtime that might have mods which change
        ///     the difficulty rating of the base map and aren't cached (such as NLN or FLN)
        /// </summary>
        public static void SetRatingProcessors(List<Score> scores)
        {
            if (MapManager.Selected.Value == null)
                return;

            var freshQua = MapManager.Selected.Value.LoadQua();

            foreach (var score in scores)
            {
                var id = (ModIdentifier) score.Mods;
                var mods = ModManager.IdentifierToModifier(id);

                if (mods != null && mods.Any(x => x.ChangesMapObjects))
                    score.RatingProcessor  = new RatingProcessorKeys(freshQua.SolveDifficulty(id, true).OverallDifficulty);
            }
        }
    }
}