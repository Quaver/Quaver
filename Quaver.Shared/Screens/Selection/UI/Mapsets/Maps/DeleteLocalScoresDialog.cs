using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics;
using Quaver.Shared.Scheduling;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets.Maps
{
    public class DeleteLocalScoresDialog: YesNoDialog
    {
        public DeleteLocalScoresDialog(Map map)
            : base("Delete Local Scores".ToUpper(), "Are you sure you would like to delete ALL local scores for this map?",
                () => ThreadScheduler.Run(() => ScoreDatabaseCache.DeleteAllLocalScores(map)))
        {
        }
    }
}