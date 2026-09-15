using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics.Sprites;
using Wobble.Logging;

namespace Quaver.Shared.Screens.V2.Options.UI
{
    /// <summary>
    ///     The icons in the options icon atlas, from top to bottom.
    /// </summary>
    internal enum OptionsIconFrame
    {
        Search,
        Video,
        Audio,
        Gameplay,
        Skin,
        Input,
        Miscellaneous,
        Advanced,
        Collapse,
        Expand,
        RecentlyChanged,
        Reset
    }

    /// <summary>
    ///     The options icon atlas: one column of 60px icons, placed 84px apart.
    /// </summary>
    internal static class OptionsIconAtlas
    {
        private const int FrameSize = 60;

        private const int FrameStride = 84;

        private const int MinimumWidth = FrameSize;

        private const int MinimumHeight = FrameStride * (int) OptionsIconFrame.Reset + FrameSize;

        /// <summary>
        ///     Loads the skin's atlas or the built-in one when the skin has none or its atlas is too small.
        /// </summary>
        internal static Texture2D Load(SkinStoreV2Lease skin, string configuredPath)
        {
            var fallback = UserInterface.OptionsV2Icons;

            if (!IsValid(fallback))
                throw new InvalidOperationException(
                    $"The bundled Options V2 icon atlas must be at least {MinimumWidth}x{MinimumHeight} pixels.");

            var texture = skin.LoadTexture(configuredPath, fallback);

            if (IsValid(texture))
                return texture;

            Logger.Warning($"The configured Options V2 icon atlas must be at least {MinimumWidth}x{MinimumHeight} " +
                           "pixels; the bundled atlas will be used instead.", LogType.Runtime, false);
            return fallback;
        }

        internal static TextureRegion GetRegion(Texture2D texture, OptionsIconFrame frame)
        {
            if (!IsValid(texture))
                throw new ArgumentException("The Options V2 icon atlas has invalid dimensions.", nameof(texture));

            return new TextureRegion(texture, new Rectangle(0, (int) frame * FrameStride, FrameSize, FrameSize));
        }

        private static bool IsValid(Texture2D texture) =>
            texture != null && !texture.IsDisposed && texture.Width >= MinimumWidth && texture.Height >= MinimumHeight;
    }
}
