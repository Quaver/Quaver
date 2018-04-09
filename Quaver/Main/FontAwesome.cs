using Microsoft.Xna.Framework.Graphics;
using Quaver.Helpers;
using Quaver.Resources;

namespace Quaver.Main
{
    internal static class FontAwesome
    {
        internal static Texture2D Home { get; set; }

        /// <summary>
        ///     Loads all FontAwesome icon textures.
        /// </summary>
        internal static void Load()
        {
            Home = ResourceHelper.LoadTexture2DFromPng(QuaverResources.fa_home);
        }
    }
}