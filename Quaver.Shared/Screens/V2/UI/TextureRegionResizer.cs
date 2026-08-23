using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     Creates a high-quality downsampled texture from a region of a larger texture.
    /// </summary>
    internal static class TextureRegionResizer
    {
        public static Texture2D Create(Texture2D source, Rectangle sourceRectangle, int width, int height)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (sourceRectangle.Width <= 0 || sourceRectangle.Height <= 0 ||
                sourceRectangle.X < 0 || sourceRectangle.Y < 0 ||
                sourceRectangle.Right > source.Width || sourceRectangle.Bottom > source.Height)
                throw new ArgumentOutOfRangeException(nameof(sourceRectangle));
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            var sourcePixels = new Color[sourceRectangle.Width * sourceRectangle.Height];
            source.GetData(0, sourceRectangle, sourcePixels, 0, sourcePixels.Length);

            var outputPixels = new Color[width * height];
            var scaleX = (double) sourceRectangle.Width / width;
            var scaleY = (double) sourceRectangle.Height / height;

            for (var outputY = 0; outputY < height; outputY++)
            {
                var sourceTop = outputY * scaleY;
                var sourceBottom = (outputY + 1) * scaleY;
                var firstSourceY = (int) Math.Floor(sourceTop);
                var lastSourceY = Math.Min(sourceRectangle.Height, (int) Math.Ceiling(sourceBottom));

                for (var outputX = 0; outputX < width; outputX++)
                {
                    var sourceLeft = outputX * scaleX;
                    var sourceRight = (outputX + 1) * scaleX;
                    var firstSourceX = (int) Math.Floor(sourceLeft);
                    var lastSourceX = Math.Min(sourceRectangle.Width, (int) Math.Ceiling(sourceRight));
                    double red = 0;
                    double green = 0;
                    double blue = 0;
                    double alpha = 0;
                    double totalWeight = 0;

                    for (var sourceY = firstSourceY; sourceY < lastSourceY; sourceY++)
                    {
                        var verticalWeight = Math.Min(sourceBottom, sourceY + 1) -
                                             Math.Max(sourceTop, sourceY);
                        if (verticalWeight <= 0)
                            continue;

                        for (var sourceX = firstSourceX; sourceX < lastSourceX; sourceX++)
                        {
                            var horizontalWeight = Math.Min(sourceRight, sourceX + 1) -
                                                   Math.Max(sourceLeft, sourceX);
                            var weight = horizontalWeight * verticalWeight;
                            if (weight <= 0)
                                continue;

                            var pixel = sourcePixels[sourceY * sourceRectangle.Width + sourceX];
                            red += pixel.R * weight;
                            green += pixel.G * weight;
                            blue += pixel.B * weight;
                            alpha += pixel.A * weight;
                            totalWeight += weight;
                        }
                    }

                    outputPixels[outputY * width + outputX] = new Color(
                        (byte) Math.Round(red / totalWeight),
                        (byte) Math.Round(green / totalWeight),
                        (byte) Math.Round(blue / totalWeight),
                        (byte) Math.Round(alpha / totalWeight));
                }
            }

            var output = new Texture2D(GameBase.Game.GraphicsDevice, width, height);
            output.SetData(outputPixels);
            return output;
        }
    }
}
