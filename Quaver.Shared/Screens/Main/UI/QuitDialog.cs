using System;
using Quaver.Shared.Graphics;
using Wobble;

namespace Quaver.Shared.Screens.Main.UI
{
    public class QuitDialog : YesNoDialog
    {
        public QuitDialog()
            : base("EXIT QUAVER", $"Are you sure you would like to quit the game?", () =>
            {
                var game = (QuaverGame) GameBase.Game;
                game.Exit();
            })
        {
        }
    }
}