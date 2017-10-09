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
        /// <param name="ObjectRect">The size of the object.</param>
        /// <param name="Boundary">The Rectangle of the Boundary.</param>
        /// <returns></returns>
        public static Rectangle DrawRect(Alignment ObjectAlignment, Rectangle ObjectRect, Rectangle Boundary)
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
            AlignX = Align(AlignX, ObjectRect.Width, new Vector2(Boundary.X, Boundary.X + Boundary.Width), ObjectRect.X);
            AlignY = Align(AlignY, ObjectRect.Height, new Vector2(Boundary.Y, Boundary.Y + Boundary.Height), ObjectRect.Y);

            return new Rectangle((int)AlignX, (int)AlignY, (int)ObjectRect.Width, (int)ObjectRect.Height);
        }
    }
}
