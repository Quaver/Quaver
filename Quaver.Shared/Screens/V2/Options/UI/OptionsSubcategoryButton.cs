using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Options.UI
{
    /// <summary>
    ///     A subcategory button in the panel next to the rail.
    /// </summary>
    internal sealed class OptionsSubcategoryButton : RoundedButton
    {
        private SkinV2OptionsCategoryNavigationConfig Config { get; }

        private MarqueeSpriteText Marquee { get; }

        private Color IdleColor { get; }

        private Color SelectedColor { get; }

        private Color ForegroundColor { get; }

        private Color SelectedForegroundColor { get; }

        private bool Selected { get; set; }

        internal string LocalizationKey { get; }

        internal OptionsSubcategoryButton(string localizationKey, WobbleFontStore font,
            SkinV2OptionsCategoryNavigationConfig config, EventHandler clickAction) : base(clickAction)
        {
            LocalizationKey = localizationKey;
            Config = config;
            IdleColor = SkinV2Color.Parse(config.SubcategoryButtonColor);
            SelectedColor = SkinV2Color.Parse(config.SubcategoryButtonSelectedColor);
            ForegroundColor = SkinV2Color.Parse(config.ForegroundColor);
            SelectedForegroundColor = SkinV2Color.Parse(config.SelectedForegroundColor);
            Size = new ScalableVector2(config.ButtonHeight, config.ButtonHeight);
            CornerRadius = config.CornerRadius;
            PerformHoverFade = true;

            Marquee = new MarqueeSpriteText(font, LocalizationManager.Get(localizationKey), config.FontSize, 1)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true
            };

            ApplyContentLayout();
            ApplyColors();
        }

        public override void Update(GameTime gameTime)
        {
            Marquee.IsActive = IsHovered || Selected;
            base.Update(gameTime);
            ApplyContentLayout();
            ApplyColors();
        }

        internal void SetSelected(bool selected)
        {
            Selected = selected;
            Marquee.IsActive = IsHovered || Selected;
            ApplyColors();
        }

        private void ApplyContentLayout()
        {
            if (Marquee == null)
                return;

            Marquee.Alignment = Alignment.MidLeft;
            Marquee.X = Config.HorizontalPadding;
            Marquee.Size = new ScalableVector2(Math.Max(1, Width - Config.HorizontalPadding * 2), Height);
        }

        private void ApplyColors()
        {
            Tint = Selected ? SelectedColor : IdleColor;

            if (Marquee?.TextSprite != null)
                Marquee.TextSprite.Tint = Selected ? SelectedForegroundColor : ForegroundColor;
        }
    }
}
