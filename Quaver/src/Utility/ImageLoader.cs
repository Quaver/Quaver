using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Main;

namespace Quaver.Utility
{
    internal class ImageLoader
    {
        /// <summary>
        ///     Loads an image into a Texture2D
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static Texture2D Load(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                return Texture2D.FromStream(GameBase.GraphicsDevice, fileStream);
            }
        }
    }
}
