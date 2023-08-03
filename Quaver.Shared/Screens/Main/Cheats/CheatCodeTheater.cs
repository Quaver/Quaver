using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Helpers.Input;
using Quaver.Shared.Screens.Theater;
using Wobble;

namespace Quaver.Shared.Screens.Main.Cheats
{
    public class CheatCodeTheater : CheatCode
    {
        public override Keys[] Combination { get; } = new[]
        {
            Keys.T,
            Keys.H,
            Keys.E,
            Keys.A,
            Keys.T,
            Keys.E,
            Keys.R,
            Keys.OemTilde
        };
        
        protected override void OnActivated()
        {
            var game = (QuaverGame)GameBase.Game;
            game.CurrentScreen.Exit(() => new TheaterScreen());
        }
    }
}