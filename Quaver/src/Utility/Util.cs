using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Graphics;

namespace Quaver.Utility
{
    internal class Util
    {
        /// <summary>
        /// Converts score to string (1234567) format
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        internal static string ScoreToString(int score)
        {
            return score.ToString("0000000");
        }

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

        /*
        /// <summary>
        /// Returns an aligned rectangle within a boundary.
        /// </summary>
        /// <param name="objectAlignment">The alignment of the object.</param>
        /// <param name="objectRect">The size of the object.</param>
        /// <param name="boundary">The Rectangle of the boundary.</param>
        /// <returns></returns>
        internal static Rectangle DrawRect(Alignment objectAlignment, Rectangle objectRect, Rectangle boundary)
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
            alignX = Align(alignX, objectRect.Width, new Vector2(boundary.X, boundary.X + boundary.Width), objectRect.X);
            alignY = Align(alignY, objectRect.Height, new Vector2(boundary.Y, boundary.Y + boundary.Height), objectRect.Y);

            return new Rectangle((int)alignX, (int)alignY, objectRect.Width, objectRect.Height);
        }*/

        /// <summary>
        /// Returns an aligned rectangle within a boundary.
        /// </summary>
        /// <param name="objectAlignment">The alignment of the object.</param>
        /// <param name="objectRect">The size of the object.</param>
        /// <param name="boundary">The Rectangle of the boundary.</param>
        /// <returns></returns>
        internal static Vector4 DrawRect(Alignment objectAlignment, Vector4 objectRect, Vector4 boundary)
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
            alignX = Align(alignX, objectRect.W, boundary.X, boundary.X + boundary.W, objectRect.X);
            alignY = Align(alignY, objectRect.Z, boundary.Y, boundary.Y + boundary.Z, objectRect.Y);

            return new Vector4(alignX, alignY, objectRect.Z, objectRect.W);
        }

        /*
        /// <summary>
        /// Returns an aligned rectangle within a boundary.
        /// </summary>
        /// <param name="objectAlignment">The alignment of the object.</param>
        /// <param name="objectRect">The size of the object.</param>
        /// <param name="boundary">The Rectangle of the boundary.</param>
        /// <returns></returns>
        internal static Vector4 DrawRect(Alignment objectAlignment, Vector4 objectRect, Rectangle boundary)
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
            alignX = Align(alignX, objectRect.W, new Vector2(boundary.X, boundary.X + boundary.Width), objectRect.X);
            alignY = Align(alignY, objectRect.Z, new Vector2(boundary.Y, boundary.Y + boundary.Height), objectRect.Y);

            return new Vector4(alignX, alignY, objectRect.Z, objectRect.W);
        }*/

            /*
        /// <summary>
        ///     Convert a Rectangle to Vector4 value
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        internal static Vector4 RectangleToVector4(Rectangle rect)
        {
            return new Vector4(rect.X, rect.Y, rect.Height, rect.Width);
        }*/

        /// <summary>
        ///     Convert a Vector4 to Rectangle value
        /// </summary>
        /// <param name="vect"></param>
        /// <returns></returns>
        internal static Rectangle Vector4ToRectangle(Vector4 vect)
        {
            return new Rectangle((int)Math.Ceiling(vect.X), (int)Math.Ceiling(vect.Y), (int)Math.Ceiling(vect.W), (int)Math.Ceiling(vect.Z));
        }

        /// <summary>
        ///     Check if a Vector2 point is inside a Vector4 "Rect"
        /// </summary>
        /// <param name="vect"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        internal static bool Vector4Contains(Vector4 vect, Vector2 point)
        {
            return (point.X >= vect.X && point.X <= vect.X + vect.W && point.Y >= vect.Y && point.Y <= vect.Y + vect.Z);
        }

        /*
        /// <summary>
        ///     Check if a Vector4 intercepts with another Vector4
        /// </summary>
        /// <param name="vect1"></param>
        /// <param name="vect2"></param>
        /// <returns></returns>
        internal static bool Vector4BorderIntercepts(Vector4 vect1, Vector4 vect2)
        {
            if ((vect2.X > vect1.X && vect2.X < vect1.X + vect1.W) ||
                (vect2.Y > vect1.Y && vect2.Y < vect1.Y + vect1.Z) ||
                (vect2.X + vect2.W > vect1.X && vect2.X + vect2.W < vect1.X + vect1.W) ||
                (vect2.Y + vect2.Z > vect1.Y && vect2.Y + vect2.Z < vect1.Y + vect1.Z)) return true;
            else return false;
            return true;
        }*/

        /// <summary>
        ///     Check if a Vector4 intercepts with another Vector4
        /// </summary>
        /// <param name="vect1"></param>
        /// <param name="vect2"></param>
        /// <returns></returns>
        internal static bool Vector4Intercepts(Vector4 vect1, Vector4 vect2)
        {
            if (!(vect1.X + vect1.W < vect2.X ||
                vect1.X > vect2.X + vect2.W ||
                vect1.Y + vect1.Z < vect2.Y ||
                vect1.Y > vect2.Y + vect2.Z)) return true;
            else return false;
        }

        /// <summary>
        /// Generates A random float between 2 numbers.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        internal static float Random(float min, float max)
        {
            var  random = new Random();

            // If min > max for some reason
            if (min > max)
            {
                var temp = min;
                max = min;
                min = temp;
            }

            //Generate the random number
            var randNum = random.Next(0, 1000) / 1000f;

            //Return the random number in the given range
            return (randNum * (max - min)) + min;
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
        ///     Turns a Stream object into a byte array
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] ConvertStreamToByteArray(Stream input)
        {
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        ///     Makes a string safe to be written as a file name.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal static string FileNameSafeString(string str)
        {
            var invalidPathChars = Path.GetInvalidFileNameChars();

            foreach (var invalidChar in invalidPathChars)
                str = str.Replace(invalidChar.ToString(), "");

            return str;
        }
    }
}
