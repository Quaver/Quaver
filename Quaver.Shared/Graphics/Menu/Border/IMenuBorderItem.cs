namespace Quaver.Shared.Graphics.Menu.Border
{
    public interface IMenuBorderItem
    {
        /// <summary>
        ///     If true, <see cref="MenuBorder"/> will use <see cref="CustomPaddingY"/> when aligning the y
        ///     position
        /// </summary>
        bool UseCustomPaddingY { get; }

        /// <summary>
        ///     The custom y padding to be used in <see cref="MenuBorder"/>
        /// </summary>
        int CustomPaddingY { get; }

        /// <summary>
        ///     If true, <see cref="MenuBorder"/> will use <see cref="CustomPaddingX"/> when aligning the x
        ///     position
        /// </summary>
        bool UseCustomPaddingX { get; }

        /// <summary>
        ///     The custom x padding to be used in <see cref="MenuBorder"/>
        /// </summary>
        int CustomPaddingX { get; }
    }
}