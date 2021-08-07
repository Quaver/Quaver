using System;
using Quaver.Shared.Online;
using Steamworks;

namespace Quaver.Shared.Graphics.Dialogs.Online
{
    public class CreatingAccountDialog : LoadingDialog
    {
        public CreatingAccountDialog(string username) : base("CREATING ACCOUNT", "Your account is being created. Please wait!",
            () =>
            {
                OnlineManager.Client.ChooseUsername(username, SteamUser.GetSteamID().m_SteamID, SteamFriends.GetPersonaName());
            })
        {
        }
    }
}