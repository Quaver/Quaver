using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     A rounded panel that draws its own texture. Used for fills with more than one color, which the
    ///     shared rounded texture cache cannot make. The texture is only rebuilt when the size or the fill
    ///     changes, because this runs every frame while a list scrolls.
    /// </summary>
    /// <typeparam name="TFill">The values that decide the fill.</typeparam>
    internal abstract class BakedRoundedPanel<TFill> : Sprite where TFill : struct, IEquatable<TFill>
    {
        private float CornerRadius { get; }

        private Texture2D OwnedTexture { get; set; }

        /// <summary>
        ///     True while the texture is being replaced, so the rebuild does not start again.
        /// </summary>
        private bool RefreshingTexture { get; set; }

        /// <summary>
        ///     The texture size and fill of the current texture.
        /// </summary>
        private (int Width, int Height, TFill Fill) BuiltWith { get; set; }

        protected BakedRoundedPanel(float cornerRadius)
        {
            CornerRadius = cornerRadius;
            Tint = Color.White;
        }

        /// <summary>
        ///     The values that decide the fill. The texture is rebuilt when these change.
        /// </summary>
        protected abstract TFill Fill { get; }

        /// <summary>
        ///     The fill color at a horizontal position, before the corners are rounded.
        /// </summary>
        /// <param name="x">The pixel's center, in pixels from the left edge.</param>
        /// <param name="textureWidth">The texture width in pixels.</param>
        protected abstract Color GetColor(float x, int textureWidth);

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();
            RefreshTexture();
        }

        public override void Destroy()
        {
            RefreshingTexture = true;

            try
            {
                Image = null;
                OwnedTexture?.Dispose();
                OwnedTexture = null;
                BuiltWith = default;
            }
            finally
            {
                RefreshingTexture = false;
            }

            base.Destroy();
        }

        /// <summary>
        ///     Rebuilds the texture, unless the current one already has this size and fill.
        /// </summary>
        protected void RefreshTexture()
        {
            if (RefreshingTexture || Width <= 0 || Height <= 0)
                return;

            RefreshingTexture = true;

            try
            {
                var textureWidth = Math.Max(1, (int) Math.Ceiling(Width));
                var textureHeight = Math.Max(1, (int) Math.Ceiling(Height));
                var buildInputs = (textureWidth, textureHeight, Fill);

                if (OwnedTexture != null && !OwnedTexture.IsDisposed && buildInputs.Equals(BuiltWith))
                    return;

                var radius = MathHelper.Clamp(CornerRadius, 0, Math.Min(textureWidth, textureHeight) / 2f);
                var halfWidth = textureWidth / 2f;
                var halfHeight = textureHeight / 2f;
                var pixels = new Color[textureWidth * textureHeight];

                for (var y = 0; y < textureHeight; y++)
                {
                    for (var x = 0; x < textureWidth; x++)
                    {
                        // Distance from the rounded edge, used to smooth the corners.
                        var qx = Math.Abs(x + 0.5f - halfWidth) - (halfWidth - radius);
                        var qy = Math.Abs(y + 0.5f - halfHeight) - (halfHeight - radius);
                        var outsideDistance = (float) Math.Sqrt(Math.Max(qx, 0) * Math.Max(qx, 0) +
                                                                Math.Max(qy, 0) * Math.Max(qy, 0));
                        var distance = outsideDistance + Math.Min(Math.Max(qx, qy), 0) - radius;
                        var coverage = 1 - SmoothStep(-1, 0, distance);

                        var source = GetColor(x + 0.5f, textureWidth);
                        pixels[y * textureWidth + x] = new Color(source.R, source.G, source.B,
                            (byte) Math.Round(source.A * coverage));
                    }
                }

                var texture = new Texture2D(GameBase.Game.GraphicsDevice, textureWidth, textureHeight, false,
                    SurfaceFormat.Color);
                texture.SetData(pixels);

                var previous = OwnedTexture;
                OwnedTexture = texture;
                BuiltWith = buildInputs;
                Image = texture;
                previous?.Dispose();
            }
            finally
            {
                RefreshingTexture = false;
            }
        }

        private static float SmoothStep(float min, float max, float value)
        {
            var amount = MathHelper.Clamp((value - min) / (max - min), 0, 1);
            return amount * amount * (3 - 2 * amount);
        }
    }
}
