using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Resources;
using Wobble.Assets;

namespace Quaver.Assets
{
    public static class Titles
    {
        public static Texture2D Default { get; private set; }

        /// <summary>
        ///     Loads all the titles for the game.
        /// </summary>
        public static void Load() => Default = AssetLoader.LoadTexture2D(QuaverResources.title_default);
    }
}
