using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Discord;
using Quaver.Logging;
using Quaver.Net;
using Quaver.Net.Requests;
using Quaver.Net.Requests.Events;

namespace Quaver.Online.Events
{
    internal class ScoreSubmitted
    {
        /// <summary>
        ///     Called when our score has successfully submitted.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void OnScoreSubmission(object sender, ScoreSubmissionEventArgs e)
        {
            switch (e.Code)
            {
                // Scores are disregarded by the server on certain occasions (Duplicates/scores w/ short play times)
                case ScoreSubmissionCode.Disregarded:
                    break;
                case ScoreSubmissionCode.FlamingoOnFire:
                    Logger.LogWarning($"Score could not be submitted due to Flamingo being offline. Retrying in 30 seconds...", LogType.Network);
                    // Retry score here
                    break;
                case ScoreSubmissionCode.InternalError:
                case ScoreSubmissionCode.InvalidRequest:
                    Logger.LogError($"Score could not be submitted due to a fatal error!", LogType.Network);
                    break;
                case ScoreSubmissionCode.MapNotRanked:
                    Logger.LogImportant($"Score could not be submitted. The ranked status may have changed, so update your map!", LogType.Network);
                    break;
                case ScoreSubmissionCode.Success:
                    Logger.LogSuccess($"Score successfully submitted: PB: {e.Score.Pb} | Rank: {e.Stats.Rank} | Ranked Score: {e.Stats.RankedScore}", LogType.Network);
                    // Do score submission success stuff!

                    // Update user stats last if you're wanting to do animations with the old user stats & the newly updated ones.
                    Flamingo.Self.Stats = e.Stats;
                    break;
                default:
                    break;
            }
        }
    }
}
