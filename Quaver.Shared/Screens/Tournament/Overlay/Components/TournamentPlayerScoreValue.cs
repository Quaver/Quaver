using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Screens.Tournament.Overlay.Components
{
    public abstract class TournamentPlayerScoreValue : TournamentOverlaySpriteText
    {
        protected TournamentPlayer Player { get; }

        protected List<TournamentPlayer> Players { get; }

        private int PreviousJudgementCount { get; set; } = -1;

        public TournamentPlayerScoreValue(TournamentDrawableSettings settings, TournamentPlayer player, List<TournamentPlayer> players) : base(settings)
        {
            Player = player;
            Players = players;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var judgeCount = Player.Scoring.TotalJudgementCount;

            if (Player.Scoring != null && PreviousJudgementCount != judgeCount)
            {
                SetValue();

                var otherPlayer = Players.Find(x => x != Player);

                var color = Settings.Tint.Value;

                if (Player.IsWinning(otherPlayer))
                {
                    Tint = color;
                    FontSize = Settings.FontSize.Value;
                }
                else
                {
                    if (Settings.DimWhenLosing.Value)
                        Tint = new Color(color.R / 2, color.G / 2, color.B / 2);
                    else
                        Tint = color;

                    FontSize = Settings.FontSizeWhenLosing.Value;
                }

                PreviousJudgementCount = judgeCount;
            }

            base.Update(gameTime);
        }

        protected abstract void SetValue();
    }
}