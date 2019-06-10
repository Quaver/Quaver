using Wobble.Graphics;

namespace Quaver.Shared.Screens.Result.UI.Multiplayer
{
    public class ResultMultiplayerContainer : Container
    {
        /// <summary>
        /// </summary>
        private ResultScreen Screen { get; }

        /// <summary>
        /// </summary>
        private ResultMultiplayerScoreboard Scoreboard { get; }

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public ResultMultiplayerContainer(ResultScreen screen)
        {
            Screen = screen;

            Scoreboard = new ResultMultiplayerScoreboard(screen.MultiplayerScores)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = 212
            };
        }
    }
}