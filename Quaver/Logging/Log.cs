using Microsoft.Xna.Framework;

namespace Quaver.Logging
{
    public class Log
    {
        /// <summary>
        /// The name of the Log-Tracking Object
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// What message the Log Object will display
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The color of the message
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// How long the message will be shown for.
        /// </summary>
        public float Duration { get; set; }

        /// <summary>
        /// This bool determines whether the object will be removed after the duration variable.
        /// </summary>
        public bool NoDuration { get; set; } = true;
    }

    public enum LogLevel
    {
        Success,
        Info,
        Important,
        Warning,
        Error
    }

    public enum LogType
    {
        Runtime,
        Network
    }
}