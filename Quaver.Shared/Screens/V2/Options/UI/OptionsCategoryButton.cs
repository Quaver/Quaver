using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Screens.V2.Options.Catalog;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Options.UI
{
    /// <summary>
    ///     A category button in the left rail: an icon, plus the category name once the rail opens.
    /// </summary>
    internal sealed class OptionsCategoryButton : RoundedButton
    {
        private SkinV2OptionsCategoryNavigationConfig Config { get; }

        private MarqueeSpriteText Marquee { get; }

        private Color CollapsedIdleColor { get; }

        private Color ExpandedIdleColor { get; }

        /// <summary>
        ///     The unselected background color, blended between the collapsed and expanded colors.
        /// </summary>
        private Color CurrentIdleColor { get; set; }

        private Color SelectedColor { get; }

        private Color ForegroundColor { get; }

        private Color SelectedForegroundColor { get; }

        private Color DimmedForegroundColor { get; }

        private bool Selected { get; set; }

        private bool Dimmed { get; set; }

        private float LabelExpansionProgress { get; set; }

        internal OptionsCategoryDefinition Definition { get; }

        internal OptionsCategoryButton(OptionsCategoryDefinition definition, TextureRegion icon,
            WobbleFontStore font, SkinV2OptionsCategoryNavigationConfig config, EventHandler clickAction)
            : base(clickAction)
        {
            Definition = definition;
            Config = config;
            CollapsedIdleColor = SkinV2Color.Parse(config.RailButtonCollapsedColor);
            ExpandedIdleColor = SkinV2Color.Parse(config.RailButtonExpandedColor);
            CurrentIdleColor = CollapsedIdleColor;
            SelectedColor = SkinV2Color.Parse(config.RailButtonSelectedColor);
            ForegroundColor = SkinV2Color.Parse(config.ForegroundColor);
            SelectedForegroundColor = SkinV2Color.Parse(config.SelectedForegroundColor);
            DimmedForegroundColor = SkinV2Color.Parse(config.SearchDimmedForegroundColor);
            Size = new ScalableVector2(config.ButtonHeight, config.ButtonHeight);
            CornerRadius = config.CornerRadius;
            PerformHoverFade = true;
            SetIcon(icon, new Vector2(config.IconSize, config.IconSize));

            Marquee = new MarqueeSpriteText(font, LocalizationManager.Get(definition.LocalizationKey),
                config.FontSize, 1)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true,
                Visible = false
            };

            ApplyContentLayout();
            ApplyColors();
        }

        public override void Update(GameTime gameTime)
        {
            RefreshMarqueeActivity();
            base.Update(gameTime);
            ApplyContentLayout();
            ApplyColors();
        }

        internal void SetSelected(bool selected)
        {
            Selected = selected;
            RefreshMarqueeActivity();
            ApplyColors();
        }

        /// <summary>
        ///     Greys the button out while a search is running, because the search entry is the selected one.
        /// </summary>
        internal void SetDimmed(bool dimmed)
        {
            if (Dimmed == dimmed)
                return;

            Dimmed = dimmed;
            ApplyColors();
        }

        /// <summary>
        ///     Fades the label in from 0 (rail closed) to 1 (rail open).
        /// </summary>
        internal void SetLabelExpansionProgress(float progress)
        {
            LabelExpansionProgress = MathHelper.Clamp(progress, 0, 1);
            Marquee.TextSprite.Alpha = LabelExpansionProgress;
            Marquee.Visible = LabelExpansionProgress > 0.001f;
            RefreshMarqueeActivity();
        }

        /// <summary>
        ///     Blends the unselected background from 0 (rail closed) to 1 (rail open).
        /// </summary>
        internal void SetIdleColorProgress(float progress)
        {
            CurrentIdleColor = Color.Lerp(CollapsedIdleColor, ExpandedIdleColor, MathHelper.Clamp(progress, 0, 1));
            ApplyColors();
        }

        /// <summary>
        ///     Long labels only scroll while visible and hovered or selected.
        /// </summary>
        private void RefreshMarqueeActivity() =>
            Marquee.IsActive = LabelExpansionProgress > 0.001f && (IsHovered || Selected);

        private void ApplyContentLayout()
        {
            if (Icon == null || Marquee == null)
                return;

            Icon.Alignment = Alignment.MidLeft;
            Icon.X = Math.Max(0, (Config.ButtonHeight - Config.IconSize) / 2f);
            Marquee.Alignment = Alignment.MidLeft;
            Marquee.X = Icon.X + Config.IconSize + Config.LabelSpacing;
            Marquee.Size = new ScalableVector2(Math.Max(1, Width - Marquee.X - Config.HorizontalPadding), Height);
        }

        private void ApplyColors()
        {
            var foreground = Dimmed ? DimmedForegroundColor : Selected ? SelectedForegroundColor : ForegroundColor;
            Tint = Selected && !Dimmed ? SelectedColor : CurrentIdleColor;

            if (Icon != null)
                Icon.Tint = foreground;

            if (Marquee?.TextSprite != null)
                Marquee.TextSprite.Tint = foreground;
        }
    }
}
