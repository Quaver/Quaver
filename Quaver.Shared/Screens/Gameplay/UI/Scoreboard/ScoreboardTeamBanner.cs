using System;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Online;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.UI.Scoreboard
{
    public class ScoreboardTeamBanner : Sprite
    {
        private Scoreboard Scoreboard { get; }

        private SpriteTextBitmap Points { get; }

        private SpriteTextBitmap TeamRating { get; }

        public ScoreboardTeamBanner(Scoreboard scoreboard)
        {
            Scoreboard = scoreboard;
            Size = new ScalableVector2(260, 34);

            // Set the correct image to use
            switch (Scoreboard.Team)
            {
                case MultiplayerTeam.Red:
                    Image = UserInterface.TeamBannerRed;
                    break;
                case MultiplayerTeam.Blue:
                    Image = UserInterface.TeamBannerBlue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var pointsContainer = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(48, 30),
                Alignment = Alignment.MidLeft,
                Alpha = 0,
            };

            pointsContainer.X = Scoreboard.Team == MultiplayerTeam.Blue ? Width - 1 - pointsContainer.Width : 0;

            Points = new SpriteTextBitmap(FontsBitmap.GothamRegular, "0")
            {
                Parent = pointsContainer,
                Alignment = Alignment.MidCenter,
                FontSize = 18
            };

            switch (Scoreboard.Team)
            {
                case MultiplayerTeam.Red:
                    Points.Text = OnlineManager.CurrentGame.RedTeamWins.ToString();
                    break;
                case MultiplayerTeam.Blue:
                    Points.Text = OnlineManager.CurrentGame.BlueTeamWins.ToString();
                    break;
            }

            var ratingContainer = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(60, 30),
                Alpha = 0
            };

            ratingContainer.X = Scoreboard.Team == MultiplayerTeam.Blue ? -Width + 1 + ratingContainer.Width : 0;

            TeamRating = new SpriteTextBitmap(FontsBitmap.GothamRegular, "0.00", false)
            {
                Parent = ratingContainer,
                Alignment = Alignment.MidCenter,
                FontSize = 17
            };

            OnlineManager.Client.OnGameTeamWinCount += OnTeamWinCountChanged;
        }

        public override void Destroy()
        {
            OnlineManager.Client.OnGameTeamWinCount -= OnTeamWinCountChanged;
            base.Destroy();
        }

        private void OnTeamWinCountChanged(object sender, TeamWinCountEventArgs e)
        {
            switch (Scoreboard.Team)
            {
                case MultiplayerTeam.Red:
                    Points.Text = e.RedTeamWins.ToString();
                    break;
                case MultiplayerTeam.Blue:
                    Points.Text = e.BlueTeamWins.ToString();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void UpdateAverageRating()
        {
            var sum = 0d;

            foreach (var players in Scoreboard.Users)
            {
                var rating = players.RatingProcessor.CalculateRating(players.Processor);

                if (players.Processor.MultiplayerProcessor != null)
                {
                    if (players.Processor.MultiplayerProcessor.IsRegeneratingHealth ||
                        players.Processor.MultiplayerProcessor.IsEliminated)
                        rating = 0;
                }

                sum += rating;
            }

            var average = Scoreboard.Users.Count == 0 ? 0 : sum / Scoreboard.Users.Count;
            TeamRating.Text = $"{average:0.00}";
        }
    }
}