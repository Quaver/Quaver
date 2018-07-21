using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Config;
using Quaver.Graphics.Text;
using Quaver.Main;

namespace Quaver.Logging
{
    internal class Logger
    {
        /// <summary>
        ///     The list of all of the current logs.
        /// </summary>
        private static List<Log> Logs { get; set; } = new List<Log>();

        /// <summary>
        ///     The SpriteFont for the logs
        /// </summary>
        private static SpriteFont Font { get; set; }

        /// <summary>
        ///     The path of the current runtime log
        /// </summary>
        private static string RuntimeLogPath { get; set; }

        /// <summary>
        ///     The path of the network log
        /// </summary>
        private static string NetworkLogPath { get; set; }

        /// <summary>
        ///     Creates the log file
        /// </summary>
        internal static void CreateLogFile()
        {
            RuntimeLogPath = ConfigManager.LogsDirectory + "/runtime.log";
            NetworkLogPath = ConfigManager.LogsDirectory + "/network.log";

            try
            {
                if (!File.Exists(RuntimeLogPath))
                    using (File.Create(RuntimeLogPath)) { }

                if (!File.Exists(NetworkLogPath))
                    using (File.Create(NetworkLogPath)) { }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        ///     Initializes the logger.
        /// </summary>
        internal static void Initialize()
        {
            Font = Fonts.Medium12;
        }

        /// <summary>
        ///     Adds a log to our current list.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="color"></param>
        [Conditional("DEBUG")]
        internal static void Add(string name, string value, Color color)
        {
            if (GameBase.Content == null)
                return;

            Logs.Add(new Log()
            {
                Name = name,
                Color = color,
                Value = value
            });
        }

        /// <summary>
        ///     Updates a logger's values.
        /// </summary>
        [Conditional("DEBUG")]
        internal static void Update(string name, string value)
        {
            if (GameBase.Content == null)
                return;

            // Find the log with the correct name passed.
            var log = Logs.FirstOrDefault(x => x.Name == name);

            if (log != null)
                log.Value = value;
        }

        /// <summary>
        ///     Removes a log from the current list.
        /// </summary>
        /// <param name="name"></param>
        [Conditional("DEBUG")]
        internal static void Remove(string name)
        {
            if (GameBase.Content == null)
                return;

            Logs.RemoveAll(x => x.Name == name);
        }

        /// <summary>
        ///     Clears all of the logs from the list.
        /// </summary>
        [Conditional("DEBUG")]
        internal static void Clear()
        {
            if (GameBase.Content == null)
                return;

            Logs.Clear();
        }

        /// <summary>
        ///     Logs a success message of a give type
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="duration"></param>
        internal static void LogSuccess(string value, LogType type, float duration = 1.5f)
        {
            Log(value, LogLevel.Success, type, duration);
        }

        /// <summary>
        ///     Logs an info message of a give type
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="duration"></param>
        internal static void LogInfo(string value, LogType type, float duration = 1.5f)
        {
            Log(value, LogLevel.Info, type, duration);
        }

        /// <summary>
        ///     Logs an important message of a give type
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="duration"></param>
        internal static void LogImportant(string value, LogType type, float duration = 1.5f)
        {
            Log(value, LogLevel.Important, type, duration);
        }

        /// <summary>
        ///     Logs a success message of a give type
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="duration"></param>
        internal static void LogWarning(string value, LogType type, float duration = 1.5f)
        {
            Log(value, LogLevel.Warning, type, duration);
        }

        /// <summary>
        ///     Logs an error message of a give type
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="duration"></param>
        internal static void LogError(string value, LogType type, float duration = 1.5f)
        {
            Log(value, LogLevel.Error, type, duration);
        }

        /// <summary>
        ///     Logs an exception error.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="type"></param>
        /// <param name="duration"></param>
        internal static void LogError(Exception exception, LogType type, float duration = 1.5f)
        {
            Log(exception.ToString(), LogLevel.Error, type, duration);
        }

        /// <summary>
        ///     Logs a message to the screen, console, and runtime log
        /// </summary>
        internal static void Log(string value, LogLevel level, LogType type, float duration = 1.5f)
        {
            // Get a stringified version of the log type, and set the path that will be used
            var logTypeStr = "";
            var logPath = "";
            switch (type)
            {
                case LogType.Runtime:
                    if (RuntimeLogPath == "")
                        return;

                    logTypeStr = "RUNTIME";
                    logPath = RuntimeLogPath;
                    break;
                case LogType.Network:
                    if (NetworkLogPath == "")
                        return;

                    logTypeStr = "NETWORK";
                    logPath = NetworkLogPath;
                    break;
            }

            // Get a stringified version of the log level, and also set the color.
            var logLevelStr = "";
            var logColor = Color.Beige;
            switch (level)
            {
                case LogLevel.Info:
                    logLevelStr = "INFO";
                    logColor = Color.LightBlue;
                    break;
                case LogLevel.Error:
                    logLevelStr = "ERROR";
                    logColor = Color.Red;
                    break;
                case LogLevel.Important:
                    logLevelStr = "IMPORTANT";
                    logColor = Color.HotPink;
                    break;
                case LogLevel.Success:
                    logLevelStr = "SUCCESS";
                    logColor = Color.Green;
                    break;
                case LogLevel.Warning:
                    logLevelStr = "WARNING";
                    logColor = Color.Yellow;
                    break;
            }

            // Format the log
            var log = $"[{DateTime.Now:h:mm:ss}] - {logTypeStr} - {logLevelStr}: {value}";         
            Console.WriteLine(log);

            // Write to the log file
            try
            {
                using (var sw = new StreamWriter(logPath, true))
                {
                    sw.AutoFlush = true;
                    sw.WriteLine(log);
                }
            }
            catch (Exception e)
            {
                // If it fails, we can't really handle the error here. This shouldn't happen though.
                Console.WriteLine(e);
            }

#if DEBUG
            if (GameBase.Content == null)
                return;

            try
            {
                Logs.Add(new Log()
                {
                    Name = "LogMethod",
                    Color = logColor,
                    Duration = duration,
                    NoDuration = false,
                    Value = log
                });
            }
            catch (Exception e)
            {
                // If it fails, we can't really handle the error here. This shouldn't happen though.
                Console.WriteLine(e);
            }
#endif
        }

        /// <summary>
        ///     Draws the logs onto the screen.
        /// </summary>
        [Conditional("DEBUG")]
        internal static void Draw(double dt)
        {
            if (GameBase.Content == null)
                return;

            for (var i = 0; i < Logs.Count; i++)
            {
                if (Logs[i].Value == null)
                    continue;

                GameBase.SpriteBatch.DrawString(Font, Logs[i].Value, new Vector2(0, i * 20 + 40), Logs[i].Color);

                if (Logs[i].NoDuration)
                    continue;

                Logs[i].Duration -= (float)dt / 1000f;

                if (Logs[i].Duration > 0)
                    continue;

                Logs.RemoveAt(i);
                i--;
            }
        }

        public static void LogInfo(string value)
        {
            throw new NotImplementedException();
        }
    }
}
