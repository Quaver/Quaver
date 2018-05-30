namespace Quaver.Graphics
{
    /// <summary>
    ///     Similar to Xna.Framework.Rectangle, but uses float instead of int
    /// </summary>
    internal class DrawRectangle
    {
        /// <summary>
        ///     Position X
        /// </summary>
        internal float X { get; set; }

        /// <summary>
        ///     Position Y
        /// </summary>
        internal float Y { get; set; }

        /// <summary>
        ///     Width Size
        /// </summary>
        internal float Width { get; set; }

        /// <summary>
        ///     Height Size
        /// </summary>
        internal float Height { get; set; }

        /// <summary>
        ///     Create a Rectangle for drawable classes
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        internal DrawRectangle(float x = 0, float y = 0, float width = 0, float height = 0)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
