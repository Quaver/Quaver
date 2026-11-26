using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Form
{
    internal sealed class SharedTextAtlasDefinition
    {
        internal string Key { get; }
        internal string Text { get; }
        internal WobbleFontStore Font { get; }
        internal int FontSize { get; }

        internal SharedTextAtlasDefinition(string key, string text, WobbleFontStore font, int fontSize)
        {
            Key = key;
            Text = text;
            Font = font;
            FontSize = fontSize;
        }
    }

    /// <summary>
    ///     Stores a label's texture region and its layout size
    /// </summary>
    internal sealed class SharedTextAtlasEntry
    {
        internal Rectangle Source { get; }
        internal Vector2 DisplaySize { get; }
        internal Vector2 ContentSize { get; }
        internal Vector2 ContentInset { get; }
        internal Vector2 LayoutSize { get; }
        internal float CapTopOffset { get; }

        internal SharedTextAtlasEntry(Rectangle source, float scale, float contentWidth, float contentHeight, float padding, float layoutWidth, float layoutHeight, float capTopOffset)
        {
            Source = source;
            DisplaySize = new Vector2(source.Width / scale, source.Height / scale);
            ContentSize = new Vector2(contentWidth / scale, contentHeight / scale);
            ContentInset = new Vector2(padding / scale);
            LayoutSize = new Vector2(layoutWidth / scale, layoutHeight / scale);
            CapTopOffset = capTopOffset / scale;
        }
    }

    /// <summary>
    ///     Renders a set collection of labels once so that every label can share a single texture
    /// </summary>
    internal sealed class SharedTextAtlas : IDisposable
    {
        private const int EntryGap = 2;
        private const int MaximumAtlasWidth = 4096;

        private sealed class LayoutEntry
        {
            internal SharedTextAtlasDefinition Definition { get; }
            internal Vector2 DrawPosition { get; }

            internal LayoutEntry(SharedTextAtlasDefinition definition, Vector2 drawPosition)
            {
                Definition = definition;
                DrawPosition = drawPosition;
            }
        }

        private sealed class MeasuredEntry
        {
            internal SharedTextAtlasDefinition Definition { get; }
            internal Vector2 DrawOffset { get; }
            internal float ContentWidth { get; }
            internal float ContentHeight { get; }
            internal float Padding { get; }
            internal float LayoutWidth { get; }
            internal float LayoutHeight { get; }
            internal float CapTopOffset { get; }
            internal int Width { get; }
            internal int Height { get; }

            internal MeasuredEntry(SharedTextAtlasDefinition definition, Vector2 drawOffset,
                float contentWidth, float contentHeight, float padding, float layoutWidth,
                float layoutHeight, float capTopOffset, int width, int height)
            {
                Definition = definition;
                DrawOffset = drawOffset;
                ContentWidth = contentWidth;
                ContentHeight = contentHeight;
                Padding = padding;
                LayoutWidth = layoutWidth;
                LayoutHeight = layoutHeight;
                CapTopOffset = capTopOffset;
                Width = width;
                Height = height;
            }
        }

        private Dictionary<string, SharedTextAtlasEntry> Entries { get; } = new();
        private List<LayoutEntry> Layout { get; } = new();
        private float RenderScale { get; }
        private int AtlasWidth { get; }
        private int AtlasHeight { get; }
        private RenderTarget2D? Texture { get; set; }
        private bool IsDisposed { get; set; }

        internal event EventHandler? TextureChanged;

        /// <summary>
        ///     Measures and vertically packs the labels before scheduling their texture creation
        /// </summary>
        internal SharedTextAtlas(IReadOnlyList<SharedTextAtlasDefinition> definitions)
        {
            if (definitions == null || definitions.Count == 0)
                throw new ArgumentException("A text atlas needs at least one label.", nameof(definitions));

            RenderScale = Math.Max(1, Math.Max(WindowManager.ScreenScale.X, WindowManager.ScreenScale.Y));
            var previousSizes = definitions.Select(x => x.Font).Distinct().ToDictionary(font => font, font => font.FontSize);
            var measuredEntries = new List<MeasuredEntry>(definitions.Count);
            var keys = new HashSet<string>();

            try
            {
                foreach (var definition in definitions)
                {
                    if (!keys.Add(definition.Key))
                        throw new ArgumentException($"Duplicate text atlas key: {definition.Key}", nameof(definitions));

                    definition.Font.FontSize = definition.FontSize * RenderScale;
                    var bounds = definition.Font.Store.TextBounds(definition.Text, Vector2.Zero);
                    var lineBounds = definition.Font.Store.TextBounds("H", Vector2.Zero);
                    var padding = Math.Max(2, definition.FontSize * RenderScale * 0.25f);
                    var drawOffset = new Vector2(padding - bounds.X, padding - lineBounds.Y);
                    
                    var measured = definition.Font.Store.MeasureString(definition.Text);
                    var contentWidth = Math.Max(measured.X, bounds.X2 - bounds.X);
                    var contentHeight = Math.Max(1, lineBounds.Y2 - lineBounds.Y);
                    
                    var fullBounds = definition.Font.Store.TextBounds("Hgj", Vector2.Zero);
                    var fullHeight = fullBounds.Y2 - fullBounds.Y;
                    
                    var extensionHeight = Math.Max(0, fullHeight - contentHeight);
                    var layoutHeight = Math.Max(definition.Font.Store.LineHeight, contentHeight + extensionHeight * 2f);

                    var capTopOffset = (layoutHeight - contentHeight) / 2f;
                    var layoutWidth = (float) Math.Ceiling(measured.X);
                    var entryWidth = Math.Max(1, (int) Math.Ceiling(contentWidth + padding * 2));
                    var entryHeight = Math.Max(1, (int) Math.Ceiling(contentHeight + padding * 2));
                    measuredEntries.Add(new MeasuredEntry(definition, drawOffset, contentWidth, contentHeight, padding, layoutWidth, layoutHeight, capTopOffset, entryWidth, entryHeight));
                }
            }
            finally
            {
                foreach (var pair in previousSizes)
                    pair.Key.FontSize = pair.Value;
            }

            var x = 0;
            var y = 0;
            var rowHeight = 0;
            var atlasWidth = 1;

            foreach (var measured in measuredEntries)
            {
                if (x > 0 && x + measured.Width > MaximumAtlasWidth)
                {
                    x = 0;
                    y += rowHeight + EntryGap;
                    rowHeight = 0;
                }

                var source = new Rectangle(x, y, measured.Width, measured.Height);
                Entries.Add(measured.Definition.Key, new SharedTextAtlasEntry(source, RenderScale, measured.ContentWidth, measured.ContentHeight, measured.Padding, measured.LayoutWidth, measured.LayoutHeight, measured.CapTopOffset));
                Layout.Add(new LayoutEntry(measured.Definition, new Vector2(x, y) + measured.DrawOffset));

                x += measured.Width + EntryGap;
                rowHeight = Math.Max(rowHeight, measured.Height);
                atlasWidth = Math.Max(atlasWidth, x - EntryGap);
            }

            AtlasWidth = atlasWidth;
            AtlasHeight = Math.Max(1, y + rowHeight);
            GameBase.Game.ScheduleRenderTargetDraw(Build);
        }

        internal SharedTextAtlasEntry GetEntry(string key) => Entries.TryGetValue(key, out var entry) ? entry : throw new KeyNotFoundException($"Text atlas key not found: {key}");

        /// <summary>
        ///     Returns a label's region once the atlas texture is ready
        /// </summary>
        internal TextureRegion? GetRegion(string key)
        {
            var texture = Texture;
            if (texture is null || texture.IsDisposed)
                return null;

            return new TextureRegion(texture, GetEntry(key).Source);
        }

        /// <summary>
        ///     Draws every label into one render during the draw phase
        /// </summary>
        private void Build()
        {
            if (IsDisposed)
                return;

            _ = GameBase.Game.TryEndBatch();
            var graphicsDevice = GameBase.Game.GraphicsDevice;
            var previousTargets = graphicsDevice.GetRenderTargets();
            var previousScissor = graphicsDevice.ScissorRectangle;

            var previousSizes = Layout.Select(x => x.Definition.Font).Distinct().ToDictionary(font => font, font => font.FontSize);
            var texture = new RenderTarget2D(graphicsDevice, AtlasWidth, AtlasHeight, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

            try
            {
                graphicsDevice.SetRenderTarget(texture);
                graphicsDevice.ScissorRectangle = new Rectangle(0, 0, AtlasWidth, AtlasHeight);
                graphicsDevice.Clear(Color.Transparent);

                new SpriteBatchOptions
                {
                    DoNotScale = true,
                    BlendState = BlendState.AlphaBlend
                }.Begin();

                foreach (var item in Layout)
                {
                    item.Definition.Font.FontSize = item.Definition.FontSize * RenderScale;
                    item.Definition.Font.Store.DrawText(GameBase.Game.SpriteBatch, item.Definition.Text, item.DrawPosition, Color.White);
                }

                _ = GameBase.Game.TryEndBatch();
            }
            finally
            {
                foreach (var pair in previousSizes)
                    pair.Key.FontSize = pair.Value;

                graphicsDevice.SetRenderTargets(previousTargets);
                graphicsDevice.ScissorRectangle = previousScissor;
            }

            if (IsDisposed)
            {
                texture.Dispose();
                return;
            }

            Texture?.Dispose();
            Texture = texture;
            TextureChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            TextureChanged = null;
            var texture = Texture;
            Texture = null;
            if (texture is null || texture.IsDisposed)
                return;

            if (System.Threading.Thread.CurrentThread.ManagedThreadId == GameBase.Game.MainThreadId)
                texture.Dispose();
            else
                GameBase.Game.ScheduleRenderTargetDraw(texture.Dispose);
        }
    }

    /// <summary>
    ///     Displays one label from a shared text atlas while preserving its text bounds
    /// </summary>
    internal sealed class SharedTextAtlasSprite : Sprite
    {
        private SharedTextAtlas Atlas { get; }
        internal SharedTextAtlasEntry Entry { get; private set; } = null!;
        private string Key { get; set; } = string.Empty;
        private bool Displayed { get; set; } = true;

        internal float ContentWidth => Entry.ContentSize.X;
        internal float ContentHeight => Entry.ContentSize.Y;
        internal Vector2 ContentInset => Entry.ContentInset;
        internal float LayoutWidth => Entry.LayoutSize.X;
        internal float LayoutHeight => Entry.LayoutSize.Y;
        internal float CapTopOffset => Entry.CapTopOffset;

        internal SharedTextAtlasSprite(SharedTextAtlas atlas, string key)
        {
            Atlas = atlas ?? throw new ArgumentNullException(nameof(atlas));
            Atlas.TextureChanged += OnTextureChanged;
            SetKey(key);
        }

        /// <summary>
        ///     Switches the sprite to another label stored in the same atlas
        /// </summary>
        internal void SetKey(string key)
        {
            Key = key;
            Entry = Atlas.GetEntry(key);
            Size = new ScalableVector2(Entry.DisplaySize.X, Entry.DisplaySize.Y);
            RefreshRegion();
        }

        private void OnTextureChanged(object? sender, EventArgs args) => RefreshRegion();

        /// <summary>
        ///     Preserves the requested visibility when the deferred atlas texture becomes available
        /// </summary>
        internal void SetDisplayed(bool displayed)
        {
            Displayed = displayed;
            RefreshRegion();
        }

        /// <summary>
        ///     Connects the sprite to its texture region after the deferred atlas build completes
        /// </summary>
        private void RefreshRegion()
        {
            var region = Atlas.GetRegion(Key);

            Visible = Displayed && region.HasValue;
            if (region.HasValue)
                Region = region.Value;
        }

        public override void Destroy()
        {
            Atlas.TextureChanged -= OnTextureChanged;
            base.Destroy();
        }
    }
}
