using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Main;
using Quaver.Resources;

namespace Quaver.Helpers
{
    internal static class ResourceHelper
    {
        /// <summary>
        ///     Loads a PNG into a Texture2D from resources.
        /// </summary>
        /// <returns></returns>
        internal static Texture2D LoadTexture2DFromPng(Bitmap image)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream, ImageFormat.Png);
                return Texture2D.FromStream(GameBase.GraphicsDevice, stream);
            }
        }

        /// <summary>
        ///     Loads a JPEG into a Texture2D from resources.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        internal static Texture2D LoadTexture2DFromJpeg(Bitmap image)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream, ImageFormat.Jpeg);
                return Texture2D.FromStream(GameBase.GraphicsDevice, stream);
            }
        }

        /// <summary>
        ///     Gets a Bitmap object from the resouurces given their file name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static object GetProperty(string name)
        {
            return typeof(QuaverResources).GetProperty(name.Replace("-", "_").Replace("@", "_"))?.GetValue(null, null);
        }
    }
}
