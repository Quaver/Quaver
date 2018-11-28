using System;
using Microsoft.Xna.Framework.Graphics;

namespace Quaver.Shared.Online
{
    public class SteamAvatarLoadedEventArgs : EventArgs
    {
        public ulong SteamId { get; }
        public Texture2D Texture { get; }

        public SteamAvatarLoadedEventArgs(ulong steamId, Texture2D tex)
        {
            SteamId = steamId;
            Texture = tex;
        }
    }
}