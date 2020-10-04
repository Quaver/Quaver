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

        /// <summary>
        /// </summary>
        public EventHandler ClickAction { get; }

        /// <summary>
        /// </summary>
        public bool WasClicked { get; set; }

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
        public NotificationInfo(NotificationLevel level, string text, bool automaticallySlide, EventHandler clickAction = null, bool forceShow = false)
        {
            Level = level;
            Text = text;

            AutomaticallySlide = automaticallySlide;
            ClickAction = clickAction;
            ForceShow = forceShow;
        }
    }
}