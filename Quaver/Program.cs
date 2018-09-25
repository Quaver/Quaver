using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quaver
{
    public static class Program
    {
        /// <summary>
        ///     The path of the current executable.
        /// </summary>
        public static string ExecutablePath => System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace(@"file:///", "");

        /// <summary>
        ///     The current working directory of the executable.
        /// </summary>
        public static string WorkingDirectory => Path.GetDirectoryName(ExecutablePath).Replace(@"file:\", "");

        [STAThread]
        public static void Main()
        {
            // Change the working directory to where the executable is.
            Directory.SetCurrentDirectory(WorkingDirectory);

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            using (var game = new QuaverGame())
            {
                game.Run();
            }
        }
    }
}
