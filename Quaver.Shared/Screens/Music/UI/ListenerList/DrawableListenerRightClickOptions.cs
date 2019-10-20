using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Music.UI.ListenerList
{
    public class DrawableListenerRightClickOptions : RightClickOptions
    {
        private const string ViewProfile = "View Profile";

        private const string SteamProfile = "Steam Profile";

        private const string GiveHost = "Give Host";

        private const string Kick = "Kick";

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public DrawableListenerRightClickOptions(OnlineUser user, ListenerListScrollContainer container) : base(GetOptions(user), new ScalableVector2(200, 40), 22)
        {
            ItemSelected += (sender, args) =>
            {
                switch (args.Text)
                {
                    case ViewProfile:
                        BrowserHelper.OpenURL($"https://quavergame.com/profile/{user.Id}");
                        break;
                    case SteamProfile:
                        BrowserHelper.OpenURL($"http://steamcommunity.com/profiles/{user.SteamId}");
                        break;
                    case GiveHost:
                        OnlineManager.Client?.ChangeListeningPartyHost(user.Id);
                        break;
                    case Kick:
                        OnlineManager.Client?.KickListeningPartyUser(user.Id);
                        break;
                }
            };
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, Color> GetOptions(OnlineUser user)
        {
            var options = new Dictionary<string, Color>()
            {
                {ViewProfile, Color.White},
                {SteamProfile, Colors.MainAccent}
            };

            if (OnlineManager.IsListeningPartyHost && OnlineManager.Connected && user != OnlineManager.Self.OnlineUser)
            {
                options.Add(GiveHost, Color.LimeGreen);
                options.Add(Kick, ColorHelper.HexToColor($"#FF6868"));
            }

            return options;
        }
    }
}