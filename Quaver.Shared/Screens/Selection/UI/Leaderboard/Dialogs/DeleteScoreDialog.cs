using System;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics;
using Quaver.Shared.Scheduling;

namespace Quaver.Shared.Screens.Selection.UI.Leaderboard.Dialogs
{
    public class DeleteScoreDialog : YesNoDialog
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="score"></param>
        public DeleteScoreDialog(Score score)
            : base("Delete Score", "Are you sure you would you like to delete this score?", () =>
                {
                    ThreadScheduler.Run(() =>
                    {
                        ScoreDatabaseCache.DeleteScoreFromDatabase(score);
                    });
                })
        {
        }
    }
}