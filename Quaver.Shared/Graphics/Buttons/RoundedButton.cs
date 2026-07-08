using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Graphics.Shaders;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Graphics.Buttons
{
    /// <summary>
    ///     Whether an axis of a <see cref="RoundedButton"/> should be a hard-coded size, or
    ///     calculated automatically from its content (<see cref="RoundedButton.Icon"/>/<see cref="RoundedButton.Label"/>) + padding.
    /// </summary>
    public enum ButtonSizeMode
    {
        Fixed,
        Auto
    }

    /// <summary>
    ///     A button with a code-generated, anti-aliased rounded-rect background instead of a pre-baked,
    ///     stretched asset. Exact-size textures are cached so multiple buttons retain normal SpriteBatch batching.
    /// </summary>
    public class RoundedButton : Button
    {
        /// <summary>
        ///     The gap between <see cref="Icon"/> and <see cref="Label"/> when both are present.
        /// </summary>
        private const int IconLabelSpacing = 8;

        /// <summary>
        /// </summary>
        public ButtonSizeMode WidthMode { get; set; } = ButtonSizeMode.Fixed;

        /// <summary>
        /// </summary>
        public ButtonSizeMode HeightMode { get; set; } = ButtonSizeMode.Fixed;

        /// <summary>
        ///     Total padding added around the content bounding box when <see cref="WidthMode"/>/<see cref="HeightMode"/> is <see cref="ButtonSizeMode.Auto"/>.
        /// </summary>
        public Vector2 AutoSizePadding { get; set; } = new Vector2(32, 12);

        private float? _cornerRadius;

        /// <summary>
        ///     The corner radius, in pixels. Defaults to <c>null</c>, which resolves to a full pill (<see cref="Drawable.Height"/> / 2).
        /// </summary>
        public float? CornerRadius
        {
            get => _cornerRadius;
            set
            {
                _cornerRadius = value;
                UpdateBackgroundTexture();
            }
        }

        /// <summary>
        ///     Whether to dim the button's <see cref="Sprite.Alpha"/> on hover, matching the existing <c>IconButton</c> feedback.
        ///     Set to <c>false</c> for buttons that implement their own hover/selected visuals.
        /// </summary>
        public bool PerformHoverFade { get; set; } = true;

        public Sprite Icon { get; private set; }

        public SpriteTextPlus Label { get; private set; }

        /// <inheritdoc />
        public RoundedButton(EventHandler clickAction = null) : base(clickAction) => SetChildrenAlpha = true;

        /// <summary>
        ///     Creates/updates the icon child, laying content back out afterwards.
        /// </summary>
        public void SetIcon(Texture2D texture, Vector2? size = null)
        {
            var iconSize = size ?? new Vector2(16, 16);

            if (Icon == null)
            {
                Icon = new Sprite
                {
                    Parent = this,
                    Alignment = Alignment.MidCenter,
                    UsePreviousSpriteBatchOptions = true
                };
            }

            Icon.Image = texture;
            Icon.Size = new ScalableVector2(iconSize.X, iconSize.Y);

            LayoutContent();
        }

        /// <summary>
        ///     Creates/updates the label child, laying content back out afterwards.
        /// </summary>
        public void SetLabel(WobbleFontStore font, string text, int fontSize, Color? color = null)
        {
            if (Label == null)
            {
                Label = new SpriteTextPlus(font, text, fontSize)
                {
                    Parent = this,
                    Alignment = Alignment.MidCenter,
                    UsePreviousSpriteBatchOptions = true
                };
            }
            else
                Label.Text = text;

            if (color != null)
                Label.Tint = color.Value;

            LayoutContent();
        }

        /// <summary>
        ///     Positions <see cref="Icon"/>/<see cref="Label"/> relative to each other, then recalculates
        ///     any axis in <see cref="ButtonSizeMode.Auto"/> mode from the resulting content bounds.
        /// </summary>
        private void LayoutContent()
        {
            if (Icon != null && Label != null)
            {
                var totalWidth = Icon.Width + IconLabelSpacing + Label.Width;
                Icon.X = -totalWidth / 2f + Icon.Width / 2f;
                Label.X = Icon.X + Icon.Width / 2f + IconLabelSpacing + Label.Width / 2f;
            }

            RecalculateAutoSize();
        }

        /// <summary>
        ///     Recalculates <see cref="Drawable.Width"/>/<see cref="Drawable.Height"/> for whichever axes
        ///     are in <see cref="ButtonSizeMode.Auto"/> mode, based on the current <see cref="Icon"/>/<see cref="Label"/> content.
        /// </summary>
        public void RecalculateAutoSize()
        {
            if (WidthMode == ButtonSizeMode.Auto)
            {
                var contentWidth = (Icon?.Width ?? 0) + (Label?.Width ?? 0);

                if (Icon != null && Label != null)
                    contentWidth += IconLabelSpacing;

                Width = contentWidth + AutoSizePadding.X;
            }

            if (HeightMode == ButtonSizeMode.Auto)
                Height = Math.Max(Icon?.Height ?? 0, Label?.Height ?? 0) + AutoSizePadding.Y;
        }

        /// <inheritdoc />
        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();
            UpdateBackgroundTexture();
        }

        private void UpdateBackgroundTexture()
        {
            if (Width <= 0 || Height <= 0)
                return;

            var radius = Math.Min(CornerRadius ?? Height / 2f, Math.Min(Width, Height) / 2f);
            var texture = RoundedRectTextureCache.Get(Width, Height, radius);

            if (Image != texture)
                Image = texture;
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            if (PerformHoverFade)
            {
                var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
                var targetAlpha = IsHovered ? 0.75f : 1f;

                if (Alpha != targetAlpha)
                {
                    var alpha = MathHelper.Lerp(Alpha, targetAlpha, (float) Math.Min(dt / 60, 1));
                    Alpha = Math.Abs(alpha - targetAlpha) < 0.001f ? targetAlpha : alpha;
                }
            }

            base.Update(gameTime);
        }
    }
}
