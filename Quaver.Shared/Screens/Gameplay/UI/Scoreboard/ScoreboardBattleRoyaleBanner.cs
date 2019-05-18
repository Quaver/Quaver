using System;
using System.Linq;
using Microsoft.Xna.Framework;
using osu_database_reader.Components.HitObjects;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Online;
using Wobble;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.UI.Scoreboard
{
    public class ScoreboardBattleRoyaleBanner : Sprite
    {
        private Scoreboard Scoreboard { get; }

        private SpriteTextBitmap PlayersLeft { get; }

        private SpriteTextBitmap TimeLeft { get; }

        public ScoreboardBattleRoyaleBanner(Scoreboard scoreboard)
        {
            Scoreboard = scoreboard;
            Size = new ScalableVector2(260, 34);
            Image = UserInterface.BattleRoyalePanel;

            PlayersLeft = new SpriteTextBitmap(FontsBitmap.GothamRegular, Scoreboard.BattleRoyalePlayersLeft + " Left")
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                FontSize = 18,
                X = 52
            };

            TimeLeft = new SpriteTextBitmap(FontsBitmap.GothamRegular, "-00:00")
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                FontSize = 18,
                X = 184
            };

            Scoreboard.BattleRoyalePlayersLeft.ValueChanged += OnBattleRoyaleRoyalePlayersLeftChanged;
        }

        public override void Update(GameTime gameTime)
        {
            UpdateTimeLeft();
            base.Update(gameTime);
        }

        /// <summary>
        ///
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            Scoreboard.BattleRoyalePlayersLeft.ValueChanged -= OnBattleRoyaleRoyalePlayersLeftChanged;
            base.Destroy();
        }

        private void OnBattleRoyaleRoyalePlayersLeftChanged(object sender, BindableValueChangedEventArgs<int> e) =>
            PlayersLeft.Text = e.Value + " Left";

        /// <summary>
        ///     Updates the amount of time left until the next elimination
        /// </summary>
        private void UpdateTimeLeft()
        {
            if (OnlineManager.CurrentGame == null)
                return;

            HitObjectInfo elimObject;

            var judgementCount = Scoreboard.Users.First().Processor.TotalJudgementCount;

            var game = (QuaverGame) GameBase.Game;
            var screen = (GameplayScreen) game.CurrentScreen;

            if (Scoreboard.BattleRoyalePlayersLeft.Value == 2)
                elimObject = screen.Map.HitObjects.Last();
            else
            {
                var nextKnockoutJudgement = GetEliminationInterval() - judgementCount % GetEliminationInterval();
                elimObject = screen.Map.HitObjects[judgementCount + nextKnockoutJudgement - 1];
            }

            var time = elimObject.StartTime - screen.Timing.Time;

            if (time < 0)
                time = 0;

            var timeLeft = new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds(time);
            TimeLeft.Text = "-" + timeLeft.ToString("mm:ss");
        }

        private int GetEliminationInterval() => OnlineManager.CurrentGame.JudgementCount / Scoreboard.Users.Count;
    }
}