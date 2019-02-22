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
using Quaver.Shared.Helpers;
using Wobble.Graphics;

namespace Quaver.Shared.Graphics.Notifications
{
    public static class NotificationManager
    {
        /// <summary>
        ///     The sprite container for our notifications.
        /// </summary>
        public static Container Container { get; } = new Container();

        /// <summary>
        ///     All of the notifications in our queue.
        /// </summary>
        private static List<Notification> Notifications { get; } = new List<Notification>();

        /// <summary>
        ///     The x position of all notifications.
        /// </summary>
        private static int TargetX { get; } = -20;

        /// <summary>
        ///     The initial position every queued notification.
        /// </summary>
        private static Vector2 InitialPosition { get; } = new Vector2(350, 130);

        ///  <summary>
        ///
        ///  </summary>
        /// <param name="gameTime"></param>
        public static void Update(GameTime gameTime)
        {
            PerformAnimations(gameTime);
            Container.Update(gameTime);
        }

        /// <summary>
        ///
        /// </summary>
        internal static void Draw(GameTime gameTime) => Container.Draw(gameTime);

        /// <summary>
        ///     Show a notification with a given type.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="text"></param>
        /// <param name="onClick"></param>
        internal static void Show(NotificationLevel level, string text, EventHandler onClick = null)
        {
            Color color;
            var image = UserInterface.UnknownAvatar;

            switch (level)
            {
                case NotificationLevel.Default:
                    color = Colors.Swan;
                    break;
                case NotificationLevel.Primary:
                    color = ColorHelper.HexToColor("#428BCA");
                    break;
                case NotificationLevel.Info:
                    color = ColorHelper.HexToColor("#5BC0DE");
                    image = UserInterface.NotificationInfoBg;
                    break;
                case NotificationLevel.Error:
                    color = ColorHelper.HexToColor("#D9534F");
                    image = UserInterface.NotificationErrorBg;
                    break;
                case NotificationLevel.Warning:
                    color = Color.Yellow;
                    image = UserInterface.NotificationWarningBg;
                    break;
                case NotificationLevel.Success:
                    color = ColorHelper.HexToColor("#5CB85C");
                    image = UserInterface.NotificationSuccessBg;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }

            Show(image, color, text, onClick);
        }

        /// <summary>
        ///     Show a completely custom notification.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="color"></param>
        /// <param name="text"></param>
        /// <param name="onClick"></param>
        internal static void Show(Texture2D image, Color color, string text, EventHandler onClick = null)
        {
            var notification = new Notification(image, text, color, onClick)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Position = new ScalableVector2(InitialPosition.X, InitialPosition.Y),
            };

            notification.X = notification.Width;
            Notifications.Add(notification);
        }

        /// <summary>
        ///     Performs all of the animations for
        /// </summary>
        private static void PerformAnimations(GameTime gameTime)
        {
            if (Notifications.Count == 0)
                return;

            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            for (var i = Notifications.Count - 1; i >= 0; i--)
            {
                var notification = Notifications[i];

                // Tween the notification to the left
                if (Math.Abs(notification.X - TargetX) > 0.02)
                    notification.X = MathHelper.Lerp(notification.X, TargetX, (float) Math.Min(dt / 60, 1));

                // Get the current iteration
                var iteration = Notifications.Count - 1 - i;

                // Calculate the new target y position
                var targetY = InitialPosition.Y + (Notifications.Last().Height + 20) * iteration;

                if (Math.Abs(notification.Y - targetY) > 0.02)
                    notification.Y = MathHelper.Lerp(notification.Y, targetY, (float) Math.Min(dt / 60, 1));
                else
                {
                    // Since the notification is now in the correct x position, we can
                    // begin counting the time it has been shown, so we'll know when to fade it out.
                    notification.TimeElapsedSinceShown += dt;

                    // Reset the alpha if the button is hovered over, and it hasn't already been clicked.
                    if (notification.IsHovered && !notification.HasBeenClicked)
                    {
                        notification.Alpha = 1;
                        notification.TimeElapsedSinceShown = 0;
                    }
                    // Begin to fade out and destroy the object after a period of time.
                    else if (Math.Abs(notification.Alpha) < 0.02)
                    {
                        Notifications.Remove(notification);
                        notification.Destroy();
                    }
                }
            }
        }
    }
}
