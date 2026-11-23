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
        [Required] public SkinV2SelectionFilterConfig Filter { get; set; } = new();
        [Required] public SkinV2SelectionRowConfig Mapset { get; set; } = new() { Height = 100, TitleFont = SkinV2FontWeightsConfig.Bold, TitleFontSize = 22 };
        [Required] public SkinV2SelectionRowConfig Difficulty { get; set; } = new() { SelectedWidth = 925, Height = 60, TitleFontSize = 20 };
        [Required] public SkinV2SelectionDetailsConfig Details { get; set; } = new();
    }

    public sealed class SkinV2SelectionLayoutConfig
    {
        [Range(0, 2048)] public float HorizontalPadding { get; set; } = SkinV2Spacing.SpacingXl;
        [Range(0, 2048)] public float VerticalPadding { get; set; } = SkinV2Spacing.Spacing2Xs;
        [Range(0, 2048)] public float ContentGap { get; set; } = SkinV2Spacing.SpacingXl;
        [Range(1, 4096)] public float LeftPanelWidth { get; set; } = 725;
        [Range(1, 4096)] public float RightPanelWidth { get; set; } = 1040;
        [Range(0, 256)] public float FilterListGap { get; set; } = 10;
    }

    public sealed class SkinV2SelectionFilterConfig
    {
        [Range(1, 512)] public float Height { get; set; } = 60;
        [Range(1, 1024)] public float ExpandedHeight { get; set; } = 130;
        [Range(0, 128)] public float CornerRadius { get; set; } = 6;
        [Range(1, 256)] public float ExpandButtonSize { get; set; } = 40;
        [Range(0, 256)] public float ExpandButtonRightInset { get; set; } = 10;
        [Range(1, 256)] public float ExpandIconSize { get; set; } = 30;
        [Range(0, 10000)] public int ExpansionMilliseconds { get; set; } = 180;
        [ConfigEditable, SkinColor] public string BackgroundColor { get; set; } = "#26313BFF";
        [ConfigEditable, SkinColor] public string ExpandButtonColor { get; set; } = "#161D26FF";
        [ConfigEditable, SkinColor] public string ExpandIconColor { get; set; } = "#FFFFFFFF";
    }

    public sealed class SkinV2SelectionListConfig
    {
        [Range(0, 128)] public int Gap { get; set; } = 10;
        [Range(1, 8192)] public int ScrollSpeed { get; set; } = 320;
        [Range(1, 10000)] public int ScrollMilliseconds { get; set; } = 400;
        [Range(1, 64)] public int OverscanRows { get; set; } = 4;
        [Range(1, 64)] public float ScrollbarWidth { get; set; } = 10;
        [ConfigEditable, SkinColor] public string ScrollbarColor { get; set; } = "#C5D3E7FF";
    }

    public sealed class SkinV2SelectionRowConfig
    {
        [Range(1, 4096)] public int Width { get; set; } = 900;
        [Range(1, 4096)] public int SelectedWidth { get; set; } = 950;
        [Range(40, 512)] public int Height { get; set; } = 80;
        [Range(0, 128)] public float CornerRadius { get; set; } = 6;
        [Range(0, 128)] public float Padding { get; set; } = SkinV2Spacing.SpacingBase;
        [SkinFont] public string Font { get; set; } = SkinV2FontWeightsConfig.Bold;
        [SkinFont] public string TitleFont { get; set; } = SkinV2FontWeightsConfig.Bold;
        [Range(1, 256)] public int TitleFontSize { get; set; } = SkinV2FontSizesConfig.TextLg;
        [Range(1, 256)] public int ArtistFontSize { get; set; } = 18;
        [Range(1, 256)] public int CreatorFontSize { get; set; } = 14;
        [Range(1, 256)] public float PillHeight { get; set; } = 30;
        [Range(1, 4096)] public float PillMinimumWidth { get; set; } = 90;
        [Range(0, 256)] public float PillGap { get; set; } = 8;
        [Range(0, 256)] public float PillHorizontalPadding { get; set; } = 24;
        [Range(1, 256)] public int PillFontSize { get; set; } = 14;
        [Range(1, 256)] public float DifficultyPillHeight { get; set; } = 36;
        [Range(1, 256)] public float DifficultyGradeSize { get; set; } = 40;
        [Range(0, 256)] public float DifficultyLeftPadding { get; set; } = 10;
        [Range(0, 256)] public float DifficultyGradeGap { get; set; } = 10;
        [Range(1, 4096)] public float DifficultyPillMinimumWidth { get; set; } = 90;
        [Range(0, 256)] public float DifficultyPillHorizontalPadding { get; set; } = 20;
        [Range(1, 256)] public int DifficultyPillFontSize { get; set; } = 14;
        [Range(0, 256)] public float DifficultyContentGap { get; set; } = 12;
        [Range(0, 256)] public float DifficultyStatsGap { get; set; } = 24;
        [Range(0, 256)] public float DifficultyStatGroupGap { get; set; } = 15;
        [Range(0, 256)] public float DifficultyStatItemGap { get; set; } = 8;
        [Range(0, 256)] public float DifficultyRightPadding { get; set; } = 10;
        [Range(1, 256)] public float DifficultyStatIconSize { get; set; } = 24;
        [Range(1, 256)] public int DifficultyStatFontSize { get; set; } = 18;
        [Range(1, 256)] public float BannerStatPillHeight { get; set; } = 30;
        [Range(0, 256)] public float BannerStatPillGap { get; set; } = 8;
        [Range(0, 256)] public float BannerStatPillPadding { get; set; } = 16;
        [Range(0, 256)] public float BannerStatInset { get; set; } = 8;
        [Range(1, 256)] public float BannerStatIconSize { get; set; } = 18;
        [Range(1, 256)] public int BannerStatFontSize { get; set; } = 14;
        [Range(0, 10000)] public int BannerStatFadeMilliseconds { get; set; } = 180;
        [ConfigEditable, SkinColor] public string BackgroundColor { get; set; } = "#162333FF";
        [ConfigEditable, SkinColor] public string HoveredBackgroundColor { get; set; } = "#263C55FF";
        [ConfigEditable, SkinColor] public string SelectedBackgroundColor { get; set; } = "#44618CFF";
        [ConfigEditable, SkinColor] public string TextColor { get; set; } = "#FFFFFFFF";
        [ConfigEditable, SkinColor] public string SecondaryTextColor { get; set; } = "#FFFFFFFF";
        [ConfigEditable, SkinColor] public string PillTextColor { get; set; } = "#FFFFFFFF";
        [ConfigEditable, SkinColor] public string DifficultyPillColor { get; set; } = "#111820F2";
        [ConfigEditable, SkinColor] public string DifficultyStatColor { get; set; } = "#FFFFFFFF";
        [ConfigEditable, SkinColor] public string BannerStatPillColor { get; set; } = "#111820E6";
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
