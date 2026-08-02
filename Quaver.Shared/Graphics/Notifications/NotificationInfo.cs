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
        ///     The changing portion of <see cref="Text"/> that should be highlighted when this notification is reused.
        /// </summary>
        public string HighlightedValue { get; }

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
        /// <param name="highlightedValue"></param>
        public NotificationInfo(NotificationLevel level, string text, bool automaticallySlide,
            EventHandler clickAction = null, bool forceShow = false, string highlightedValue = null)
        {
            Level = level;
            Text = text;
            HighlightedValue = highlightedValue;

            AutomaticallySlide = automaticallySlide;
            ClickAction = clickAction;
            ForceShow = forceShow;
        }
    }
}