using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Net;
using Quaver.Net.Handlers;
using Quaver.Online.Events;

namespace Quaver.Online
{
    public static class FlamingoHelper
    {
        /// <summary>
        ///     Initializes the Flamingo event handlers so that we can execute
        ///     methods in Quaver when they are received from Flamingo.
        /// </summary>
        public static void InitializeFlamingoEventHandlers()
        {
            LoginReplyHandler.LoginReplyEvent += Login.OnLoginReplyEvent;
            UserConnectedHandler.UserConnectedEvent += UserConnected.OnUserConnectedEvent;
            UserDisconnectedHandler.UserDisconnectedEvent += UserDisconnected.OnUserDisconnectedEvent;
            JoinedChatChannelHandler.JoinedChatChannelEvent += JoinedChatChannel.OnChatChannelJoined;
            LeftChatChannelHandler.LeftChatChannelEvent += LeftChatChannel.OnChatChannelLeft;
            ChatMessageHandler.ChatMessageReceivedEvent += ChatMessageReceived.OnChatMessageReceived;
            FlamingoRequests.ResponseUserDataEvent += UserData.OnResponseUserDataHandler;
            AlertHandler.ServerAlertEvent += Alert.OnServerAlertReceived;
            ChickenHandler.ChickenEvent += Chicken.OnChicken;
            FlamingoRequests.ScoreSubmissionResponseEvent += ScoreSubmitted.OnScoreSubmission;
            ;
        }
    }
}