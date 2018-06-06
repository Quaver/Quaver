using Quaver.Main;

namespace Quaver.Graphics.Base
{
    /// <inheritdoc />
    /// <summary>
    ///     This is used for sprite/QuaverUserInterface layout
    /// </summary>
    internal class Container : Drawable
    {
        /// <summary>
        ///     Create a new QuaverContainer Given Xposition, Yposition, Xsize, Ysize
        /// </summary>
        /// <param name="xPosition"></param>
        /// <param name="yPosition"></param>
        /// <param name="xSize"></param>
        /// <param name="ySize"></param>
        public Container(float xPosition = 0, float yPosition = 0, float? xSize = null, float? ySize = null)
        {
            Size.X.Offset = xSize ?? GameBase.WindowRectangle.Width;
            Size.Y.Offset = ySize ?? GameBase.WindowRectangle.Height;
            Position.X.Offset = xPosition;
            Position.Y.Offset = yPosition;
        }
    }
}
