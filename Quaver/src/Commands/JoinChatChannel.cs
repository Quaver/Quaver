using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Logging;
#if !PUBLIC
using Quaver.Framework;
using Quaver.Online;

namespace Quaver.Commands
{
    internal class JoinChatChannel : ICommand
    {
        public string Name { get; set; } = "JOINCHAT";

        public int Args { get; set; } = 2;

        public string Description { get; set; } = "Joins a chat channel provided";

        public string Usage { get; set; } = "joinchat <#channel>";

        public void Execute(string[] args)
        {
            var didJoin = RattleClient.JoinChatChannel(Rattle.ChatChannels, args[1]);

            if (!didJoin)
            {
                Logger.Log($"Could not join channel: {args[1]} as you are already in it!", LogColors.GameError);
                return;
            }

            Logger.Log($"Attempting to join channel: {args[1]}...", LogColors.GameSuccess);
        }
    }
}
#endif