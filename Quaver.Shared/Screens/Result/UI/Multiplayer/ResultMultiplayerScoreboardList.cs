using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Gameplay.UI.Scoreboard;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Result.UI.Multiplayer
{
    public class ResultMultiplayerScoreboardList : PoolableScrollContainer<ScoreboardUser>
    {
        /// <summary>
        /// </summary>
        private ResultMultiplayerScoreboardTableHeader TableHeader { get; }

        public ResultMultiplayerScoreboardList(ResultMultiplayerScoreboardTableHeader header, List<ScoreboardUser> availableItems,
            ScalableVector2 size, bool startFromBottom = false) : base(availableItems, int.MaxValue, 0, size, size, startFromBottom)
        {
            TableHeader = header;
            Scrollbar.Tint = ColorHelper.HexToColor("#eeeeee");
            Scrollbar.Width = 6;
            Scrollbar.X = 14;
            ScrollSpeed = 150;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1500;
            InputEnabled = true;
            Alpha = 0;

            switch (OnlineManager.CurrentGame.Ruleset)
            {
                case MultiplayerGameRuleset.Team:
                    AvailableItems = availableItems.OrderBy(x => x.Scoreboard.Team).ThenBy(x => x.Rank).ToList();
                    break;
                default:
                    AvailableItems = availableItems.OrderBy(x => x.Rank).ToList();
                    break;
            }

            CreatePool();

            for (var i = 0; i < Pool.Count; i++)
            {
                switch (OnlineManager.CurrentGame.Ruleset)
                {
                    case MultiplayerGameRuleset.Team:
                        switch (Pool[i].Item.Scoreboard.Team)
                        {
                            case MultiplayerTeam.Red:
                                Pool[i].Image = UserInterface.ResultRedTeam;
                                break;
                            case MultiplayerTeam.Blue:
                                Pool[i].Image = UserInterface.ResultBlueTeam;
                                break;
                        }
                        break;
                    default:
                        Pool[i].Image = UserInterface.ResultNoTeam;
                        break;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && DialogManager.Dialogs.Count == 0;

            base.Update(gameTime);
        }

        public override void Destroy()
        {
            Pool.ForEach(x => x.Destroy());
            base.Destroy();
        }

        protected override PoolableSprite<ScoreboardUser> CreateObject(ScoreboardUser item, int index)
            => new ResultMultiplayerScoreboardUser(this, item, index, TableHeader);
    }
}