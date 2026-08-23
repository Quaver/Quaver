using System.ComponentModel.DataAnnotations;
using Quaver.Shared.Skinning.V2;
using Wobble.Configuration;

namespace Quaver.Shared.Screens.V2.Downloading
{
    /// <summary>
    ///     Skin configuration owned by the V2 Download screen.
    /// </summary>
    public sealed class SkinV2DownloadingConfig
    {
        [Required]
        [ConfigEditable]
        public SkinV2BackgroundConfig Background { get; set; } =
            new SkinV2BackgroundConfig { SolidColor = "#080D13FF" };

        [Required]
        public SkinV2DownloadingLayoutConfig Layout { get; set; } =
            new SkinV2DownloadingLayoutConfig();

        [Required]
        public SkinV2DownloadingSearchAreaConfig SearchArea { get; set; } =
            new SkinV2DownloadingSearchAreaConfig();

        [Required]
        public SkinV2DownloadingFieldConfig Field { get; set; } =
            new SkinV2DownloadingFieldConfig();

        [Required]
        public SkinV2DownloadingButtonConfig Button { get; set; } =
            new SkinV2DownloadingButtonConfig();

        [Required]
        public SkinV2DownloadingDropdownConfig Dropdown { get; set; } =
            new SkinV2DownloadingDropdownConfig();

        [Required]
        public SkinV2DownloadingRangeConfig Range { get; set; } =
            new SkinV2DownloadingRangeConfig();
    }

    public sealed class SkinV2DownloadingLayoutConfig
    {
        [Range(0, 2048)]
        public float HorizontalPadding { get; set; } = SkinV2Spacing.SpacingXl;

        [Range(0, 2048)]
        public float TopPadding { get; set; } = SkinV2Spacing.Spacing2Xs;

        [Range(1, 8192)]
        public float ReflowBreakpoint { get; set; } = 1120;
    }

    public sealed class SkinV2DownloadingSearchAreaConfig
    {
        [Range(0, 2048)]
        public float Padding { get; set; } = 8;

        [Range(0, 2048)]
        public float ColumnGap { get; set; } = 8;

        [Range(0, 2048)]
        public float RowGap { get; set; } = 8;

        [Range(1, 8192)]
        public float CompactHeight { get; set; } = 50;

        [Range(1, 8192)]
        public float ExpandedHeight { get; set; } = 90;

        [Range(1, 8192)]
        public float PlaylistHeight { get; set; } = 50;

        [Range(1, 10000)]
        public int ExpansionDurationMilliseconds { get; set; } = 180;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [ConfigEditable]
        [SkinColor]
        public string BackgroundColor { get; set; } = "#898989FF";
    }

    public sealed class SkinV2DownloadingFieldConfig
    {
        [Range(1, 8192)]
        public float Height { get; set; } = 34;

        [Range(1, 8192)]
        public float SearchWidth { get; set; } = 292;

        [Range(1, 8192)]
        public float SearchMinimumWidth { get; set; } = 220;

        [Range(1, 8192)]
        public float SearchIconSize { get; set; } = 15;

        [Range(0, 2048)]
        public float SearchIconInset { get; set; } = 9;

        [Range(1, 8192)]
        public float NumericWidth { get; set; } = 68;

        [Range(1, 8192)]
        public float NumericCompactWidth { get; set; } = 62;

        [Range(0, 2048)]
        public float TextInset { get; set; } = SkinV2MarginsConfig.Md;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [SkinFont]
        public string Font { get; set; } = SkinV2FontWeightsConfig.SemiBold;

        [Range(1, 256)]
        public int FontSize { get; set; } = SkinV2FontSizesConfig.TextSm;

        [ConfigEditable]
        [SkinColor]
        public string BackgroundColor { get; set; } = "#555555FF";

        [ConfigEditable]
        [SkinColor]
        public string TextColor { get; set; } = "#FFFFFFFF";

        [ConfigEditable]
        [SkinColor]
        public string PlaceholderColor { get; set; } = "#B8B8B8FF";

        [ConfigEditable]
        [SkinColor]
        public string CursorColor { get; set; } = "#FFFFFFFF";
    }

    public sealed class SkinV2DownloadingButtonConfig
    {
        [Range(1, 8192)]
        public float Height { get; set; } = 34;

        [Range(1, 8192)]
        public float OwnedMapsetsWidth { get; set; } = 94;

        [Range(1, 8192)]
        public float OwnedPlaylistsWidth { get; set; } = 108;

        [Range(1, 8192)]
        public float MapsetsTabWidth { get; set; } = 72;

        [Range(1, 8192)]
        public float PlaylistsTabWidth { get; set; } = 74;

        [Range(1, 8192)]
        public float KeymodeWidth { get; set; } = 140;

        [Range(1, 8192)]
        public float RankedWidth { get; set; } = 140;

        [Range(1, 8192)]
        public float StaticSelectorWidth { get; set; } = 140;

        [Range(1, 8192)]
        public float SortWidth { get; set; } = 180;

        [Range(1, 8192)]
        public float ExpandWidth { get; set; } = 34;

        [Range(1, 8192)]
        public float IconSize { get; set; } = 17;

        [Range(0, 2048)]
        public float HorizontalPadding { get; set; } = SkinV2Spacing.SpacingXs;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [SkinFont]
        public string Font { get; set; } = SkinV2FontWeightsConfig.SemiBold;

        [Range(1, 256)]
        public int FontSize { get; set; } = SkinV2FontSizesConfig.TextSm;

        [ConfigEditable]
        [SkinColor]
        public string BackgroundColor { get; set; } = "#555555FF";

        [ConfigEditable]
        [SkinColor]
        public string ActiveColor { get; set; } = "#A7A7A7FF";

        [ConfigEditable]
        [SkinColor]
        public string TextColor { get; set; } = "#FFFFFFFF";

        [ConfigEditable]
        [SkinColor]
        public string ActiveTextColor { get; set; } = "#FFFFFFFF";
    }

    public sealed class SkinV2DownloadingDropdownConfig
    {
        [Range(0, 2048)]
        public float MenuGap { get; set; } = SkinV2MarginsConfig.Sm;

        [Range(0, 2048)]
        public float MenuPadding { get; set; } = SkinV2MarginsConfig.Sm;

        [Range(0, 2048)]
        public float ItemSpacing { get; set; } = 2;

        [Range(1, 8192)]
        public float ItemHeight { get; set; } = 32;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [ConfigEditable]
        [SkinColor]
        public string MenuColor { get; set; } = "#454545FF";

        [ConfigEditable]
        [SkinColor]
        public string ItemColor { get; set; } = "#555555FF";

        [ConfigEditable]
        [SkinColor]
        public string SelectedItemColor { get; set; } = "#737373FF";
    }

    public sealed class SkinV2DownloadingRangeConfig
    {
        [Range(1, 8192)]
        public float Width { get; set; } = 210;

        [Range(1, 8192)]
        public float TrackHeight { get; set; } = 20;

        [Range(1, 8192)]
        public float ThumbWidth { get; set; } = 13;

        [Range(1, 8192)]
        public float ThumbHeight { get; set; } = 28;

        [Range(0, 4096)]
        public float TrackCornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [Range(0, 4096)]
        public float ThumbCornerRadius { get; set; } = 4;

        [ConfigEditable]
        [SkinColor]
        public string TrackColor { get; set; } = "#B7B7B7FF";

        [ConfigEditable]
        [SkinColor]
        public string SelectedTrackColor { get; set; } = "#D0D0D0FF";

        [ConfigEditable]
        [SkinColor]
        public string ThumbColor { get; set; } = "#FFFFFFFF";
    }
}
