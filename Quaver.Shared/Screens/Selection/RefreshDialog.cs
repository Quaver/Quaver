using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Importing;
using Wobble;

namespace Quaver.Shared.Screens.Selection
{
    public class RefreshDialog : YesNoDialog
    {
        /// <summary>
        /// </summary>
        public RefreshDialog()
            : base("REFRESH MAPSETS", $"Are you sure you would like to refresh?", () =>
            {
                var game = (QuaverGame) GameBase.Game;
                game.CurrentScreen.Exit(() => new ImportingScreen(null, true, true));
            })
        {
        }
    }
}
