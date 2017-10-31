using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Main;

namespace Quaver.Logging
{
    /// <summary>
    /// This class will be used to track variables and any necessary logging on screen.
    /// </summary>
    static class LogManager
    {
        private static List<LogObject> _logs = new List<LogObject>();

        /// <summary>
        ///     The SpriteFont for the loggers.
        /// </summary>
        private static SpriteFont Font { get; } = GameBase.Content.Load<SpriteFont>("testFont");

        /// <summary>
        /// Create a new Log to track
        /// </summary>
        /// <param name="logName"></param>
        /// <param name="newColor"></param>
        public static void AddLogger(string logName, Color newColor)
        {
            var newLog = new LogObject()
            {
                Name = logName,
                LogColor = newColor
            };
            _logs.Add(newLog);
        }

        /// <summary>
        /// Update the current log with provided value.
        /// </summary>
        /// <param name="logName"></param>
        /// <param name="value"></param>
        public static void UpdateLogger(string logName, string value)
        {
            bool found = false;
            foreach (var current in _logs)
            {
                if (current.Name == logName)
                {
                    current.Value = value;
                    found = true;
                    break;
                }
            }
            //If no logger with given name is found
            if (!found)
            Console.WriteLine("[LogTracker.UpdateLogger] ERROR: Log with given name is not found: "+ logName);
        }

        /// <summary>
        /// This method will remove a log object with the given name.
        /// </summary>
        /// <param name="logName"></param>
        public static void RemoveLogger(string logName)
        {
            var Index = _logs.FindIndex(r => r.Name == logName);
            if (Index >= 0) _logs.RemoveAt(Index);
            else Console.WriteLine("[LogTracker.RemoveLogger] ERROR: Log with given name is not found: " + logName);
        }

        /// <summary>
        /// Creates a log that will only be displayed on screen for a limited amount of time.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="newColor"></param>
        /// <param name="duration"></param>
        public static void QuickLog(string message, Color newColor, float duration = 0.2f)
        {
            var newLog = new LogObject()
            {
                Name = "_QuickLog",
                LogColor = newColor,
                Duration = duration,
                NoDuration = false,
                Value = message
            };
            _logs.Add(newLog);
        }

        /// <summary>
        /// Draw the log objects
        /// </summary>
        public static void Draw(double dt)
        {
            for (int i=0; i<_logs.Count; i++)
            {
                LogObject current = _logs[i];
                if (current.Value != null)
                {
                    GameBase.SpriteBatch.DrawString(Font, current.Value, new Vector2(0, i * 20), current.LogColor);
                    if (!current.NoDuration)
                    {
                        current.Duration -= (float)dt;
                        if (current.Duration <= 0)
                        {
                            _logs.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
        }
    }
}
