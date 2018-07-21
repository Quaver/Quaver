using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Graphics.Base;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.Graphics.UI.Notifications
{
    internal static class NotificationManager
    {
        /// <summary>
        ///     The sprite container for our notifications.
        /// </summary>
        private static Container Container { get; } = new Container();

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
        private static Vector2 InitialPosition { get; } = new Vector2(350, 120);

        /// <summary>
        ///
        /// </summary>
        /// <param name="dt"></param>
        internal static void Update(double dt)
        {
            PerformAnimations(dt);
            Container.Update(dt);
        }

         /// <summary>
        ///
        /// </summary>
        internal static void Draw() => Container.Draw();

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
                    image = UserInterface.NotificationInfo;
                    break;
                case NotificationLevel.Error:
                    color = ColorHelper.HexToColor("#D9534F");
                    image = UserInterface.NotificationError;
                    break;
                case NotificationLevel.Warning:
                    color = Color.Yellow;
                    image = UserInterface.NotificationWarning;
                    break;
                case NotificationLevel.Success:
                    color = ColorHelper.HexToColor("#5CB85C");
                    image = UserInterface.NotificationSuccess;
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
                Position = new UDim2D(InitialPosition.X, InitialPosition.Y),
            };

            notification.PosX = notification.SizeX;
            Notifications.Add(notification);
        }

        /// <summary>
        ///     Performs all of the animations for
        /// </summary>
        private static void PerformAnimations(double dt)
        {
            if (Notifications.Count == 0)
                return;

            for (var i = Notifications.Count - 1; i >= 0; i--)
            {
                var n = Notifications[i];

                // Tween the notification to the left
                if (Math.Abs(n.PosX - TargetX) > 0.02)
                    n.PosX = GraphicsHelper.Tween(TargetX, n.PosX, Math.Min(dt / 60, 1));

                // Get the current iteration
                var iteration = Notifications.Count - 1 - i;

                // Calculate the new target y position
                var targetY = InitialPosition.Y + (Notifications.Last().SizeY + 20) * iteration;

                if (Math.Abs(n.PosY - targetY) > 0.02)
                    n.PosY = GraphicsHelper.Tween(targetY, n.PosY, Math.Min(dt / 60, 1));
                else
                {
                    // Since the notification is now in the correct x position, we can
                    // begin counting the time it has been shown, so we'll know when to fade it out.
                    n.TimeElapsedSinceShown += dt;

                    // Reset the alpha if the button is hovered over, and it hasn't already been clicked.
                    if (n.IsTrulyHovered && !n.HasBeenClicked)
                    {
                        n.Alpha = 1;
                        n.TimeElapsedSinceShown = 0;
                    }
                    // Begin to fade out and destroy the object after a period of time.
                    else if (Math.Abs(n.Alpha) < 0.02)
                    {
                        Notifications.Remove(n);
                        n.Destroy();
                    }
                }
            }
        }
    }
}