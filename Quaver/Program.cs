using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Quaver.Config;
using Quaver.Logging;
using Quaver.Online;

namespace Quaver
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            ConfigManager.Initialize();
            SteamManager.Initialize();

            using (var game = new QuaverGame())
                game.Run();
        }
    }
}
