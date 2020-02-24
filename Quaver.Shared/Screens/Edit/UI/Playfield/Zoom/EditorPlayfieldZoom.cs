using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Zoom
{
    public class EditorPlayfieldZoom : Container
    {
        /// <summary>
        /// </summary>
        private ScalableVector2 ButtonSize { get; } = new ScalableVector2(40, 40);

        /// <summary>
        /// </summary>
        private IconButton ZoomIn { get; }

        /// <summary>
        /// </summary>
        private IconButton ZoomOut { get; }

        /// <summary>
        /// </summary>
        public EditorPlayfieldZoom(BindableInt scrollSpeed)
        {
            var tooltipColor = ColorHelper.HexToColor("#808080");

            var game = GameBase.Game as QuaverGame;

            ZoomIn = new IconButton(UserInterface.EditorZoomIn, (sender, args) => scrollSpeed.Value++)
            {
                Parent = this,
                Size = ButtonSize
            };

            ZoomIn.Hovered += (sender, args) => game?.CurrentScreen?.ActivateTooltip(new Tooltip($"Zoom In (Page Up)", tooltipColor));
            ZoomIn.LeftHover += (sender, args) => game?.CurrentScreen?.DeactivateTooltip();

            ZoomOut = new IconButton(UserInterface.EditorZoomOut, (sender, args) => scrollSpeed.Value--)
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Size = ButtonSize
            };

            ZoomOut.Hovered += (sender, args) => game?.CurrentScreen?.ActivateTooltip(new Tooltip($"Zoom Out (Page Down)", tooltipColor));
            ZoomOut.LeftHover += (sender, args) => game?.CurrentScreen?.DeactivateTooltip();

            Size = new ScalableVector2(ButtonSize.X.Value, ButtonSize.Y.Value * 2 + 4);
        }
    }
}