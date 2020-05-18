using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.Importing;
using Wobble;

namespace Quaver.Shared.Screens.Selection.UI.Dialogs
{
    public class RefreshDialog : YesNoDialog
    {
        /// <summary>
        /// </summary>
        public RefreshDialog()
            : base("REFRESH MAPSETS", $"Are you sure you would like to refresh all of your maps?", () =>
            {
                var game = (QuaverGame) GameBase.Game;
                game.CurrentScreen.Exit(() => new ImportingScreen(null, true, true));
            })
        {
        }
    }
}
