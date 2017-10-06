using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace Quaver.Utility
{

    internal enum Alignment
    {
        TopLeft,
        TopCenter,
        TopRight,
        MidLeft,
        MidCenter,
        MidRight,
        BotLeft,
        BotCenter,
        BotRight
    }

    internal static partial class Util
    {
        /// <summary>
        /// Returns a 1-dimensional value for an object's alignment within the provided boundary.
        /// </summary>
        /// <param name="ObjectAlignment">The alignment of the object (Enum)</param>
        /// <param name="ObjectSize">The size of the object (Vector2)</param>
        /// <param name="Rect">The Rect Transform of the object (Vector4)</param>
        /// <returns></returns>
        public static Rectangle DrawRect(Alignment ObjectAlignment, Vector2 ObjectSize, Rectangle Rect, Vector2 Offset = new Vector2())
        {
            float AlignX = 0;
            float AlignY = 0;

            //X allign to middle
            if (ObjectAlignment == Alignment.BotCenter || ObjectAlignment == Alignment.MidCenter || ObjectAlignment == Alignment.TopCenter) AlignX = 0.5f;
            else if (ObjectAlignment == Alignment.BotRight || ObjectAlignment == Alignment.MidRight || ObjectAlignment == Alignment.TopRight) AlignX = 1f;

            if (ObjectAlignment == Alignment.MidLeft || ObjectAlignment == Alignment.MidCenter || ObjectAlignment == Alignment.MidRight) AlignY = 0.5f;
            else if (ObjectAlignment == Alignment.BotLeft || ObjectAlignment == Alignment.BotCenter || ObjectAlignment == Alignment.BotRight) AlignY = 1f;

            AlignX = Align(AlignX, ObjectSize.X, new Vector2(Rect.X, Rect.Width), Offset.X);
            AlignY = Align(AlignY, ObjectSize.Y, new Vector2(Rect.Y, Rect.Height), Offset.Y);

            return new Rectangle((int)AlignX, (int)AlignY, (int)ObjectSize.X, (int)ObjectSize.Y);
        }
    }
}
