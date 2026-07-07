using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Graphics.Shaders;
using Wobble.Graphics;
using Wobble.Graphics.Shaders;
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
    ///     A button with a code-drawn, anti-aliased rounded-rect background (see <see cref="RoundedRectShader"/>)
    ///     instead of a pre-baked texture, so it never stretch-blurs at arbitrary sizes.
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
                UpdateShaderRadius();
            }
        }

        /// <summary>
        ///     Whether to dim the button's <see cref="Sprite.Alpha"/> on hover, matching the existing <c>IconButton</c> feedback.
        ///     Set to <c>false</c> for buttons that implement their own hover/selected visuals.
        /// </summary>
        public bool PerformHoverFade { get; set; } = true;

        public Sprite Icon { get; private set; }

        public SpriteTextPlus Label { get; private set; }

        /// <summary>
        ///     The shader instance drawing this button's rounded-rect background.
        /// </summary>
        private Shader BackgroundShader { get; }

        /// <inheritdoc />
        public RoundedButton(EventHandler clickAction = null) : base(clickAction)
        {
            BackgroundShader = RoundedRectShader.Create(0);

            // Scissor test must stay enabled so this still gets clipped when nested inside a
            // ScrollContainer (which relies on RasterizerState.ScissorTestEnable to confine its content) -
            // the default SpriteBatchOptions.RasterizerState (CullNone) doesn't enable it.
            SpriteBatchOptions = new SpriteBatchOptions
            {
                Shader = BackgroundShader,
                RasterizerState = RoundedRectShader.ScissorSafeRasterizerState
            };
        }

        /// <summary>
        ///     Creates/updates the icon child, laying content back out afterwards.
        /// </summary>
        public void SetIcon(Texture2D texture, Vector2? size = null)
        {
            var iconSize = size ?? new Vector2(16, 16);

            if (Icon == null)
            {
                // Its own (shader-less) scissor-safe batch: sharing the background's rounded-rect shader
                // batch would corrupt the icon (the shader's coverage math is sized to the button's
                // rectangle, not the icon's much smaller one), but it still needs scissor testing enabled
                // so it respects an ancestor ScrollContainer's clip region like the background does.
                Icon = new Sprite
                {
                    Parent = this,
                    Alignment = Alignment.MidCenter,
                    SpriteBatchOptions = RoundedRectShader.CreateScissorSafeOptions()
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
                    Alignment = Alignment.MidCenter
                };

                // Reuse the icon's scissor-safe batch if there is one, otherwise get its own - either way,
                // avoid sharing the background's rounded-rect shader batch (see SetIcon for why).
                if (Icon != null)
                    Label.UsePreviousSpriteBatchOptions = true;
                else
                    Label.SpriteBatchOptions = RoundedRectShader.CreateScissorSafeOptions();
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
            UpdateShaderSize();
            UpdateShaderRadius();
        }

        private void UpdateShaderSize() => RoundedRectShader.UpdateSize(BackgroundShader, new Vector2(Width, Height));

        private void UpdateShaderRadius() =>
            // Radius must never exceed half of the smaller dimension - the SDF in RoundedRectShader
            // degenerates past that point (see https://github.com/raysan5/raylib/blob/65abee1cbade6bf7edf55da6eb1eed6980aa754b/src/rshapes.c#L706
            // for the equivalent clamp raylib applies), producing a seam where the two rounded halves
            // overlap instead of a clean curve.
            RoundedRectShader.UpdateRadius(BackgroundShader, Math.Min(CornerRadius ?? Height / 2f, Math.Min(Width, Height) / 2f));

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            if (PerformHoverFade)
            {
                var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
                Alpha = MathHelper.Lerp(Alpha, IsHovered ? 0.75f : 1f, (float) Math.Min(dt / 60, 1));
            }

            base.Update(gameTime);
        }
    }
}
