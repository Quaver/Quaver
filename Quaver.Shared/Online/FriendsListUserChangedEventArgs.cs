using System;
using Quaver.Server.Client.Enums;

namespace Quaver.Shared.Online
{
    public class FriendsListUserChangedEventArgs : EventArgs
    {
        public FriendsListAction Action { get; }

        public int UserId { get; }

        public FriendsListUserChangedEventArgs(FriendsListAction action, int userId)
        {
            Action = action;
            UserId = userId;
        }
    }
}