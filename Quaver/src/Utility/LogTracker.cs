using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Main;

namespace Quaver.Utility
{
    /// <summary>
    /// This class will be used to track variables and any necessary logging on screen.
    /// </summary>
    static class LogTracker
    {
        private static List<LogObject> _logs = new List<LogObject>();
        private static List<string> _quickLog = new List<string>();

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

        public static void RemoveLogger(string logName)
        {
            var Index = _logs.FindIndex(r => r.Name == logName);
            if (Index >= 0) _logs.RemoveAt(Index);
            else Console.WriteLine("[LogTracker.RemoveLogger] ERROR: Log with given name is not found: " + logName);
        }

        public static void QuickLog(string value)
        {
            _quickLog.Add(value);
        }

        /// <summary>
        /// Draw the log objects
        /// </summary>
        public static void Draw()
        {
            var i = 0;

            //Draw everything in the log tracker
            foreach (var current in _logs)
            {
                if (current.Value != null)
                {
                    GameBase.SpriteBatch.DrawString(Font, current.Value, new Vector2(0, i * 20), current.LogColor);
                    i++;
                }
            }

            //Draw anything in quicklog
            if (_quickLog.Count > 0)
            {
                foreach (var current in _quickLog)
                {
                    GameBase.SpriteBatch.DrawString(Font, current, new Vector2(0, i * 20), Color.Aqua);
                    i++;
                }
                _quickLog.Clear();
            }
        }
    }
}
