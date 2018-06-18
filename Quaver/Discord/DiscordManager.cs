using DiscordRPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordRPC.Logging;
using Quaver.Config;
using Quaver.Logging;
using Quaver.Main;
using LogLevel = DiscordRPC.Logging.LogLevel;

namespace Quaver.Discord
{
    internal static class DiscordManager
    {
        internal static DiscordRpcClient Client { get; private set; }

        private static string ApplicationId => "376180410490552320";

        internal static RichPresence Presence { get; private set; }

        private static Assets Assets { get; set; }


        internal static void Initialize()
        {
            if (Client != null && !Client.Disposed)
                throw new Exception("DiscordRPCClient is either not null or already disposed.");

            Client = new DiscordRpcClient(ApplicationId, true, -1);

            Client.Logger = new ConsoleLogger()
            {
                Level = LogLevel.Error
            };
            
            Client.Initialize();

            Assets = new Assets
            {
                LargeImageKey = "quaver",
                LargeImageText = ConfigManager.Username.Value,
                SmallImageKey = "4k"
            };
            
            Presence = new RichPresence 
            { 
                State = "In the menus",
                Assets = Assets,
                Timestamps = new Timestamps()
            };
            
            Client.SetPresence(Presence);
        }
    }
}
