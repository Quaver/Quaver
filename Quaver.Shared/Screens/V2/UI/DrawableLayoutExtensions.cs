using System;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     Size, position and alignment setters that do nothing when the value is already the same.
    ///     Wobble's own setters always recalculate the whole subtree, even for the same value.
    ///     That adds up in OnRectangleRecalculated, which runs every frame for everything inside a
    ///     list while it scrolls.
    /// </summary>
    internal static class DrawableLayoutExtensions
    {
        private const float Epsilon = 0.001f;

        internal static void SetSizeIfChanged(this Drawable drawable, float width, float height) =>
            drawable.SetSizeIfChanged(new ScalableVector2(width, height));

        internal static void SetSizeIfChanged(this Drawable drawable, ScalableVector2 size)
        {
            if (drawable == null || !drawable.SizeDiffers(size))
                return;

            drawable.Size = size;
        }

        internal static bool SizeDiffers(this Drawable drawable, ScalableVector2 size)
        {
            if (drawable == null)
                return false;

            var current = drawable.Size;
            return Math.Abs(current.X.Value - size.X.Value) >= Epsilon ||
                   Math.Abs(current.Y.Value - size.Y.Value) >= Epsilon ||
                   Math.Abs(current.X.Scale - size.X.Scale) >= Epsilon ||
                   Math.Abs(current.Y.Scale - size.Y.Scale) >= Epsilon;
        }

        internal static void SetAlignmentIfChanged(this Drawable drawable, Alignment alignment)
        {
            if (drawable == null || drawable.Alignment == alignment)
                return;

            drawable.Alignment = alignment;
        }

        internal static void SetPositionIfChanged(this Drawable drawable, float x, float y) =>
            drawable.SetPositionIfChanged(new ScalableVector2(x, y));

        internal static void SetPositionIfChanged(this Drawable drawable, ScalableVector2 position)
        {
            if (drawable == null)
                return;

            var current = drawable.Position;
            if (Math.Abs(current.X.Value - position.X.Value) < Epsilon &&
                Math.Abs(current.Y.Value - position.Y.Value) < Epsilon &&
                Math.Abs(current.X.Scale - position.X.Scale) < Epsilon &&
                Math.Abs(current.Y.Scale - position.Y.Scale) < Epsilon)
                return;

            drawable.Position = position;
        }
    }
}
