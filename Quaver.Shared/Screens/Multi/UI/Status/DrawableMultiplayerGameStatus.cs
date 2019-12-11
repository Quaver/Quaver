using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
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
            UpdateEllipsisPeriodCount(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void UpdateText() => ScheduleUpdate(() =>
        {
            if (Game.Value.InProgress)
                Text = "A match is currently in progress. Please wait until it finishes";
            else if (Game.Value.HostSelectingMap)
                Text = "The host is currently selecting a map";
            else if (Game.Value.CountdownStartTime > 0)
                Text = "The game is starting in 5 seconds";
            else
                Text = "Waiting for the host to start";

            EllipsisPeriodCount = 0;
            TimeSinceLastEllipisCountChange = 0;
        });

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
    }
}