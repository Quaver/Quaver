using System;
using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning;
using Wobble.Bindables;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Multi.UI.Status
{
    public class DrawableMultiplayerGameStatus : SpriteTextPlus
    {
        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> Game { get; }

        /// <summary>
        ///     The amount of periods in the ellipsis
        /// </summary>
        private int EllipsisPeriodCount { get; set; }

        /// <summary>
        /// </summary>
        private double TimeSinceLastEllipisCountChange { get; set; }

        /// <summary>
        /// </summary>
        public bool CompletedCountdownSecondInterval { get; set; }

        /// <summary>
        /// </summary>
        private int LastNearestCountdownSecond { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        public DrawableMultiplayerGameStatus(Bindable<MultiplayerGame> game)
            : base(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
        {
            Game = game;
            UpdateText();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (Game.Value.CountdownStartTime != -1)
                UpdateTextState(false);

            UpdateEllipsisPeriodCount(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void UpdateText(bool resetEllipsis = true) => ScheduleUpdate(() => UpdateTextState(resetEllipsis));

        /// <summary>
        /// </summary>
        /// <param name="resetEllipsis"></param>
        private void UpdateTextState(bool resetEllipsis)
        {
            if (Game.Value.InProgress)
                Text = "A match is currently in progress. Please wait until it finishes";
            else if (Game.Value.HostSelectingMap)
                Text = "The host is currently selecting a map";
            else if (Game.Value.CountdownStartTime != -1)
            {
                UpdateCountdownTimer();
                Text = $"The match is starting in {Math.Abs(LastNearestCountdownSecond) + 1} seconds. Get Ready!";
            }
            else
                Text = "Waiting for the host to start";

            if (!resetEllipsis)
                return;

            EllipsisPeriodCount = 0;
            TimeSinceLastEllipisCountChange = 0;
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdateEllipsisPeriodCount(GameTime gameTime)
        {
            TimeSinceLastEllipisCountChange += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (TimeSinceLastEllipisCountChange < 400)
                return;

            if (EllipsisPeriodCount == 3)
            {
                UpdateText();
                return;
            }

            Text += ".";
            EllipsisPeriodCount++;
            TimeSinceLastEllipisCountChange = 0;
        }

        /// <summary>
        /// </summary>
        private void UpdateCountdownTimer()
        {
            var timeLeft = (int) Math.Abs((DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - OnlineManager.CurrentGame.CountdownStartTime) / 1000);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (LastNearestCountdownSecond != timeLeft)
                CompletedCountdownSecondInterval = false;

            if (timeLeft >= LastNearestCountdownSecond && !CompletedCountdownSecondInterval)
            {
                SkinManager.Skin?.SoundHover?.CreateChannel()?.Play();
                CompletedCountdownSecondInterval = true;
            }

            LastNearestCountdownSecond = timeLeft;
        }
    }
}