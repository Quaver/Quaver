using System;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Resources;
using Wobble.Assets;
using Wobble.Logging;

namespace Quaver.Helpers
{
    public class ResourceHelper
    {
        /// <summary>
        ///     Loads a skin's texture from resources.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        internal static Texture2D LoadTextureByName(string element, ImageFormat format)
        {
            try
            {
                return AssetLoader.LoadTexture2D((Bitmap)GetProperty(element), format);
            }
            catch (Exception e)
            {
                Logger.Error($"Element: {element} was not found in QuaverResources", LogType.Runtime);
                return UserInterface.BlankBox;
            }
        }

        /// <summary>
        ///     Gets an object from the resouurces given their file name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static object GetProperty(string name)
        {
            return typeof(QuaverResources).GetProperty(name.Replace("-", "_").Replace("@", "_"))?.GetValue(null, null);
        }
    }
}