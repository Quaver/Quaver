using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Discord
{
    internal class DiscordController
    {
        public DiscordRPC.RichPresence presence;
        DiscordRPC.EventHandlers handlers;
        public string applicationId = "376180410490552320";
        public string optionalSteamId;

        /// <summary>
        ///     Initializes Discord RPC
        /// </summary>
        public void Initialize()
        {
            handlers = new DiscordRPC.EventHandlers();
            handlers.readyCallback = ReadyCallback;
            handlers.disconnectedCallback += DisconnectedCallback;
            handlers.errorCallback += ErrorCallback;
            DiscordRPC.Initialize(applicationId, ref handlers, true, optionalSteamId);
        }

        public void ReadyCallback()
        {
            Console.WriteLine("Discord RPC is ready!");
        }

        public void DisconnectedCallback(int errorCode, string message)
        {
            Console.WriteLine($"Error: {errorCode} - {message}");
        }

        public void ErrorCallback(int errorCode, string message)
        {
            Console.WriteLine($"Error: {errorCode} - {message}");
        }
    }
}
