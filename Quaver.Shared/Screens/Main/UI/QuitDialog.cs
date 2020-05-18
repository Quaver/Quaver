using System;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers.Input;
using Wobble;

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
            : base("EXIT QUAVER", $"Are you sure you would like to quit the game?", () =>
            {
                var game = (QuaverGame) GameBase.Game;
                game.Exit();
            })
        {
            Cheat = new CheatCodeQuit(this) {Parent = this};
        }
    }
}