using System.Collections.Generic;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Gameplay.UI.Scoreboard;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Result.UI.Multiplayer
{
    public class ResultMultiplayerScoreboard : Sprite
    {
        /// <summary>
        /// </summary>
        private ResultMultiplayerScoreboardTableHeader TableHeader { get; }

        /// <summary>
        /// </summary>
        private ResultMultiplayerScoreboardList ScoreboardList { get; }

        /// <summary>
        /// </summary>
        public ResultMultiplayerScoreboard(List<ScoreboardUser> scoreboardUsers)
        {
            Size = new ScalableVector2(WindowManager.Width - 56, 490);

            if (OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Team)
                Height -= 46;

            Image = UserInterface.ResultMultiplayerPanel;

            TableHeader = new ResultMultiplayerScoreboardTableHeader((int) Width - 4)
            {
                Parent = this
            };

            ScoreboardList = new ResultMultiplayerScoreboardList(TableHeader, scoreboardUsers, new ScalableVector2(Width - 4, Height - TableHeader.Height - 2))
            {
                Parent = this,
                Y = TableHeader.Height + 1,
                X = 2
            };
        }
    }
}