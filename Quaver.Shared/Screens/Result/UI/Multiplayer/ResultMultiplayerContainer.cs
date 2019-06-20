using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Online;
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
        private ResultMultiplayerTeamPanel TeamPanel { get; }

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public ResultMultiplayerContainer(ResultScreen screen)
        {
            Screen = screen;

            if (OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Team)
            {
                TeamPanel = new ResultMultiplayerTeamPanel(screen)
                {
                    Parent = this,
                    Alignment = Alignment.TopCenter,
                    Y = 208
                };
            }

            Scoreboard = new ResultMultiplayerScoreboard(screen.MultiplayerScores)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = 212
            };

            if (TeamPanel != null)
                Scoreboard.Y = TeamPanel.Y + TeamPanel.Height + 16;
        }
    }
}