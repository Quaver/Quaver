using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Quaver.Notifications
{
    internal class Notification
    {
        /// <summary>
        ///     The type of notification
        /// </summary>
        private NotificationType _type;

        /// <summary>
        ///     The action the notification will do when clicked.
        /// </summary>
        private NotificationAction _action;

        /// <summary>
        ///     The content of the notification.
        /// </summary>
        private string _content;

        /// <summary>
        ///     The URL of the notification, if specified.
        /// </summary>
        private string _url;

        /// <summary>
        ///     The color of the notitfication.
        /// </summary>
        private Color _color;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="action"></param>
        /// <param name="content"></param>
        /// <param name="url"></param>
        public Notification(NotificationType type, NotificationAction action, string content, string url = "")
        {
            _type = type;
            _action = action;
            _content = content;
            _url = url;

            // Set the color
            switch (type)
            {
                case NotificationType.Error:
                    _color = new Color(255, 255, 255, 1);
                    break;
                case NotificationType.Warning:
                    _color = new Color(255, 255, 255, 1);
                    break;
                case NotificationType.Alert:
                    _color = new Color(255, 255, 255, 1);
                    break;
                case NotificationType.Message:
                    _color = new Color(255, 255, 255, 1);
                    break;
                case NotificationType.Screenshot:
                    _color = new Color(255, 255, 255, 1);
                    break;
                case NotificationType.Debug:
                    _color = new Color(255, 255, 255, 1);
                    break;
                default:
                    break;
            }

            // Determine what happens when the notification is clicked.
            switch (_action)
            {
                case NotificationAction.Disappear:
                    break;
                case NotificationAction.OpenChat:
                    break;
                case NotificationAction.OpenLink:
                    break;
                case NotificationAction.OpenReplay:
                    break;
                case NotificationAction.OpenScreenshot:
                    break;
            }
        }
    }

    /// <summary>
    ///     Defines the type of notification.
    /// </summary>
    internal enum NotificationType
    {
        Error,
        Warning,
        Alert,
        Message,
        Screenshot,
        Debug
    }

    /// <summary>
    ///     Defines the action that will take place when the notification is clicked.
    /// </summary>
    internal enum NotificationAction
    {
        Disappear,
        OpenChat,
        OpenLink,
        OpenScreenshot,
        OpenReplay
    }
}
