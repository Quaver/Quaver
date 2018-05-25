using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Enums;
using Quaver.Main;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Quaver.Helpers
{
    internal static class GraphicsHelper
    {
        /// <summary>
        /// Converts Point to Vector2
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        internal static Vector2 PointToVector2(Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        /// <summary>
        /// Converts Vector2 to Point
        /// </summary>
        /// <param name="vector2"></param>
        /// <returns></returns>
        internal static Point Vector2ToPoint(Vector2 vector2)
        {
            return new Point((int)vector2.X, (int)vector2.Y);
        }

        /// <summary>
        /// Returns a 1-dimensional value for an object's alignment within the provided boundary.
        /// </summary>
        /// <param name="scale">The value (percentage) which the object will be aligned to (0=min, 0.5 =mid, 1.0 = max)</param>
        /// <param name="objectSize">The size of the object</param>
        /// <param name="boundary"></param>
        /// <returns></returns>
        internal static float Align(float scale, float objectSize, float boundaryX, float boundaryY, float offset = 0)
        {
            //BoundaryMin + (BoundarySize - ObjectSize) * Scale + Offset
            return Math.Min(boundaryX, boundaryY) + ((Math.Abs(boundaryX - boundaryY) - objectSize) * scale) + offset;
        }

        /// <summary>
        /// Returns an aligned rectangle within a boundary.
        /// </summary>
        /// <param name="objectAlignment">The alignment of the object.</param>
        /// <param name="objectRect">The size of the object.</param>
        /// <param name="boundary">The Rectangle of the boundary.</param>
        /// <returns></returns>
        internal static DrawRectangle AlignRect(Alignment objectAlignment, DrawRectangle objectRect, DrawRectangle boundary)
        {
            float alignX = 0;
            float alignY = 0;

            // Set the X-Alignment Scale
            switch (objectAlignment)
            {
                case Alignment.BotCenter:
                case Alignment.MidCenter:
                case Alignment.TopCenter:
                    alignX = 0.5f;
                    break;
                case Alignment.BotRight:
                case Alignment.MidRight:
                case Alignment.TopRight:
                    alignX = 1f;
                    break;
                default:
                    break;
            }

            // Set the Y-Alignment Scale
            switch (objectAlignment)
            {
                case Alignment.MidLeft:
                case Alignment.MidCenter:
                case Alignment.MidRight:
                    alignY = 0.5f;
                    break;
                case Alignment.BotLeft:
                case Alignment.BotCenter:
                case Alignment.BotRight:
                    alignY = 1f;
                    break;
                default:
                    break;
            }

            //Set X and Y Alignments
            alignX = Align(alignX, objectRect.Width, boundary.X, boundary.X + boundary.Width, objectRect.X);
            alignY = Align(alignY, objectRect.Height, boundary.Y, boundary.Y + boundary.Height, objectRect.Y);

            return new DrawRectangle(alignX, alignY, objectRect.Width, objectRect.Height);
        }

        /// <summary>
        ///     Convert Drawable.DrawRectangle to Xna.Framework.Rectangle
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        internal static Rectangle DrawRectToRectangle(DrawRectangle rect)
        {
            return new Rectangle((int)Math.Ceiling(rect.X), (int)Math.Ceiling(rect.Y), (int)Math.Ceiling(rect.Width), (int)Math.Ceiling(rect.Height));
        }

        /// <summary>
        ///      Check if a Vector2 point is inside a DrawRectangle
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        internal static bool RectangleContains(DrawRectangle rect, Vector2 point)
        {
            return (point.X >= rect.X && point.X <= rect.X + rect.Width && point.Y >= rect.Y && point.Y <= rect.Y + rect.Height);
        }

        /// <summary>
        ///     Check if 2 DrawRectangles are Intercepting
        /// </summary>
        /// <param name="rect1"></param>
        /// <param name="rect2"></param>
        /// <returns></returns>
        internal static bool RectangleIntercepts(DrawRectangle rect1, DrawRectangle rect2)
        {
            return !(rect1.X + rect1.Width < rect2.X || rect1.X > rect2.X + rect2.Width || rect1.Y + rect1.Height < rect2.Y ||
                    rect1.Y > rect2.Y + rect2.Height);
        }

        /// <summary>
        /// This method is used for animation/tweening.
        /// </summary>
        /// <param name="target">The target value.</param>
        /// <param name="current">The current value.</param>
        /// <param name="scale">Make sure this value is between 0 and 1.</param>
        /// <returns></returns>
        internal static float Tween(float target, float current, double scale)
        {
            return (float)(current + ((target - current) * scale));
        }

        /// <summary>
        ///     Linear interpolation.
        /// </summary>
        /// <param name="firstFloat"></param>
        /// <param name="secondFloat"></param>
        /// <param name="by"></param>
        /// <returns></returns>
        internal static float Lerp(float firstFloat, float secondFloat, double by)
        {
            return (float)(firstFloat * by + secondFloat * (1 - by));
        }
        
        /// <summary>
        ///     Loads an image into a Texture2D
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static Texture2D LoadTexture2DFromFile(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                return Texture2D.FromStream(GameBase.GraphicsDevice, fileStream);
            }
        }

        /// <summary>
        ///     Returns a list of textures from a spritesheet texture.
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        internal static List<Texture2D> LoadSpritesheetFromTexture(Texture2D tex, int rows, int columns)
        {
            var frames = new List<Texture2D>();
            
            for (var i = 0; i < rows * columns; i++)
            {
                // Get the specific row and column from the index.
                var row = i / rows;
                var column = i % rows;
                
                // Get the x and y from the row and column
                var sourceRect = new Rectangle(tex.Width / rows * row, tex.Height / columns * column, tex.Width / rows, tex.Height / columns);
                var cropTexture = new Texture2D(GameBase.GraphicsDevice, sourceRect.Width, sourceRect.Height);
                var data = new Color[sourceRect.Width * sourceRect.Height];
                tex.GetData(0, sourceRect, data, 0, data.Length);
                cropTexture.SetData(data);
                
                frames.Add(cropTexture);
            }

            return frames;
        }
    }
}
