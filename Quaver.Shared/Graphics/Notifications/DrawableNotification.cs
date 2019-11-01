using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Graphics.Overlays.Hub.Notifications;
using Quaver.Shared.Helpers;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Notifications
{
    public class DrawableNotification : PoolableSprite<NotificationInfo>
    {
        /// <summary>
        ///     Horizontal padding between the components of a notification.
        /// </summary>
        private const int PADDING = 14;

        /// <summary>
        /// </summary>
        public override int HEIGHT { get; } = 0;

        /// <summary>
        /// </summary>
        private ImageButton Button { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Icon { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Text { get; set; }

        /// <summary>
        ///     The amount of time the notification has been inactive (not hovered)
        /// </summary>
        private double TimeInactive { get; set; }

        /// <summary>
        ///     If the notification is currently sliding out
        /// </summary>
        public bool IsSlidingOut { get; private set; }

        /// <summary>
        /// </summary>
        public bool HasSlidOut { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public DrawableNotification(PoolableScrollContainer<NotificationInfo> container, NotificationInfo item, int index) : base(container, item, index)
        {
            Size = new ScalableVector2(408, 86);
            Tint = ColorHelper.HexToColor("#242424");
            AddBorder(Color.White, 2);

            CreateButton();
            CreateIcon();
            CreateText();

            // ReSharper disable once VirtualMemberCallInConstructor
            UpdateContent(Item, Index);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Button.Size = new ScalableVector2(Width - Border.Thickness * 2, Height - Border.Thickness * 2);
            Button.Alpha = Button.IsHovered ? 0.35f : 0;

            var game = (QuaverGame) GameBase.Game;

            if (Container != null)
                Button.IsClickable = game.OnlineHub.SelectedSection == game.OnlineHub.Sections[OnlineHubSectionType.Notifications];

            if (Button.IsHovered)
                TimeInactive = 0;
            else
                TimeInactive += gameTime.ElapsedGameTime.TotalMilliseconds;

            // Automatically slide out the notification after a few seconds
            if (Item.AutomaticallySlide && !IsSlidingOut && TimeInactive >= 5000)
                SlideOut();

            // Mark the notification as having slid out
            if (Item.AutomaticallySlide && !HasSlidOut && IsSlidingOut && Animations.Count == 0)
                HasSlidOut = true;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(NotificationInfo item, int index)
        {
            Item = item;
            Index = index;

            ScheduleUpdate(() =>
            {
                if (Item.AutomaticallySlide)
                    SlideIn();

                Border.Tint = GetColor();
                Icon.Image = GetIconTexture();

                Text.Text = Item.Text;

                const int padding = 30;
                Height = Math.Max(Icon.Height + padding, Text.Height + padding);
            });
        }

        /// <summary>
        /// </summary>
        private void CreateButton()
        {
            Button = new ImageButton(UserInterface.BlankBox, (sender, args) =>
            {
                // Make the notification not clickable if it's currently sliding out
                if (IsSlidingOut)
                    return;

                Item.ClickAction?.Invoke(sender, args);
                Item.WasClicked = true;

                if (Container != null)
                {
                    var container = (NotificationScrollContainer) Container;
                    container.Remove(Item, false);
                }
            })
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Alpha = 0,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        public void SlideIn()
        {
            X = Width + 10;
            MoveToX(-30, Easing.OutQuint, 450);
        }

        /// <summary>
        /// </summary>
        public void SlideOut()
        {
            IsSlidingOut = true;
            MoveToX(Width + 10, Easing.OutQuint, 450);
        }

        /// <summary>
        /// </summary>
        private void CreateIcon()
        {
            Icon = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(34, 34),
                X = PADDING,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateText()
        {
            Text = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 20)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = PADDING + Icon.Width + PADDING,
                MaxWidth = Width - PADDING - PADDING - Icon.Width - PADDING,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private Color GetColor()
        {
            switch (Item.Level)
            {
                case NotificationLevel.Info:
                    return ColorHelper.HexToColor("#0FBAE5");
                case NotificationLevel.Error:
                    return ColorHelper.HexToColor("#F9645D");
                case NotificationLevel.Warning:
                    return ColorHelper.HexToColor("#E9B736");
                case NotificationLevel.Success:
                    return ColorHelper.HexToColor("#27B06E");
                default:
                    return ColorHelper.HexToColor("#0FBAE5");
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private Texture2D GetIconTexture()
        {
            switch (Item.Level)
            {
                case NotificationLevel.Info:
                    return UserInterface.NotificationInfo;
                case NotificationLevel.Error:
                    return UserInterface.NotificationError;
                case NotificationLevel.Warning:
                    return UserInterface.NotificationWarning;
                case NotificationLevel.Success:
                    return UserInterface.NotificationSuccess;
                default:
                    return UserInterface.NotificationInfo;
            }
        }
    }
}