using System.ComponentModel.DataAnnotations;
using Quaver.Shared.Skinning.V2;
using Wobble.Configuration;
using Wobble.Graphics.UI.Navigation;

namespace Quaver.Shared.Screens.V2.Main
{
    /// <summary>
    ///     Skin configuration owned by the V2 main menu.
    /// </summary>
    public sealed class SkinV2MainConfig
    {
        [Required]
        public SkinV2MainLayoutConfig Layout { get; set; } = new SkinV2MainLayoutConfig();

        [Required]
        [ConfigEditable]
        public SkinV2BackgroundConfig Background { get; set; } =
            new SkinV2BackgroundConfig
            {
                Type = NavigationBarBackgroundType.Image,
                SolidColor = "#1F88FF26",
                Image = new SkinV2BackgroundImageConfig { Fit = NavigationBarImageFit.Cover }
            };

        [Required]
        [ConfigEditable]
        public SkinV2MainBackgroundEffectsConfig BackgroundEffects { get; set; } =
            new SkinV2MainBackgroundEffectsConfig();

        [Required]
        public SkinV2MainLogoConfig Logo { get; set; } = new SkinV2MainLogoConfig();

        [Required]
        [ConfigEditable]
        public SkinV2MainActionsConfig Actions { get; set; } = new SkinV2MainActionsConfig();

        [Required]
        [ConfigEditable]
        public SkinV2MainNewsConfig News { get; set; } = new SkinV2MainNewsConfig();
    }

    public enum SkinV2MainBackgroundEffect
    {
        None,
        Particles,
        SoftPop,
        Matrix,
        RibbonTrail,
        Snowfall,
        StreakRain
    }

    public sealed class SkinV2MainBackgroundEffectsConfig
    {
        [EnumDataType(typeof(SkinV2MainBackgroundEffect))]
        public SkinV2MainBackgroundEffect Effect { get; set; } = SkinV2MainBackgroundEffect.Particles;

        [SkinColor]
        public string PrimaryColor { get; set; } = "#D9E3F4FF";

        [SkinColor]
        public string SecondaryColor { get; set; } = "#1FBBFFFF";

        [Required]
        public SkinV2MainBackgroundEffectIconsConfig Icons { get; set; } =
            new SkinV2MainBackgroundEffectIconsConfig();
    }

    public sealed class SkinV2MainBackgroundEffectIconsConfig
    {
        [SkinAssetPath]
        public string Primary { get; set; } = "";

        [SkinAssetPath]
        public string Secondary { get; set; } = "";

        [SkinAssetPath]
        public string Tertiary { get; set; } = "";
    }

    public sealed class SkinV2MainLayoutConfig
    {
        [Range(0, 2048)]
        public float HorizontalPadding { get; set; } = 160;

        [Range(1, 8192)]
        public float MinimumContentWidth { get; set; } = 640;

        [Range(1, 8192)]
        public float MinimumContentHeight { get; set; } = 620;

        [Range(0, 2048)]
        public float RowGap { get; set; } = 55;
    }

    public sealed class SkinV2MainLogoConfig
    {
        [SkinAssetPath]
        public string Image { get; set; } = "";

        [Range(1, 8192)]
        public float MinimumWidth { get; set; } = 320;

        [Range(0, 2048)]
        public float HorizontalMargin { get; set; } = 40;

        [Range(0.01, 1)]
        public float ViewportWidthRatio { get; set; } = 0.4166667f;
    }

    public sealed class SkinV2MainActionsConfig
    {
        [Range(0, 2048)]
        public float Gap { get; set; } = 34;

        [Range(0, 2048)]
        public float RowGap { get; set; } = SkinV2Spacing.SpacingXl;

        [Range(1, 8192)]
        public float ButtonWidth { get; set; } = 340;

        [Range(1, 8192)]
        public float ButtonHeight { get; set; } = 64;

        [Range(1, 8192)]
        public float SingleRowHeight { get; set; } = 76;

        [Range(1, 8192)]
        public float WrappedRowHeight { get; set; } = 160;

        [Range(1, 8192)]
        public float FourColumnBreakpoint { get; set; } = 1400;

        [Range(0, 2048)]
        public float TwoColumnMargin { get; set; } = 80;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [SkinColor]
        public string Color { get; set; } = "#1F88FF26";

        [SkinColor]
        public string HoverColor { get; set; } = "#1F88FF0F";

        [SkinColor]
        public string AccentColor { get; set; } = "#1FBBFFFF";

        [SkinColor]
        public string TextColor { get; set; } = "#D9E3F4";

        [SkinFont]
        public string Font { get; set; } = SkinV2FontWeightsConfig.SemiBold;

        [Range(1, 256)]
        public int FontSize { get; set; } = SkinV2FontSizesConfig.Text2Xl;

        [Range(1, 8192)]
        public float IconSize { get; set; } = 24;

        [Range(1, 8192)]
        public float IndicatorWidth { get; set; } = 60;

        [Range(1, 8192)]
        public float IndicatorHeight { get; set; } = 15;

        [Range(0, 2048)]
        public float IndicatorSpacing { get; set; } = 4;

        [Range(0d, 1d)]
        public float IndicatorIdleOpacity { get; set; } = 0.45f;

        [SkinAssetPath]
        public string SinglePlayerIcon { get; set; } = "";

        [SkinAssetPath]
        public string MultiplayerIcon { get; set; } = "";

        [SkinAssetPath]
        public string EditorIcon { get; set; } = "";

        [SkinAssetPath]
        public string DownloadIcon { get; set; } = "";
    }

    public sealed class SkinV2MainNewsConfig
    {
        [Range(1, 8192)]
        public float MaximumWidth { get; set; } = 570;

        [Range(1, 8192)]
        public float MinimumWidth { get; set; } = 320;

        [Range(0, 2048)]
        public float HorizontalMargin { get; set; } = 40;

        /// <summary>
        ///     Vertical offset from the bottom edge of the window. Negative values move the banner up.
        /// </summary>
        [Range(-8192, 8192)]
        public float BottomOffset { get; set; } = -SkinV2Spacing.SpacingXs;

        [Range(1, 8192)]
        public float BannerHeight { get; set; } = 128;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [SkinColor]
        public string HoverOverlayColor { get; set; } = "#000000FF";

        [Range(0d, 1d)]
        public float HoverOverlayOpacity { get; set; } = 0.35f;

        [Range(1, 10000)]
        public int HoverTransitionMilliseconds { get; set; } = 100;

        [SkinAssetPath]
        public string FallbackImage { get; set; } = "";
    }
}
