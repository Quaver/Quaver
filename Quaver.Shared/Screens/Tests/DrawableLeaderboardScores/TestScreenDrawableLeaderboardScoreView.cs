using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Selection.UI.Leaderboard.Components;
using Wobble;
using Wobble.Graphics;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.DrawableLeaderboardScores
{
    public class TestScreenDrawableLeaderboardScoreView : ScreenView
    {
        public TestScreenDrawableLeaderboardScoreView(Screen screen) : base(screen)
        {
            var score = new Score()
            {
                MapMd5 = "test",
                Name = "Player",
                DateTime = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                TotalScore = 1000000,
                Grade = Grade.X,
                Accuracy = 100,
                MaxCombo = 9999,
                CountMarv = 100,
                CountPerf = 99,
                CountGreat = 98,
                CountGood = 97,
                CountOkay = 96,
                CountMiss = 95,
                Mods = (long) (ModIdentifier.Speed12X | ModIdentifier.Paused),
                PerformanceRating = 42.69f,
                IsOnline = true,
                SteamId = -1,
                PlayerId = 1,
            };

            // ReSharper disable twice ObjectCreationAsStatement
            new DrawableLeaderboardScore(null, score, 0, false)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
            };

            new DrawableLeaderboardScore(null, score, 0, true)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = 100
            };
        }

        public override void Update(GameTime gameTime)
        {
            Container?.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#2f2f2f"));
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}