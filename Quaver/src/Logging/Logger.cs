using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using Quaver.Graphics.Text;

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
        ///     Creates the log file
        /// </summary>
        internal static void CreateLogFile()
        {
            RuntimeLogPath = Configuration.LogsDirectory + "/" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".log";

            try
            {
                var file = File.Create(RuntimeLogPath);
                file.Close();
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
        internal static void Add(string name, string value, Color color)
        {
            if (!Configuration.Debug)
                return;

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
        internal static void Update(string name, string value)
        {
            if (!Configuration.Debug)
                return;

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
        internal static void Remove(string name)
        {
            if (!Configuration.Debug)
                return;

            if (GameBase.Content == null)
                return;

            Logs.RemoveAll(x => x.Name == name);
        }

        /// <summary>
        ///     Clears all of the logs from the list.
        /// </summary>
        internal static void Clear()
        {
            if (!Configuration.Debug)
                return;

            if (GameBase.Content == null)
                return;

            Logs.Clear();
        }

        /// <summary>
        ///     Logs a message to the screen, console, and runtime log
        /// </summary>
        internal static void Log(string value, Color color, float duration = 2.5f)
        {
            if (RuntimeLogPath == null)
                return;

            // Put the time in front of the incoming log 
            value = "[" +DateTime.Now.ToString("h:mm:ss") + "] " + value;
            Console.WriteLine(value);

            // Write to the log file
            try
            {
                var sw = new StreamWriter(RuntimeLogPath, true)
                {
                    AutoFlush = true
                };

                sw.WriteLine(value);
                sw.Close();
            }
            catch (Exception e)
            {

            }

            if (!Configuration.Debug)
                return;

            if (GameBase.Content == null)
                return;

            Logs.Add(new Log()
            {
                Name = "LogMethod",
                Color = color,
                Duration = duration,
                NoDuration = false,
                Value = value
            });
        }

        /// <summary>
        ///     Draws the logs onto the screen.
        /// </summary>
        internal static void Draw(double dt)
        {
            if (!Configuration.Debug)
                return;

                        if (GameBase.Content == null)
                return;

            for (var i = 0; i < Logs.Count; i++)
            {
                if (Logs[i].Value == null)
                    continue;

                GameBase.SpriteBatch.DrawString(Font, Logs[i].Value, new Vector2(0, i * 20), Logs[i].Color);

                if (Logs[i].NoDuration)
                    continue;

                Logs[i].Duration -= (float)dt / 1000f;

                if (Logs[i].Duration > 0)
                    continue;

                Logs.RemoveAt(i);
                i--;
            }
        }
    }
}
