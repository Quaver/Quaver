using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Framework.Events.Packets.Structures;
using Quaver.Logging;
#if !PUBLIC
using Quaver.Framework;
using Quaver.Online;

namespace Quaver.Commands
{
    internal class SendMessage : ICommand
    {
        public string Name { get; set; } = "MSG";

        public int Args { get; set; } = 3;

        public string Description { get; set; } = "Send a message to a given channel/user";

        public string Usage { get; set; } = "msg <channel> <message text>";

        public void Execute(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Invalid Args Given. Must be 'msg <channel> <text>'");
                return;
            }

            var channel = args[1];

            var argsList = new List<string>(args);
            argsList.RemoveAt(0);
            argsList.RemoveAt(0);

            var message = new Message(channel, string.Join(" ", argsList));
            var didSend = RattleClient.SendMessage(Rattle.ChatChannels, Rattle.OnlineClients, message);

            if (!didSend)
            {
                Logger.Log("Error sending message: You are not in that channel, or the user is not online", LogColors.GameError);
                return;
            }

            Logger.Log($"{Rattle.Client.Username} @{message.Channel}: {message.Text}", LogColors.GameInfo);
        }
    }
}

#endif