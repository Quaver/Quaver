/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Gameplay;
using Wobble;
using Wobble.Graphics;
using Wobble.Logging;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Notifications
{
    public static class NotificationManager
    {
        /// <summary>
        ///     The sprite container for our notifications.
        /// </summary>
        public static Container Container { get; } = new Container();

        /// <summary>
        ///     Notifications that are queued to be displayed
        /// </summary>
        public static List<DrawableNotification> QueuedNotifications { get; set; } = new List<DrawableNotification>();

        /// <summary>
        ///     Notifications that are currently active
        /// </summary>
        public static List<DrawableNotification> ActiveNotifications { get; set; } = new List<DrawableNotification>();

        /// <summary>
        ///     Notifications from <see cref="QueuedNotifications"/> that can be cleared from the list
        /// </summary>
        private static List<DrawableNotification> NotificationsToClear { get; set; } = new List<DrawableNotification>();

        /// <summary>
        ///     Notifications that should be reused while they're queued or active.
        /// </summary>
        private static Dictionary<string, DrawableNotification> KeyedNotifications { get; } = new Dictionary<string, DrawableNotification>();

        /// <summary>
        ///     Notifications list used for the hub's new notification section
        /// </summary>
        public static List<NotificationInfo> NewNotificationsFeed { get; } = new List<NotificationInfo>();

        /// <summary>
        ///     Notifications list used for the hub's history notification section
        /// </summary>
        public static List<NotificationInfo> HistoryNotificationsFeed { get; }  = new List<NotificationInfo>();

        /// <summary>
        ///     Event invoked when a notification has been missed by the user
        /// </summary>
        public static event EventHandler<NotificationMissedEventArgs> NotificationMissed;

        /// <summary>
        ///     Invoked when a queued notification becomes visible.
        /// </summary>
        public static event Action<NotificationInfo> NotificationShown;

        /// <summary>
        ///     Invoked when a shown notification is clicked and removed from the new feed.
        /// </summary>
        public static event Action<NotificationInfo> NotificationViewed;

        /// <summary>
        ///     The initial/top level position for notifications
        /// </summary>
        private static float InitialY { get; } = 130;

        ///  <summary>
        ///  </summary>
        /// <param name="gameTime"></param>
        public static void Update(GameTime gameTime)
        {
            Container.Width = WindowManager.Width;
            Container.Height = WindowManager.Height;

            FlushNotificationQueue();
            PerformAnimations(gameTime);
            Container.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        internal static void Draw(GameTime gameTime) => Container.Draw(gameTime);

        /// <summary>
        ///     Show a notification with a given type.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="text"></param>
        /// <param name="onClick"></param>
        /// <param name="forceShow"></param>
        internal static void Show(NotificationLevel level, string text, EventHandler onClick = null, bool forceShow = false,
            NotificationType type = NotificationType.General, string senderName = null, long senderSteamId = 0,
            string detailText = null)
        {
            var info = new NotificationInfo(level, text, true, onClick, forceShow, type: type, senderName: senderName,
                senderSteamId: senderSteamId, detailText: detailText);
            var notification = new DrawableNotification(null, info, -1) { Alignment = Alignment.TopRight };

            lock (QueuedNotifications)
            {
                QueuedNotifications.Add(notification);
            }
        }

        internal static bool StoreShownNotification(NotificationInfo info)
        {
            if (info.StoredInHub || (info.Level != NotificationLevel.Error && info.ClickAction == null))
                return false;

            StoreInHub(info, false);
            return true;
        }

        internal static void MarkClicked(NotificationInfo info)
        {
            info.WasClicked = true;

            if (info.StoredInHub && NewNotificationsFeed.Remove(info))
                NotificationViewed?.Invoke(info);
        }

        private static void StoreInHub(NotificationInfo info, bool missed)
        {
            info.StoredInHub = true;
            NewNotificationsFeed.Insert(0, info);
            HistoryNotificationsFeed.Insert(0, info);

            if (missed)
                NotificationMissed?.Invoke(typeof(NotificationManager), new NotificationMissedEventArgs(info));
        }

        /// <summary>
        ///     Shows a notification, or updates the existing notification with the same key while it's visible.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="level"></param>
        /// <param name="text"></param>
        /// <param name="onClick"></param>
        /// <param name="forceShow"></param>
        internal static void ShowOrUpdate(string key, NotificationLevel level, string text, EventHandler onClick = null, bool forceShow = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                Show(level, text, onClick, forceShow);
                return;
            }

            var info = new NotificationInfo(level, text, true, onClick, forceShow);

            lock (QueuedNotifications)
            {
                if (KeyedNotifications.TryGetValue(key, out var existing))
                {
                    if (QueuedNotifications.Contains(existing))
                    {
                        existing.UpdateContent(info, existing.Index);
                        return;
                    }

                    if (ActiveNotifications.Contains(existing))
                    {
                        existing.Refresh(info);
                        return;
                    }

                    KeyedNotifications.Remove(key);
                }

                var notification = new DrawableNotification(null, info, -1) { Alignment = Alignment.TopRight };

                QueuedNotifications.Add(notification);
                KeyedNotifications.Add(key, notification);
            }
        }

        /// <summary>
        ///     Moves all of the notifications that are queued in <see cref="QueuedNotifications"/>
        ///     and makes them active
        /// </summary>
        private static void FlushNotificationQueue()
        {
            var game = GameBase.Game as QuaverGame;

            lock (QueuedNotifications)
            {
                foreach (var notification in QueuedNotifications)
                {
                    // Prevent unimportant notifications from displaying during gameplay
                    if (game?.CurrentScreen is GameplayScreen screen && !screen.IsPaused && !notification.Item.ForceShow
                        && !ConfigManager.DisplayNotificationsInGameplay.Value)
                        continue;

                    notification.Parent = Container;

                    if (ConfigManager.DisplayNotificationsBottomToTop?.Value ?? false)
                    {
                        notification.Alignment = Alignment.BotRight;
                        notification.Y = -InitialY;
                    }
                    else
                    {
                        notification.Y = InitialY;
                    }

                    ActiveNotifications.Add(notification);
                    NotificationsToClear.Add(notification);
                    NotificationShown?.Invoke(notification.Item);
                }

                foreach (var notification in NotificationsToClear)
                    QueuedNotifications.Remove(notification);
            }

            NotificationsToClear.Clear();
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private static void PerformAnimations(GameTime gameTime)
        {
            if (ActiveNotifications.Count == 0)
                return;

            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            for (var i = ActiveNotifications.Count - 1; i >= 0; i--)
            {
                var notification = ActiveNotifications[i];

                // Get the current iteration
                var iteration = ActiveNotifications.Count - 1 - i;

                // Calculate the new target y position
                if (!notification.IsSlidingOut)
                {
                    var targetY = InitialY + (ActiveNotifications.Last().Height + 20) * iteration;

                    if (ConfigManager.DisplayNotificationsBottomToTop?.Value ?? false)
                        targetY = -targetY;

                    notification.Y = MathHelper.Lerp(notification.Y, targetY, (float)Math.Min(dt / 60, 1));
                }

                if (!notification.Item.WasClicked && !notification.HasSlidOut)
                    continue;

                notification.Destroy();
                ActiveNotifications.Remove(notification);
                RemoveKeyedNotification(notification);

                if (notification.Item.WasClicked)
                    continue;

                if (notification.Item.StoredInHub)
                    continue;

                // Consider a notification "missed" if it's an error OR it has a click action attached to it
                if (notification.Item.Level != NotificationLevel.Error && notification.Item.ClickAction == null)
                    continue;

                // Notification was missed, so invoke an event with its info so the OnlineHub can add it to its queue
                Logger.Important($"Notification Missed: {notification.Item.Level} | {notification.Item.Text}",
                    LogType.Runtime, false);

                var info = new NotificationInfo(notification.Item.Level, notification.Item.Text, false, notification.Item.ClickAction,
                    createdAt: notification.Item.CreatedAt, type: notification.Item.Type, senderName: notification.Item.SenderName, 
                    senderSteamId: notification.Item.SenderSteamId, detailText: notification.Item.DetailText);

                StoreInHub(info, true);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="notification"></param>
        private static void RemoveKeyedNotification(DrawableNotification notification)
        {
            var keyedNotification = KeyedNotifications.FirstOrDefault(x => x.Value == notification);

            if (keyedNotification.Key == null)
                return;

            KeyedNotifications.Remove(keyedNotification.Key);
        }
    }
}
