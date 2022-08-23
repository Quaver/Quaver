using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.Selection.UI.Leaderboard.Components;
using WebSocketSharp;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Options.Items
{
    public class OptionsItem : Sprite
    {
        /// <summary>
        /// </summary>
        public SpriteTextPlus Name { get; protected set; }

        /// <summary>
        /// </summary>
        protected RectangleF ContainerRectangle { get; set; }

        /// <summary>
        ///     Any tags to search the item by
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        private FadeableButton TooltipArea;

        private Tooltip Tooltip;

        /// <summary>
        /// </summary>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        /// <param name="tooltipText"></param>
        public OptionsItem(RectangleF containerRect, string name, string tooltipText = null)
        {
            ContainerRectangle = containerRect;

            Image = UserInterface.OptionsItemBackground;
            Size = new ScalableVector2(containerRect.Width * 0.96f, 54);

            Tint = ColorHelper.HexToColor("#242424");

            CreateName(name);
            UsePreviousSpriteBatchOptions = true;

            CreateTooltip(tooltipText);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var color = IsHovered() ? ColorHelper.HexToColor("#3F3F3F") : ColorHelper.HexToColor("#242424");

            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
            FadeToColor(color, dt, 20);

            // Set visibility based on if the options item is visible inside of the container.
            // Helps to raise FPS by not drawing unnecessary items
            if (Parent is Drawable contentContainer && contentContainer.Parent is ScrollContainer container)
                Visible = !RectangleF.Intersection(ScreenRectangle, container.ScreenRectangle).IsEmpty;
            else
                Visible = true;

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        private void CreateName(string name)
        {
            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), name, 21)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 16,
                UsePreviousSpriteBatchOptions = true,
            };
        }

        private void CreateTooltip(string text)
        {
            if (text.IsNullOrEmpty()) return;

            TooltipArea = new FadeableButton(UserInterface.NotificationInfo)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(15, 15),
                X = Name.X + Name.Width + 10,
                Y = Name.Y,
            };

            Tooltip = new Tooltip(text, ColorHelper.HexToColor("#5dc7f9"));

            var game = GameBase.Game as QuaverGame;
            TooltipArea.Hovered += (sender, args) => game?.CurrentScreen.ActivateTooltip(Tooltip);
            TooltipArea.LeftHover += (sender, args) => game?.CurrentScreen.DeactivateTooltip();
        }
    }
}