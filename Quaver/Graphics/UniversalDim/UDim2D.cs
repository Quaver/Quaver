namespace Quaver.Graphics.UniversalDim
{
    /// <summary>
    ///     2 Dimensional Udim value used for QuaverUserInterface
    /// </summary>
    internal class UDim2D
    {
        /// <summary>
        ///     X UDim value
        /// </summary>
        internal UDim X { get; set; }

        /// <summary>
        ///     Y UDim Value
        /// </summary>
        internal UDim Y { get; set; }

        /// <summary>
        ///     Create a new UDim2D value with given Xoffset, yOffset, Xscale, Yscale
        /// </summary>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="xScale"></param>
        /// <param name="yScale"></param>
        internal UDim2D(float xOffset = 0, float yOffset = 0, float xScale = 0, float yScale = 0)
        {
            X = new UDim(xOffset, xScale);
            Y = new UDim(yOffset, yScale);
        }
    }
}
