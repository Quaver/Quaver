using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     A single-color rounded panel. Its texture comes from the shared rounded texture cache.
    /// </summary>
    internal sealed class RoundedPanel : Sprite
    {
        private float CornerRadius { get; }

        private float ResolvedWidth { get; set; } = -1;

        private float ResolvedHeight { get; set; } = -1;

        internal RoundedPanel(float cornerRadius) => CornerRadius = cornerRadius;

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();

            if (Width <= 0 || Height <= 0)
                return;

            if (Width == ResolvedWidth && Height == ResolvedHeight && Image != null && !Image.IsDisposed)
                return;

            var texture = RoundedRectTextureCache.Get(Width, Height, CornerRadius);
            ResolvedWidth = Width;
            ResolvedHeight = Height;

            if (Image != texture)
                Image = texture;
        }
    }
}
