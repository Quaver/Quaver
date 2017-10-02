using System;
using System.IO;
using System.Threading.Tasks;
using Quaver.Config;
using System.Reflection;
using Microsoft.Xna.Framework.Input;

namespace Quaver
{
    /// <summary>
    ///     The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Configuration.InitializeConfig();
            // Start game
            using (var game = new Game1())
            {
                game.Run();

            }
        }
    }
}