using System.ComponentModel.DataAnnotations;
using Quaver.Shared.Skinning.V2;
using Wobble.Configuration;

namespace Quaver.Shared.Screens.V2.Selection
{
    public sealed class SkinV2SelectionConfig
    {
        [Required, ConfigEditable]
        public SkinV2BackgroundConfig Background { get; set; } =
            new SkinV2BackgroundConfig { SolidColor = "#080D13FF" };

        [Required] public SkinV2SelectionLayoutConfig Layout { get; set; } = new();
        [Required] public SkinV2SelectionListConfig List { get; set; } = new();
        [Required] public SkinV2SelectionMapsetConfig Mapset { get; set; } = new();
        [Required] public SkinV2SelectionDetailsConfig Details { get; set; } = new();
    }

    public sealed class SkinV2SelectionLayoutConfig
    {
        [Range(0, 2048)] public float HorizontalPadding { get; set; } = SkinV2Spacing.SpacingXl;
        [Range(0, 2048)] public float VerticalPadding { get; set; } = SkinV2Spacing.SpacingBase;
        [Range(0, 2048)] public float ContentGap { get; set; } = SkinV2Spacing.SpacingXl;
        [Range(0.1, 0.8)] public float LeftPanelFraction { get; set; } = 0.4f;
    }

    public sealed class SkinV2SelectionListConfig
    {
        [Range(1, 8192)] public int ScrollSpeed { get; set; } = 320;
        [Range(1, 10000)] public int ScrollMilliseconds { get; set; } = 400;
        [Range(1, 64)] public int OverscanRows { get; set; } = 4;
        [Range(1, 64)] public float ScrollbarWidth { get; set; } = 10;
        [ConfigEditable, SkinColor] public string ScrollbarColor { get; set; } = "#C5D3E7FF";
    }

    public sealed class SkinV2SelectionMapsetConfig
    {
        [Range(40, 512)] public int Height { get; set; } = 80;
        [Range(0, 128)] public int Gap { get; set; } = 10;
        [Range(0, 128)] public float Padding { get; set; } = SkinV2Spacing.SpacingBase;
        [SkinFont] public string Font { get; set; } = SkinV2FontWeightsConfig.SemiBold;
        [Range(1, 256)] public int TitleFontSize { get; set; } = SkinV2FontSizesConfig.TextLg;
        [Range(1, 256)] public int ArtistFontSize { get; set; } = SkinV2FontSizesConfig.TextSm;
        [ConfigEditable, SkinColor] public string BackgroundColor { get; set; } = "#162333FF";
        [ConfigEditable, SkinColor] public string HoveredBackgroundColor { get; set; } = "#263C55FF";
        [ConfigEditable, SkinColor] public string SelectedBackgroundColor { get; set; } = "#44618CFF";
        [ConfigEditable, SkinColor] public string TextColor { get; set; } = "#FFFFFFFF";
        [ConfigEditable, SkinColor] public string SecondaryTextColor { get; set; } = "#B8C4D1FF";
    }

    public sealed class SkinV2SelectionDetailsConfig
    {
        [Range(0, 128)] public float Padding { get; set; } = SkinV2Spacing.SpacingXl;
        [Range(0, 128)] public float Gap { get; set; } = SkinV2Spacing.SpacingBase;
        [SkinFont] public string Font { get; set; } = SkinV2FontWeightsConfig.SemiBold;
        [Range(1, 256)] public int TitleFontSize { get; set; } = SkinV2FontSizesConfig.TextXl;
        [Range(1, 256)] public int FontSize { get; set; } = SkinV2FontSizesConfig.TextBase;
        [ConfigEditable, SkinColor] public string BackgroundColor { get; set; } = "#111B28FF";
        [ConfigEditable, SkinColor] public string TextColor { get; set; } = "#FFFFFFFF";
        [ConfigEditable, SkinColor] public string SecondaryTextColor { get; set; } = "#B8C4D1FF";
    }
}
