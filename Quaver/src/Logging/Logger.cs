using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        private static SpriteFont Font { get; } = GameBase.Content.Load<SpriteFont>("testFont");

        /// <summary>
        ///     Adds a log to our current list.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="color"></param>
        internal static void Add(string name, string value, Color color)
        {
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
            // Find the log with the correct name passed.
            var log = Logs.FirstOrDefault(x => x.Name == name);

            if (log != null)
                log.Value = value;
            else
                Console.WriteLine($"Error: Log with the name {name} has not been found.");
        }

        /// <summary>
        ///     Removes a log from the current list.
        /// </summary>
        /// <param name="name"></param>
        internal static void Remove(string name)
        {
            Logs.RemoveAll(x => x.Name == name);
        }

        /// <summary>
        ///     Clears all of the logs from the list.
        /// </summary>
        internal static void Clear()
        {
            Logs.Clear();
        }

        /// <summary>
        ///     Logs a message to the screen, console, and runtime log
        /// </summary>
        internal static void Log(string value, Color color, float duration = 1.5f)
        {
            Logs.Add(new Log()
            {
                Name = "LogMethod",
                Color = color,
                Duration = duration,
                NoDuration = false,
                Value = DateTime.Now.ToString("h:mm:ss") + " " + value
            });
        }

        /// <summary>
        ///     Draws the logs onto the screen.
        /// </summary>
        internal static void Draw(double dt)
        {
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
