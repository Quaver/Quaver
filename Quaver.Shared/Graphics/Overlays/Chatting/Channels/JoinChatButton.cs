using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Logging;

namespace Quaver.Shared.Graphics.Overlays.Chatting.Channels
{
    public class JoinChatButton : IconButton
    {
        public JoinChatButton() : base(FontAwesome.Get(FontAwesomeIcon.fa_plus_black_symbol))
        {
            Tint = Color.LimeGreen;

            Clicked += (sender, args) =>
            {
                Logger.Important($"User clicked to join a new channel", LogType.Runtime);
            };
        }
    }
}