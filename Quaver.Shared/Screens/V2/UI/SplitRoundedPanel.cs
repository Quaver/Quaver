using System;
using Microsoft.Xna.Framework;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     A rounded panel with two colors: one left of <see cref="SplitPosition"/> and one right of it.
    /// </summary>
    internal sealed class SplitRoundedPanel : BakedRoundedPanel<(float Split, Color Left, Color Right)>
    {
        private Color LeftColor { get; }

        private Color RightColor { get; }

        private float _splitPosition;

        /// <summary>
        ///     Where the left color ends, measured from the left edge.
        /// </summary>
        internal float SplitPosition
        {
            get => _splitPosition;
            set
            {
                if (Math.Abs(_splitPosition - value) < 0.001f)
                    return;

                _splitPosition = value;
                RefreshTexture();
            }
        }

        internal SplitRoundedPanel(float cornerRadius, Color leftColor, Color rightColor) : base(cornerRadius)
        {
            LeftColor = leftColor;
            RightColor = rightColor;
        }

        protected override (float Split, Color Left, Color Right) Fill => (SplitInPixels, LeftColor, RightColor);

        protected override Color GetColor(float x, int textureWidth) => x < SplitInPixels ? LeftColor : RightColor;

        /// <summary>
        ///     The split in texture pixels. The texture width is rounded up, so it can differ a little from Width.
        /// </summary>
        private float SplitInPixels => SplitPosition * Math.Max(1, (int) Math.Ceiling(Width)) / Width;
    }
}
