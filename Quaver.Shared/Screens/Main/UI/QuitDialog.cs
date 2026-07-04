using System;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers.Input;
using Wobble;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Main.UI
{
    public class QuitDialog : YesNoDialog
    {
        /// <summary>
        /// </summary>
        private CheatCodeQuit Cheat { get; }

        /// <summary>
        /// </summary>
        public QuitDialog()
            : base(LocalizationManager.Get("Screen_Main_QuitDialogTitle"),
                LocalizationManager.Get("Screen_Main_QuitDialogMessage"), () =>
            {
                var game = (QuaverGame)GameBase.Game;
                game.Exit();
            })
        {
            Cheat = new CheatCodeQuit(this) { Parent = this };
        }
    }
}
