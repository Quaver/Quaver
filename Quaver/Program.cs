using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Quaver.QuaFile;
using Quaver.Config;
using Quaver.Tests;

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
            Task.Run(() => RunTestMethods());

            // Start game
            using (var game = new Game1())
            {
                game.Run();
            }
        }

        /// <summary>
        /// This'll run all of the test methods in our code. 
        /// They should be marked with [Conditional("DEBUG")]
        /// These will only be ran when the solution was built in debug mode.
        /// All functions in this method should have an argument "run",
        /// which specifies whether or not you want to run the specific method
        /// </summary>
        [Conditional("DEBUG")]
        private static void RunTestMethods()
        {
            QuaTest.ParseQuaTest(true);
            SkinTest.ParseSkinTest(true);
        }
    }
}