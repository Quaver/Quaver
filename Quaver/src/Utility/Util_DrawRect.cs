using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Quaver.Graphics;

namespace Quaver.Utility
{
    internal static partial class Util
    {
        /// <summary>
        /// Returns an aligned rectangle within a boundary.
        /// </summary>
        /// <param name="ObjectAlignment">The alignment of the object.</param>
        /// <param name="ObjectSize">The size of the object.</param>
        /// <param name="Boundary">The Rectangle of the Boundary.</param>
        /// <returns></returns>
        public static Rectangle DrawRect(Alignment ObjectAlignment, Vector2 ObjectSize, Rectangle Boundary, Vector2 Offset = new Vector2())
        {
            float AlignX = 0;
            float AlignY = 0;

            //Set X-Alignment Scale
            //To middle
            if (ObjectAlignment == Alignment.BotCenter || ObjectAlignment == Alignment.MidCenter || ObjectAlignment == Alignment.TopCenter) AlignX = 0.5f;
            //To right
            else if (ObjectAlignment == Alignment.BotRight || ObjectAlignment == Alignment.MidRight || ObjectAlignment == Alignment.TopRight) AlignX = 1f;

            //Set Y-Alignment Scale
            //To middle
            if (ObjectAlignment == Alignment.MidLeft || ObjectAlignment == Alignment.MidCenter || ObjectAlignment == Alignment.MidRight) AlignY = 0.5f;
            //To bottom
            else if (ObjectAlignment == Alignment.BotLeft || ObjectAlignment == Alignment.BotCenter || ObjectAlignment == Alignment.BotRight) AlignY = 1f;

            //Set X and Y Alignments
            AlignX = Align(AlignX, ObjectSize.X, new Vector2(Boundary.X, Boundary.X + Boundary.Width), Offset.X);
            AlignY = Align(AlignY, ObjectSize.Y, new Vector2(Boundary.Y, Boundary.Y + Boundary.Height), Offset.Y);

            return new Rectangle((int)AlignX, (int)AlignY, (int)ObjectSize.X, (int)ObjectSize.Y);
        }
    }
}
