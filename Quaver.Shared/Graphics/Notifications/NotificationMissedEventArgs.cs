using System;

namespace Quaver.Shared.Graphics.Notifications
{
    public class NotificationMissedEventArgs : EventArgs
    {
        public NotificationInfo Notification { get; }

        public NotificationMissedEventArgs(NotificationInfo n) => Notification = n;
    }
}