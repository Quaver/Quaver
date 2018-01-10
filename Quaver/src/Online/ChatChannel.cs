using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if !PUBLIC
using Quaver.Framework.Events.Packets.Structures;

namespace Quaver.Online
{
    internal class ChatChannel
    {
        /// <summary>
        ///     The name of the chat channel
        /// </summary>
        internal string ChannelName { get; set; }
    }
}
#endif
