using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.V2.Selection.UI.List
{
    /// <summary>
    ///     Owns the shared sources used to compose every rounded mapset row surface.
    /// </summary>
    internal sealed class MapsetRowSurfaceCache : IDisposable
    {
        private RoundedTextureCache Cache { get; }
        private Texture2D Background { get; }
        private Texture2D NormalGradient { get; }
        private Texture2D HoveredGradient { get; }
        private Texture2D SelectedGradient { get; }

        internal MapsetRowSurfaceCache(SkinV2SelectionRowConfig config)
        {
            Cache = new RoundedTextureCache(config.Width, config.Height, config.CornerRadius, true);
            Background = CreateSolidTexture(SkinV2Color.Parse(config.BackgroundColor));
            NormalGradient = CreateGradient(config.Width, SkinV2Color.Parse(config.BackgroundColor));
            HoveredGradient = CreateGradient(config.Width, SkinV2Color.Parse(config.HoveredBackgroundColor));
            SelectedGradient = CreateGradient(config.Width, SkinV2Color.Parse(config.SelectedOverlayColor));
        }

        internal SurfaceKey GetKey(Texture2D banner, bool hovered, bool selected) => new(
            banner == null || banner.IsDisposed ? Background : banner,
            selected ? SelectedGradient : hovered ? HoveredGradient : NormalGradient);

        internal void Bind(Sprite sprite, SurfaceKey key) => Cache.Bind(sprite, key.Source, key.Overlay);

        internal void Unbind(Sprite sprite) => Cache.Unbind(sprite);

        public void Dispose()
        {
            Cache.Dispose();
            Background.Dispose();
            NormalGradient.Dispose();
            HoveredGradient.Dispose();
            SelectedGradient.Dispose();
        }

        /// <summary>
        ///     Creates the small horizontal fade that is composited before the rounded mask
        /// </summary>
        private static Texture2D CreateGradient(int configuredWidth, Color color)
        {
            var width = Math.Max(1, configuredWidth);
            var pixels = new Color[width];

            for (var x = 0; x < width; x++)
            {
                var opacity = width == 1 ? 1 : 1 - x / (width - 1f);
                pixels[x] = Color.FromNonPremultiplied(color.R, color.G, color.B,
                    (byte)(color.A * opacity));
            }

            var texture = new Texture2D(GameBase.Game.GraphicsDevice, width, 1, false, SurfaceFormat.Color);
            texture.SetData(pixels);
            return texture;
        }

        private static Texture2D CreateSolidTexture(Color color)
        {
            var texture = new Texture2D(GameBase.Game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            texture.SetData(new[] { color });
            return texture;
        }

        internal readonly record struct SurfaceKey(Texture2D Source, Texture2D Overlay);
    }

    /// <summary>
    ///     Swaps between two cached sprites so a mapset keeps its previous surface until the next one is ready
    /// </summary>
    internal sealed class MapsetRowSurface : IDisposable
    {
        private MapsetRowSurfaceCache Cache { get; }
        private Sprite[] Sprites { get; }
        private int CurrentIndex { get; set; } = -1;
        private int PendingIndex { get; set; } = -1;
        private MapsetRowSurfaceCache.SurfaceKey? CurrentKey { get; set; }
        private MapsetRowSurfaceCache.SurfaceKey? PendingKey { get; set; }

        internal bool Visible => CurrentIndex >= 0 && Sprites[CurrentIndex].Visible ||
                                 PendingIndex >= 0 && Sprites[PendingIndex].Visible;

        internal ScalableVector2 Size
        {
            set
            {
                foreach (var sprite in Sprites)
                    sprite.Size = value;
            }
        }

        internal MapsetRowSurface(MapsetRowSurfaceCache cache, Drawable parent)
        {
            Cache = cache;
            Sprites = new[]
            {
                CreateSprite(parent),
                CreateSprite(parent)
            };
        }

        /// <summary>
        ///     Requests the row state and keeps the currently completed surface visible
        /// </summary>
        internal void Set(Texture2D banner, bool hovered, bool selected)
        {
            PromotePendingSurface();
            var key = Cache.GetKey(banner, hovered, selected);

            if (PendingKey == key)
                return;

            if (CurrentKey == key)
            {
                CancelPendingSurface();
                return;
            }

            CancelPendingSurface();

            PendingIndex = CurrentIndex == 0 ? 1 : 0;
            PendingKey = key;
            Cache.Bind(Sprites[PendingIndex], key);
            PromotePendingSurface();
        }

        /// <summary>
        ///     Removes surfaces belonging to a previously pooled mapset
        /// </summary>
        internal void Reset()
        {
            foreach (var sprite in Sprites)
                Cache.Unbind(sprite);

            CurrentIndex = PendingIndex = -1;
            CurrentKey = PendingKey = null;
        }

        private void PromotePendingSurface()
        {
            if (PendingIndex < 0 || !Sprites[PendingIndex].Visible)
                return;

            var previousIndex = CurrentIndex;
            CurrentIndex = PendingIndex;
            CurrentKey = PendingKey;
            PendingIndex = -1;
            PendingKey = null;

            if (previousIndex >= 0 && previousIndex != CurrentIndex)
                Cache.Unbind(Sprites[previousIndex]);
        }

        private void CancelPendingSurface()
        {
            if (PendingIndex < 0)
                return;

            Cache.Unbind(Sprites[PendingIndex]);
            PendingIndex = -1;
            PendingKey = null;
        }

        private static Sprite CreateSprite(Drawable parent) => new()
        {
            Parent = parent,
            UsePreviousSpriteBatchOptions = true,
            Visible = false
        };

        public void Dispose()
        {
            Reset();
            foreach (var sprite in Sprites)
                sprite.Destroy();
        }
    }
}
