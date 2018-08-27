using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Quaver.Logging;
using Quaver.Online;
using SteamworksSharp;
using SteamworksSharp.Native;

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

            SteamNative.Initialize();
            SteamApi.RestartAppIfNecessary((uint) SteamManager.ApplicationId);

            if (SteamApi.IsSteamRunning())
            {
                if (SteamApi.Initialize(SteamManager.ApplicationId))
                {
                    using (var game = new QuaverGame())
                    {
                        game.Run();
                    }
                }
                else
                {
                    Logger.LogError($"SteamAPI failed to initialize!", LogType.Runtime);
                }
            }
            else
            {
                Logger.LogError($"Game started but Steam is not running", LogType.Runtime);
            }
        }
    }
}
