using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Logging;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     Caches rounded render-target copies so repeated images can use the normal sprite batch
    /// </summary>
    internal sealed class RoundedTextureCache(int width, int height, float radius) : IDisposable
    {
        private const int MaximumCachedTextures = 64;

        public int Width { get; } = width;
        public int Height { get; } = height;

        private float Radius { get; } = radius;
        private object SyncRoot { get; } = new();
        private Dictionary<Texture2D, CachedTexture> Textures { get; } = new();
        private Dictionary<Sprite, CachedTexture> Bindings { get; } = new();
        private Shader Shader { get; set; }
        private long LastUsed { get; set; }
        private bool IsDisposed { get; set; }

        public void Bind(Sprite sprite, Texture2D source)
        {
            lock (SyncRoot)
            {
                if (IsDisposed || sprite.IsDisposed)
                    return;

                if (Bindings.TryGetValue(sprite, out var previous) && previous.Source == source)
                    return;

                Unbind(sprite);
                if (source == null || source.IsDisposed)
                    return;

                if (!Textures.TryGetValue(source, out var cached))
                {
                    cached = new CachedTexture(source);
                    Textures.Add(source, cached);
                    var pending = cached;
                    GameBase.Game.ScheduleRenderTargetDraw(() => CreateTexture(pending));
                }

                cached.LastUsed = ++LastUsed;
                cached.Users.Add(sprite);
                Bindings.Add(sprite, cached);
                ApplyTexture(sprite, cached);
                TrimCache();
            }
        }

        public void Unbind(Sprite sprite)
        {
            lock (SyncRoot)
            {
                if (Bindings.Remove(sprite, out var cached))
                {
                    cached.Users.Remove(sprite);
                    cached.LastUsed = ++LastUsed;
                }

                sprite.Visible = false;
                TrimCache();
            }
        }

        private static void ApplyTexture(Sprite sprite, CachedTexture cached)
        {
            sprite.Image = cached.Texture;
            sprite.Visible = cached.Texture != null && !cached.Texture.IsDisposed;
        }

        private void CreateTexture(CachedTexture cached)
        {
            lock (SyncRoot)
            {
                if (IsDisposed || !Textures.TryGetValue(cached.Source, out var current) || current != cached)
                    return;

                if (cached.Users.Count == 0)
                {
                    Textures.Remove(cached.Source);
                    return;
                }

                if (cached.Source.IsDisposed)
                    return;

                try
                {
                    cached.Texture = RenderTexture(cached.Source);
                    foreach (var sprite in cached.Users)
                    {
                        if (!sprite.IsDisposed)
                            ApplyTexture(sprite, cached);
                    }
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, LogType.Runtime);
                }
            }
        }

        private RenderTarget2D RenderTexture(Texture2D source)
        {
            var game = GameBase.Game;
            var device = game.GraphicsDevice;

            var previousTargets = device.GetRenderTargets();
            var viewport = device.Viewport;
            var scissor = device.ScissorRectangle;

            var target = new RenderTarget2D(device, Width, Height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            try
            {
                game.TryEndBatch();

                if (Shader == null)
                {
                    Shader = RoundedRectShader.Create(Radius);
                    RoundedRectShader.UpdateSize(Shader, new Vector2(Width, Height));
                }

                device.SetRenderTarget(target);
                device.Clear(Color.Transparent);

                // Preserve straight alpha so the ordinary sprite applies blending only once
                game.BeginBatch(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, Shader.ShaderEffect, Matrix.Identity);
                game.SpriteBatch.Draw(source, new Rectangle(0, 0, Width, Height), Color.White);
                game.TryEndBatch();

                return target;
            }
            catch
            {
                target.Dispose();
                throw;
            }
            finally
            {
                game.TryEndBatch();

                device.SetRenderTargets(previousTargets);
                device.Viewport = viewport;
                device.ScissorRectangle = scissor;
            }
        }

        private void TrimCache()
        {
            while (Textures.Count > MaximumCachedTextures)
            {
                CachedTexture oldest = null;
                foreach (var cached in Textures.Values)
                {
                    if (cached.Users.Count == 0 && (oldest == null || cached.LastUsed < oldest.LastUsed))
                        oldest = cached;
                }

                // Textures displayed by pooled rows stay alive until these rows release them
                if (oldest == null)
                    return;

                Textures.Remove(oldest.Source);

                var texture = oldest.Texture;
                if (texture != null)
                    GameBase.Game.ScheduleRenderTargetDraw(() => texture.Dispose());
            }
        }

        public void Dispose()
        {
            lock (SyncRoot)
            {
                if (IsDisposed)
                    return;

                IsDisposed = true;

                var textures = new List<CachedTexture>(Textures.Values);
                Textures.Clear();
                Bindings.Clear();
                GameBase.Game.ScheduleRenderTargetDraw(() =>
                {
                    GameBase.Game.TryEndBatch();
                    foreach (var cached in textures)
                        cached.Texture?.Dispose();

                    Shader?.Dispose();
                    Shader = null;
                });
            }
        }

        /// <summary>
        ///     Tracks one generated texture and every sprite currently using it
        /// </summary>
        private sealed class CachedTexture(Texture2D source)
        {
            public Texture2D Source { get; } = source;
            public RenderTarget2D Texture { get; set; }
            public HashSet<Sprite> Users { get; } = new();
            public long LastUsed { get; set; }
        }
    }
}
