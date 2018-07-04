using Microsoft.Xna.Framework.Graphics;
using Quaver.Helpers;
using Quaver.Resources;

namespace Quaver.Main
{
    public static class Titles
    {
        internal static Texture2D OfflinePlayer { get; private set; }

        internal static void Load()
        {
            OfflinePlayer = ResourceHelper.LoadTexture2DFromPng(QuaverResources.title_offline_player);
        }
    }
}