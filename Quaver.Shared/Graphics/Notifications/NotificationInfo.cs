using System;

namespace Quaver.Shared.Graphics.Notifications
{
    public class NotificationInfo
    {
        /// <summary>
        /// </summary>
        public NotificationLevel Level { get; }

        /// <summary>
        /// </summary>
        public string Text { get; }

        public NotificationType Type { get; }

        public string SenderName { get; }

        public long SenderSteamId { get; }

        public string DetailText { get; }

        /// <summary>
        ///     When the notification was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; }

        /// <summary>
        /// </summary>
        public EventHandler ClickAction { get; }

        /// <summary>
        /// </summary>
        public bool WasClicked { get; set; }

        /// <summary>
        ///     Whether this notification has already been added to the hub feeds.
        /// </summary>
        internal bool StoredInHub { get; set; }

        /// <summary>
        /// </summary>
        public bool AutomaticallySlide { get; }

        /// <summary>
        ///     If the notification will be shown regardless of the state
        /// </summary>
        public bool ForceShow { get; }

        /// <summary>
        /// </summary>
        /// <param name="level"></param>
        /// <param name="text"></param>
        /// <param name="automaticallySlide"></param>
        /// <param name="clickAction"></param>
        /// <param name="forceShow"></param>
        public NotificationInfo(NotificationLevel level, string text, bool automaticallySlide, EventHandler clickAction = null,
            bool forceShow = false, DateTimeOffset? createdAt = null, NotificationType type = NotificationType.General,
            string senderName = null, long senderSteamId = 0, string detailText = null)
        {
            Level = level;
            Text = text;
            CreatedAt = createdAt ?? DateTimeOffset.Now;
            Type = type;
            SenderName = senderName;
            SenderSteamId = senderSteamId;
            DetailText = detailText;

            AutomaticallySlide = automaticallySlide;
            ClickAction = clickAction;
            ForceShow = forceShow;
        }
    }
}