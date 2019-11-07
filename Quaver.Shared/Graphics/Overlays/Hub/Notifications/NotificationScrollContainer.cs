using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Notifications;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.Hub.Notifications
{
    public class NotificationScrollContainer : PoolableScrollContainer<NotificationInfo>
    {
        /// <summary>
        /// </summary>
        private int PoolCountInLastFrame { get; set; }

        /// <summary>
        /// </summary>
        private const int MAX_NOTIFICATIONS = 30;

        /// <summary>
        /// </summary>
        private SpriteTextPlus NoNotifications { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public NotificationScrollContainer(ScalableVector2 size) : base(new List<NotificationInfo>(),
            int.MaxValue, 0, size, size)
        {
            Alpha = 0;
            Scrollbar.Width = 4;
            Scrollbar.Tint = Color.White;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 220;

            InputEnabled = true;

            CreateTextNoNotifications();

            NotificationManager.NotificationMissed += OnNotificationMissed;
            CreatePool();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var game = (QuaverGame) GameBase.Game;

            if (game.OnlineHub.IsOpen && game.OnlineHub.SelectedSection == game.OnlineHub.Sections[OnlineHubSectionType.Notifications])
                game.OnlineHub.SelectedSection.MarkAsRead();

            NoNotifications.Visible = Pool.Count == 0;

            base.Update(gameTime);

            if (PoolCountInLastFrame != Pool.Count)
            {
                ReAlignNotifications();

                if (Pool.Count != 0 && PoolCountInLastFrame < Pool.Count)
                    ScrollTo(-Pool.Last().Y, 1000);
            }

            PoolCountInLastFrame = Pool.Count;
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            NotificationManager.NotificationMissed -= OnNotificationMissed;
            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<NotificationInfo> CreateObject(NotificationInfo item, int index)
            => new DrawableNotification(this, item, index);

        /// <summary>
        /// </summary>
        private void CreateTextNoNotifications()
        {
            NoNotifications = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 20)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                TextAlignment = TextAlignment.Center
            };

            AddScheduledUpdate(() => NoNotifications.Text = "All clear! You do not have any\nmissed notifications.".ToUpper());
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNotificationMissed(object sender, NotificationMissedEventArgs e)
        {
            var game = (QuaverGame) GameBase.Game;
            game.OnlineHub.MarkSectionAsUnread(OnlineHubSectionType.Notifications, true);

            AddScheduledUpdate(() =>
            {
                AddObjectToBottom(e.Notification, false);
                var item = Pool.Last();

                item.Alignment = Alignment.TopCenter;
                ScrollTo(-Pool.Last().Y, 1000);

                if (Pool.Count >= MAX_NOTIFICATIONS)
                {
                    for (var i = 0; i < Pool.Count - MAX_NOTIFICATIONS; i++)
                        Remove(Pool[i].Item);
                }
            });
        }

        /// <summary>
        /// </summary>
        public void ClearAll()
        {
            Pool.ForEach(x => x.Destroy());
            Pool.Clear();
            AvailableItems.Clear();
            RecalculateContainerHeight();
        }

        /// <summary>
        ///     Removes the download from the list
        /// </summary>
        /// <param name="info"></param>
        public void Remove(NotificationInfo info, bool scroll = true)
        {
            var item = Pool.Find(x => x.Item == info);
            AvailableItems.Remove(info);

            // Remove the item if it exists in the pool.
            if (item != null)
            {
                item.Destroy();
                RemoveContainedDrawable(item);
                Pool.Remove(item);
            }

            AddScheduledUpdate(() =>
            {
                // Reset the pool item index
                for (var i = 0; i < Pool.Count; i++)
                    Pool[i].Index = i;

                ReAlignNotifications();

                if (Pool.Count != 0 && scroll)
                    ScrollTo(-Pool.Last().Y, 1000);
            });

            if (AvailableItems.Count == 0)
            {
                var game = (QuaverGame) GameBase.Game;
                game.OnlineHub.Sections[OnlineHubSectionType.Notifications].MarkAsRead();
            }
        }

        /// <summary>
        /// </summary>
        private void ReAlignNotifications()
        {
            var totalHeight = 0f;
            const int padding = 16;

            for (var i = 0; i < Pool.Count; i++)
            {
                var drawable = Pool[i];

                if (i == 0)
                {
                    drawable.Y = padding;
                    totalHeight += drawable.Y + drawable.Height;
                    continue;
                }

                var last = Pool[i - 1];
                drawable.Y = last.Y + last.Height + padding;

                totalHeight += drawable.Height + padding;
            }

            ContentContainer.Height = Math.Max(Height, totalHeight + padding);
        }
    }
}