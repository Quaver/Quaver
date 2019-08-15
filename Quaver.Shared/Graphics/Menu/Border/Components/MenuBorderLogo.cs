using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Graphics.Menu.Border.Components
{
    /// <summary>
    ///     Logo that goes in the left side of a menu header.
    /// </summary>
    public class MenuBorderLogo : ImageButton, IMenuBorderItem
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UseCustomPaddingY { get; } = true;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public int CustomPaddingY { get; } = -2;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UseCustomPaddingX { get; } = true;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public int CustomPaddingX { get; } = 0;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public MenuBorderLogo() : base(UserInterface.Logo) => Size = new ScalableVector2(126, 28);
    }
}