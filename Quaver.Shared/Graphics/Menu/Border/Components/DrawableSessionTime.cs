using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Menu.Border.Components
{
    public class DrawableSessionTime : ImageButton, IMenuBorderItem
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UseCustomPaddingY { get; } = true;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public int CustomPaddingY { get; } = 0;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UseCustomPaddingX { get; } = true;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public int CustomPaddingX { get; } = 34;

        /// <summary>
        ///     The time that the game has been running for
        /// </summary>
        public SpriteTextPlus Time { get; }

        /// <summary>
        /// </summary>
        private Tooltip Tooltip { get; }

        /// <summary>
        ///     The time in the previous frame
        /// </summary>
        private double TimeSinceLastSecond { get; set; }

        /// <summary>
        ///     The original timespan that the clock started at
        /// </summary>
        private TimeSpan Clock { get; set; }

        /// <summary>
        /// </summary>
        public DrawableSessionTime() : base(UserInterface.DropdownClosed)
        {
            Size = new ScalableVector2(100, 26);
            Tint = ColorHelper.HexToColor($"#363636");

            Clock = TimeSpan.FromMilliseconds(GameBase.Game.TimeRunning);

            Time = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoHeavy), $"{Clock.Hours:00}:{Clock.Minutes:00}:{Clock.Seconds:00}", 19)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Y = 1,
            };

            Tooltip = new Tooltip($"This displays how long the game has been running. Be sure to take breaks often!",
                Colors.MainAccent) {DestroyIfParentIsNull = false};

            Hovered += (sender, args) =>
            {
                var game = (QuaverGame) GameBase.Game;
                game.CurrentScreen?.ActivateTooltip(Tooltip);
            };

            LeftHover += (sender, args) =>
            {
                var game = (QuaverGame) GameBase.Game;
                game.CurrentScreen?.DeactivateTooltip();
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            ChangeTime();
            base.Update(gameTime);
        }

        /// <summary>
        ///     Changes the time if a second has passed
        /// </summary>
        private void ChangeTime()
        {
            TimeSinceLastSecond += GameBase.Game.TimeSinceLastFrame;

            if (!(TimeSinceLastSecond >= 1000))
                return;

            Clock = Clock.Add(TimeSpan.FromSeconds(1));

            Time.Text = $"{Clock.Hours:00}:{Clock.Minutes:00}:{Clock.Seconds:00}";
            TimeSinceLastSecond = 0;
        }
    }
}