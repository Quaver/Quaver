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
    internal class LeaveChatChannel : ICommand
    {
        public string Name { get; set; } = "LEAVECHAT";

        public int Args { get; set; } = 2;

        public string Description { get; set; } = "Leaves a chat channel provided";

        public string Usage { get; set; } = "leavechat <#channel>";

        public void Execute(string[] args)
        {
            var didLeave = RattleClient.LeaveChatChannel(Rattle.ChatChannels, args[1]);

            if (!didLeave)
            {
                Logger.Log($"Could not leave channel: {args[1]} as you are not in it!", LogColors.GameError);
                return;
            }

            Logger.Log($"Successfully left chat channel: {args[1]}", LogColors.GameSuccess);
        }
    }
}
#endif