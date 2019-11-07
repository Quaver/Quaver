using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Quaver.Shared.Config;
using Quaver.Shared.Online;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Graphics.Overlays.Hub.SongRequests.Header
{
    public class OnlineHubSongRequestsHeader : Sprite
    {
        /// <summary>
        /// </summary>
        private ConnectTwitchButton ConnectTwitch { get; set; }

        /// <summary>
        /// </summary>
        private DisplayAlertsCheckbox DisplayAlerts { get; set; }

        /// <summary>
        /// </summary>
        private const int Padding = 16;

        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public OnlineHubSongRequestsHeader(ScalableVector2 size)
        {
            Size = size;

            CreateConnectTwitchButton();
            CreateDisplayAlerts();
        }

        /// <summary>
        /// </summary>
        private void CreateConnectTwitchButton()
        {
            ConnectTwitch = new ConnectTwitchButton()
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Padding
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDisplayAlerts()
        {
            DisplayAlerts = new DisplayAlertsCheckbox()
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Padding
            };
        }
    }
}