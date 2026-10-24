using System.ComponentModel.DataAnnotations;
using Quaver.Shared.Assets;
using Quaver.Shared.Skinning.V2;
using Wobble.Configuration;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     Shared visual and layout defaults for the music controls hosted by the V2 footer.
    /// </summary>
    public sealed class SkinV2FooterMusicPlayerConfig
    {
        public bool ShowBanner { get; set; } = true;

        [Range(1, 8192)]
        public float MinimumWidth { get; set; } = 320;

        [Range(1, 8192)]
        [SkinEditorIgnore]
        public float ControlsHeight { get; set; } = 25;

        [Range(1, 8192)]
        public float BannerWidth { get; set; } = 580;

        [Range(1, 8192)]
        public float BannerHeight { get; set; } = 55;

        [Range(0, 2048)]
        [SkinEditorIgnore]
        public float BannerTextHorizontalPadding { get; set; } = SkinV2MarginsConfig.Md;

        [Range(0, 2048)]
        [SkinEditorIgnore]
        public float BannerTextGap { get; set; } = 0;

        [Range(0d, 1d)]
        public float BannerBrightness { get; set; } = 0.2f;

        [Range(0, 2048)]
        [SkinEditorIgnore]
        public float Padding { get; set; } = SkinV2MarginsConfig.Md;

        [Range(0, 2048)]
        [SkinEditorIgnore]
        public float SectionGap { get; set; } = SkinV2MarginsConfig.Sm;

        [Range(0, 2048)]
        [SkinEditorIgnore]
        public float ControlGap { get; set; } = SkinV2MarginsConfig.Lg;

        [Range(1, 8192)]
        [SkinEditorIgnore]
        public float ButtonSize { get; set; } = 20;

        [Range(1, 8192)]
        [SkinEditorIgnore]
        public float PreviousNextIconWidth { get; set; } = 16;

        [Range(1, 8192)]
        [SkinEditorIgnore]
        public float PreviousNextIconHeight { get; set; } = 18;

        [Range(1, 8192)]
        [SkinEditorIgnore]
        public float PlayPauseIconWidth { get; set; } = 20;

        [Range(1, 8192)]
        [SkinEditorIgnore]
        public float PlayPauseIconHeight { get; set; } = 20;

        [Range(1, 8192)]
        [SkinEditorIgnore]
        public float ListIconWidth { get; set; } = 21;

        [Range(1, 8192)]
        [SkinEditorIgnore]
        public float ListIconHeight { get; set; } = 20;

        [Range(1, 8192)]
        [SkinEditorIgnore]
        public float VolumeIconWidth { get; set; } = 30;

        [Range(1, 8192)]
        [SkinEditorIgnore]
        public float VolumeIconHeight { get; set; } = 30;

        [Range(1, 8192)]
        [SkinEditorIgnore]
        public float ProgressWidth { get; set; } = 265;

        [Range(1, 8192)]
        [SkinEditorIgnore]
        public float ProgressHeight { get; set; } = 12;

        [Range(1, 8192)]
        [SkinEditorIgnore]
        public float TimeWidth { get; set; } = 110;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [Range(0, 4096)]
        public float ProgressCornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [SkinFont]
        [SkinEditorIgnore]
        public string TitleFont { get; set; } = SkinV2FontWeightsConfig.Bold;

        [Range(1, 256)]
        public int TitleFontSize { get; set; } = SkinV2FontSizesConfig.TextBase;

        [SkinFont]
        [SkinEditorIgnore]
        public string ArtistFont { get; set; } = SkinV2FontWeightsConfig.SemiBold;

        [Range(1, 256)]
        public int ArtistFontSize { get; set; } = SkinV2FontSizesConfig.TextSm;

        [SkinFont]
        [SkinEditorIgnore]
        public string TimeFont { get; set; } = SkinV2FontWeightsConfig.SemiBold;

        [Range(1, 256)]
        [SkinEditorIgnore]
        public int TimeFontSize { get; set; } = SkinV2FontSizesConfig.TextSm;

        [ConfigEditable]
        [SkinColor]
        public string BackgroundColor { get; set; } = "#273038FF";

        [ConfigEditable]
        [SkinColor]
        public string ForegroundColor { get; set; } = "#FFFFFFFF";

        [ConfigEditable]
        [SkinColor]
        public string SecondaryForegroundColor { get; set; } = "#D0DBEDFF";

        [ConfigEditable]
        [SkinColor]
        public string TitleColor { get; set; } = "#FFFFFFFF";

        [ConfigEditable]
        [SkinColor]
        public string ArtistColor { get; set; } = "#D0DBEDFF";

        [ConfigEditable]
        [SkinColor]
        public string ProgressTrackColor { get; set; } = "#181E25FF";

        [ConfigEditable]
        [SkinColor]
        public string ProgressFillColor { get; set; } = "#6B83B2FF";

        [Required]
        public SkinV2MusicPlayerIconsConfig Icons { get; set; } = new SkinV2MusicPlayerIconsConfig();

        [Required]
        public SkinV2VolumeDialogConfig VolumeDialog { get; set; } = new SkinV2VolumeDialogConfig();

        [Required]
        public SkinV2MusicQueueDialogConfig MusicQueueDialog { get; set; } =
            new SkinV2MusicQueueDialogConfig();
    }

    public sealed class SkinV2MusicQueueDialogConfig
    {
        [Range(520, 1000)]
        public float Width { get; set; } = 620;

        [Range(300, 1000)]
        public float Height { get; set; } = 762;

        [Range(0, 20)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [ConfigEditable]
        [SkinColor]
        public string PanelColor { get; set; } = "#273038FF";

        [Required]
        public SkinV2MusicQueueSearchConfig Search { get; set; } =
            new SkinV2MusicQueueSearchConfig();

        [Required]
        public SkinV2MusicQueueListConfig List { get; set; } =
            new SkinV2MusicQueueListConfig();

        [Required]
        public SkinV2MusicQueueRowConfig Row { get; set; } =
            new SkinV2MusicQueueRowConfig();
    }

    public sealed class SkinV2MusicQueueSearchConfig
    {
        [Range(1, 8192)]
        [SkinEditorIgnore]
        public float Height { get; set; } = 40;

        [Range(1, 8192)]
        [SkinEditorIgnore]
        public float IconSize { get; set; } = SkinV2FontSizesConfig.TextXl;

        [Range(0, 2048)]
        [SkinEditorIgnore]
        public float IconInset { get; set; } = SkinV2MarginsConfig.Lg;

        [Range(0, 20)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [SkinFont]
        [SkinEditorIgnore]
        public string Font { get; set; } = SkinV2FontWeightsConfig.Bold;

        [Range(1, 256)]
        [SkinEditorIgnore]
        public int FontSize { get; set; } = SkinV2FontSizesConfig.TextXl;

        [ConfigEditable]
        [SkinColor]
        public string BackgroundColor { get; set; } = "#181E25FF";

        [ConfigEditable]
        [SkinColor]
        public string TextColor { get; set; } = "#FFFFFFFF";

        [ConfigEditable]
        [SkinColor]
        public string PlaceholderColor { get; set; } = "#6B83B2FF";

        [ConfigEditable]
        [SkinColor]
        public string CursorColor { get; set; } = "#FFFFFFFF";
    }

    public sealed class SkinV2MusicQueueListConfig
    {
        [Range(1, 8192)]
        [SkinEditorIgnore]
        public float HeaderHeight { get; set; } = 52;

        [Range(0, 2048)]
        [SkinEditorIgnore]
        public float ContentPadding { get; set; } = SkinV2MarginsConfig.Md;

        [Range(0, 2048)]
        [SkinEditorIgnore]
        public float HeaderGap { get; set; } = SkinV2MarginsConfig.Md;

        [Range(0, 2048)]
        [SkinEditorIgnore]
        public float RowGap { get; set; } = SkinV2MarginsConfig.Md;

        [SkinFont]
        [SkinEditorIgnore]
        public string Font { get; set; } = SkinV2FontWeightsConfig.Bold;

        [Range(1, 256)]
        [SkinEditorIgnore]
        public int FontSize { get; set; } = SkinV2FontSizesConfig.TextXl;

        [ConfigEditable]
        [SkinColor]
        public string TextColor { get; set; } = "#FFFFFFFF";
    }

    public sealed class SkinV2MusicQueueRowConfig
    {
        [Range(1, 8192)]
        [SkinEditorIgnore]
        public float Height { get; set; } = 40;

        [Range(0, 2048)]
        [SkinEditorIgnore]
        public float HorizontalPadding { get; set; } = SkinV2MarginsConfig.Lg;

        [Range(0, 20)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [Range(0.3, 0.6)]
        public float ArtistColumnStart { get; set; } = 0.48f;

        [Range(0.7, 0.9)]
        public float LengthColumnStart { get; set; } = 0.82f;

        [SkinFont]
        [SkinEditorIgnore]
        public string Font { get; set; } = SkinV2FontWeightsConfig.Bold;

        [Range(1, 256)]
        [SkinEditorIgnore]
        public int FontSize { get; set; } = SkinV2FontSizesConfig.TextXl;

        [ConfigEditable]
        [SkinColor]
        public string BackgroundColor { get; set; } = "#181E25FF";

        [ConfigEditable]
        [SkinColor]
        public string SelectedColor { get; set; } = "#6B83B2FF";

        [ConfigEditable]
        [SkinColor]
        public string TextColor { get; set; } = "#FFFFFFFF";

        [ConfigEditable]
        [SkinColor]
        public string LengthColor { get; set; } = "#D0DBEDFF";
    }

    public sealed class SkinV2VolumeDialogConfig
    {
        [Range(-1920, 1920)]
        public float PositionOffsetX { get; set; } = 0;

        [Range(-1080, 1080)]
        public float PositionOffsetY { get; set; } = 0;

        [Range(40, 80)]
        public float CardWidth { get; set; } = 44;

        [Range(240, 360)]
        public float CardHeight { get; set; } = 245;

        [Range(5, 30)]
        public float CardGap { get; set; } = SkinV2MarginsConfig.Md;

        [Range(30, 50)]
        public float FooterHeight { get; set; } = 40;

        [Range(8, 20)]
        public float TrackWidth { get; set; } = 12;

        [Range(120, 180)]
        public float TrackHeight { get; set; } = 170;

        [Range(0, 2048)]
        [SkinEditorIgnore]
        public float TrackTopInset { get; set; } = SkinV2MarginsConfig.Md;

        [Range(1, 8192)]
        [SkinEditorIgnore]
        public float IconHeight { get; set; } = 20;

        [Range(0, 20)]
        public float CardCornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [Range(0, 10)]
        public float TrackCornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [Range(0, 2048)]
        [SkinEditorIgnore]
        public float AnchorGap { get; set; } = SkinV2MarginsConfig.Md;

        [Range(-2048, 2048)]
        [SkinEditorIgnore]
        public float HorizontalOffset { get; set; } = SkinV2MarginsConfig.Md;

        [Range(0, 2048)]
        [SkinEditorIgnore]
        public float WindowInset { get; set; } = SkinV2MarginsConfig.Md;

        public bool SlideFromLeft { get; set; } = true;

        [Range(0, 200)]
        public float SlideDistance { get; set; } = 40;

        [Range(50, 1000)]
        public int AnimationDuration { get; set; } = 250;

        [Range(0d, 1d)]
        [SkinEditorIgnore]
        public float DimOpacity { get; set; } = 0.75f;

        [ConfigEditable]
        [SkinColor]
        public string CardColor { get; set; } = "#181E25FF";

        [ConfigEditable]
        [SkinColor]
        public string FooterColor { get; set; } = "#273038FF";

        [ConfigEditable]
        [SkinColor]
        public string TrackColor { get; set; } = "#273038FF";

        [ConfigEditable]
        [SkinColor]
        public string InactiveFillColor { get; set; } = "#77808FFF";

        [ConfigEditable]
        [SkinColor]
        public string SelectedColor { get; set; } = "#6B83B2FF";

        [ConfigEditable]
        [SkinColor]
        public string IconColor { get; set; } = "#FFFFFFFF";
    }

    public sealed class SkinV2MusicPlayerIconsConfig
    {
        [SkinAssetPath]
        public string Previous { get; set; } = "";

        [SkinAssetPath]
        public string Play { get; set; } = "";

        [SkinAssetPath]
        public string Pause { get; set; } = "";

        [SkinAssetPath]
        public string Next { get; set; } = "";

        [SkinAssetPath]
        public string Queue { get; set; } = "";

        [SkinAssetPath]
        public string Volume { get; set; } = "";
    }

    /// <summary>
    ///     Shared visual and layout defaults for V2 modal dialogs.
    /// </summary>
    public sealed class SkinV2ModalConfig
    {
        [Range(1, 8192)]
        public float Width { get; set; } = 660;

        [Range(1, 8192)]
        public float TitleHeight { get; set; } = 36;

        [Range(0, 2048)]
        public float TitleHorizontalPadding { get; set; } = 40;

        [Range(1, 8192)]
        public float BannerHeight { get; set; } = 128;

        [Range(0, 2048)]
        public float SectionGap { get; set; } = SkinV2MarginsConfig.Md;

        [Range(0, 2048)]
        public float FormInset { get; set; } = SkinV2MarginsConfig.Md;

        [Range(1, 8192)]
        public float FormWidth { get; set; } = 620;

        [Range(0, 2048)]
        public float FormItemGap { get; set; } = SkinV2MarginsConfig.Md;

        [Range(1, 8192)]
        public float FieldLabelHeight { get; set; } = 34;

        [Range(0, 2048)]
        public float FieldLabelHorizontalPadding { get; set; } = 32;

        [Range(1, 8192)]
        public float InputHeight { get; set; } = 40;

        [Range(1, 8192)]
        public float ButtonWidth { get; set; } = 180;

        [Range(1, 8192)]
        public float ButtonHeight { get; set; } = 40;

        [Range(0, 2048)]
        public float ButtonGap { get; set; } = SkinV2MarginsConfig.Md;

        [Range(0, 2048)]
        public float ButtonBottomInset { get; set; } = SkinV2Spacing.SpacingXl;

        [Range(0, 2048)]
        public float FormToButtonsGap { get; set; } = 31;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [Range(1, 10000)]
        public int AnimationDurationMilliseconds { get; set; } = 150;

        [SkinFont]
        public string Font { get; set; } = SkinV2FontWeightsConfig.Bold;

        [Range(1, 256)]
        public int TitleFontSize { get; set; } = SkinV2FontSizesConfig.TextXl;

        [Range(1, 256)]
        public int DescriptionFontSize { get; set; } = SkinV2FontSizesConfig.TextBase;

        [Range(1, 256)]
        public int ControlFontSize { get; set; } = SkinV2FontSizesConfig.TextLg;

        [ConfigEditable, SkinColor]
        public string TitleColor { get; set; } = "#4B5973FF";

        [ConfigEditable, SkinColor]
        public string PanelColor { get; set; } = "#273038FF";

        [ConfigEditable, SkinColor]
        public string InputColor { get; set; } = "#181E25FF";

        [ConfigEditable, SkinColor]
        public string TextColor { get; set; } = "#FFFFFFFF";

        [ConfigEditable, SkinColor]
        public string BannerShadeColor { get; set; } = "#00000073";

        [ConfigEditable, SkinColor]
        public string ConfirmButtonColor { get; set; } = "#27B06EFF";

        [ConfigEditable, SkinColor]
        public string CancelButtonColor { get; set; } = "#F9645DFF";

        [ConfigEditable, SkinColor]
        public string NeutralButtonColor { get; set; } = "#4B5973FF";

        [SkinAssetPath]
        public string BannerImage { get; set; } = "";
    }

    /// <summary>
    ///     Shared visual and interaction defaults for V2 dropdown selectors.
    /// </summary>
    public sealed class SkinV2DropdownConfig
    {
        public const string DefaultItemColor = "#273038FF";

        [Range(1, 8192)]
        public float Height { get; set; } = 34;

        [Range(0, 2048)]
        public float HorizontalPadding { get; set; } = SkinV2Spacing.SpacingXs;

        [Range(0, 2048)]
        public float MenuGap { get; set; } = 0;

        [Range(0, 2048)]
        public float ItemSpacing { get; set; } = 0;

        [Range(1, 8192)]
        public float ItemHeight { get; set; } = 32;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [Range(1, 8192)]
        public float IconSize { get; set; } = 17;

        [Range(1, 8192)]
        public float ChevronWidth { get; set; } = 16;

        [Range(1, 8192)]
        public float ChevronHeight { get; set; } = 10;

        [Range(0, 2048)]
        public float DividerInset { get; set; } = 0;

        [Range(0, 64)]
        public float DividerThickness { get; set; } = 1;

        [Range(1, 256)]
        public int DefaultMaxVisibleItems { get; set; } = 6;

        [Range(1, 128)]
        public float ScrollbarWidth { get; set; } = 4;

        [Range(1, 10000)]
        public int AnimationDurationMilliseconds { get; set; } = 180;

        [SkinFont]
        public string Font { get; set; } = SkinV2FontWeightsConfig.SemiBold;

        [Range(1, 256)]
        public int FontSize { get; set; } = SkinV2FontSizesConfig.TextSm;

        [ConfigEditable]
        [SkinColor]
        public string TriggerColor { get; set; } = "#061019FF";

        [ConfigEditable]
        [SkinColor]
        public string ItemColor { get; set; } = DefaultItemColor;

        [ConfigEditable]
        [SkinColor]
        public string HoverColor { get; set; } = "#174E76FF";

        [ConfigEditable]
        [SkinColor]
        public string SelectedItemColor { get; set; } = "#256EAAFF";

        [ConfigEditable]
        [SkinColor]
        public string TextColor { get; set; } = "#FFFFFFFF";

        [ConfigEditable]
        [SkinColor]
        public string SelectedTextColor { get; set; } = "#FFFFFFFF";

        [ConfigEditable]
        [SkinColor]
        public string IconColor { get; set; } = "#FFFFFFFF";

        [ConfigEditable]
        [SkinColor]
        public string DividerColor { get; set; } = "#FFFFFF80";

        [ConfigEditable]
        [SkinColor]
        public string ScrollbarColor { get; set; } = "#D9E3F4FF";

        [ConfigEditable]
        [SkinColor]
        public string ScrollbarTrackColor { get; set; } = "#181E25FF";
    }

    /// <summary>
    ///     Skin configuration owned by the persistent V2 navigation and account UI.
    /// </summary>
    public sealed class SkinV2NavigationConfig
    {
        /// <summary>
        ///     Shared inset around navbar buttons. This controls horizontal edge padding and,
        ///     together with the configured button size, the vertical bar padding.
        /// </summary>
        [Range(0, 2048)]
        public float EdgePadding { get; set; } = SkinV2Spacing.SpacingXs;

        [Range(0, 2048)]
        public float ItemSpacing { get; set; } = SkinV2Spacing.SpacingXs;

        [Required]
        [ConfigEditable]
        public SkinV2NavigationBarConfig Bar { get; set; } = new SkinV2NavigationBarConfig();

        [Required]
        [ConfigEditable]
        public SkinV2NavigationBarConfig Footer { get; set; } =
            new SkinV2NavigationBarConfig();

        [Required]
        [ConfigEditable]
        public SkinV2NavigationButtonConfig Button { get; set; } = new SkinV2NavigationButtonConfig();

        [Required]
        public SkinV2NavigationLogoConfig Logo { get; set; } = new SkinV2NavigationLogoConfig();

        [Required]
        public SkinV2ProfileConfig Profile { get; set; } = new SkinV2ProfileConfig();

        [Required]
        public SkinV2NavigationStatsConfig Stats { get; set; } = new SkinV2NavigationStatsConfig();

        [Required]
        public SkinV2AccountDropdownConfig AccountDropdown { get; set; } = new SkinV2AccountDropdownConfig();
    }

    public sealed class SkinV2NavigationBarConfig
    {
        [Required]
        [ConfigEditable]
        public SkinV2BackgroundConfig Background { get; set; } =
            new SkinV2BackgroundConfig();
    }

    public sealed class SkinV2NavigationButtonConfig
    {
        [Range(1, 8192)]
        public float Size { get; set; } = 50;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [Range(1, 8192)]
        public float IconSize { get; set; } = 40;

        [Range(0, 2048)]
        public float ExpandedLabelRightPadding { get; set; } = SkinV2MarginsConfig.Sm;

        [ConfigEditable]
        [SkinColor]
        public string BackgroundColor { get; set; } = "#1F88FF26";

        [ConfigEditable]
        [SkinColor]
        public string ForegroundColor { get; set; } = "#D9E3F4";
    }

    public sealed class SkinV2NavigationLogoConfig
    {
        [SkinAssetPath]
        public string Image { get; set; } = "";

        [Range(1, 8192)]
        public float Height { get; set; } = 40;
    }

    public sealed class SkinV2ProfileConfig
    {
        [Range(1, 8192)]
        public float Width { get; set; } = 280;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = 5;

        [Range(1, 8192)]
        public float StatusBorderSize { get; set; } = 20;

        [Range(1, 8192)]
        public float StatusDotSize { get; set; } = 14;

        [Range(-8192, 8192)]
        public float FlagX { get; set; } = 58;

        [Range(1, 8192)]
        public float FlagSize { get; set; } = 24;

        [Range(0, 2048)]
        public float TextSpacing { get; set; } = 8;

        [Range(0, 2048)]
        public float UsernameRightPadding { get; set; } = 14;

        [Range(0, 2048)]
        public float DropdownGap { get; set; } = 13;

        [SkinFont]
        public string UsernameFont { get; set; } = Fonts.InterBold;

        [Range(1, 256)]
        public int UsernameFontSize { get; set; } = 18;

        [SkinColor]
        [ConfigEditable]
        public string TextColor { get; set; } = "#FFFFFFFF";

        [SkinColor]
        [ConfigEditable]
        public string OfflineStatusColor { get; set; } = "#FF3A6FFF";
    }

    public sealed class SkinV2NavigationStatsConfig
    {
        [Range(1, 8192)]
        public float TimeWidth { get; set; } = 94;

        [Range(1, 8192)]
        public float FriendsWidth { get; set; } = 160;

        [Range(1, 8192)]
        public float ButtonHeight { get; set; } = 22;

        [Range(0, 2048)]
        public float RowGap { get; set; } = 6;

        [Range(0, 2048)]
        public float TextGap { get; set; } = 4;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = 11;

        [SkinFont]
        public string Font { get; set; } = Fonts.InterBold;

        [Range(1, 256)]
        public int FontSize { get; set; } = 16;

        [SkinColor]
        public string BackgroundColor { get; set; } = "#273038FF";

        [SkinColor]
        public string TextColor { get; set; } = "#FFFFFFFF";

        [SkinColor]
        public string CountColor { get; set; } = "#8CAFEAFF";
    }

    public sealed class SkinV2AccountDropdownConfig
    {
        [Range(1, 8192)]
        public float Width { get; set; } = 526;

        [Range(1, 8192)]
        public float ConnectedHeight { get; set; } = 145;

        [Range(1, 8192)]
        public float OfflineHeight { get; set; } = 68;

        [Range(0, 2048)]
        public float PanelGap { get; set; } = 5;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = 6;

        [Range(1, 8192)]
        public float UpperHeight { get; set; } = 100;

        [Range(1, 8192)]
        public float ProfileWidth { get; set; } = 394;

        [Range(1, 8192)]
        public float ActionsWidth { get; set; } = 132;

        [Range(1, 8192)]
        public float StatsHeight { get; set; } = 40;

        [Range(1, 8192)]
        public float AvatarSize { get; set; } = 80;

        [Range(0, 2048)]
        public float ContentPadding { get; set; } = 10;

        [Range(1, 8192)]
        public float InfoWidth { get; set; } = 284;

        [Range(1, 8192)]
        public float IdentityHeight { get; set; } = 22;

        [Range(0, 2048)]
        public float InfoGap { get; set; } = 2;

        [Range(1, 8192)]
        public float FlagSize { get; set; } = 22;

        [Range(0, 2048)]
        public float IdentitySpacing { get; set; } = 6;

        [Range(1, 8192)]
        public float StatusHeight { get; set; } = 20;

        [Range(1, 8192)]
        public float RoleHeight { get; set; } = 25;

        [Range(1, 8192)]
        public float ActionButtonSize { get; set; } = 30;

        [Range(1, 8192)]
        public float ActionIconSize { get; set; } = 20;

        [Range(0, 2048)]
        public float ActionTopSpacer { get; set; } = 60;

        [Range(0, 2048)]
        public float ActionLeftSpacer { get; set; } = 52;

        [Range(0, 2048)]
        public float ActionSpacing { get; set; } = 10;

        [Range(1, 8192)]
        public float StatHeight { get; set; } = 30;

        [Range(1, 8192)]
        public float RankWidth { get; set; } = 121;

        [Range(1, 8192)]
        public float RatingWidth { get; set; } = 95;

        [Range(1, 8192)]
        public float AccuracyWidth { get; set; } = 102;

        [Range(1, 8192)]
        public float ModeWidth { get; set; } = 70;

        [Range(1, 8192)]
        public float ModeHeight { get; set; } = 20;

        [Range(1, 8192)]
        public float ModeSelectionWidth { get; set; } = 40;

        [Range(1, 8192)]
        public float ModeSelectionHeight { get; set; } = 16;

        [Range(0, 2048)]
        public float ModeSelectionInset { get; set; } = 2;

        [Range(1, 8192)]
        public float RoleDefaultWidth { get; set; } = 150;

        [Range(0, 2048)]
        public float RolePadding { get; set; } = 22;

        [Range(1, 8192)]
        public float RoleIconSize { get; set; } = 16;

        [Range(1, 8192)]
        public float StatIconSize { get; set; } = 22;

        [Range(1, 8192)]
        public float OfflineAvatarSize { get; set; } = 48;

        [Range(1, 8192)]
        public float OfflineInfoHeight { get; set; } = 48;

        [Range(0, 2048)]
        public float OfflineAvatarSpacing { get; set; } = 12;

        [Range(1, 8192)]
        public float LoginButtonSize { get; set; } = 40;

        [Range(1, 8192)]
        public float LoginIconSize { get; set; } = 24;

        [Range(0d, 1d)]
        public float DarknessOpacity { get; set; } = 0.75f;

        [Range(0d, 1d)]
        public float ProfileCoverBrightness { get; set; } = 0.55f;

        [SkinColor]
        public string UpperPanelColor { get; set; } = "#555555FF";

        [SkinColor]
        public string ActionPanelColor { get; set; } = "#444444FF";

        [SkinColor]
        public string ActionButtonColor { get; set; } = "#999999FF";

        [SkinColor]
        public string StatsPanelColor { get; set; } = "#8D8D8DFF";

        [SkinColor]
        public string StatPillColor { get; set; } = "#CDCDCDFF";

        [SkinColor]
        public string RolePillColor { get; set; } = "#929292FF";

        [SkinColor]
        public string ModeBackgroundColor { get; set; } = "#555555FF";

        [SkinColor]
        public string TextColor { get; set; } = "#FFFFFFFF";

        [SkinFont]
        public string PrimaryFont { get; set; } = Fonts.InterBold;

        [SkinFont]
        public string SecondaryFont { get; set; } = Fonts.InterSemiBold;

        [Range(1, 256)]
        public int UsernameFontSize { get; set; } = 18;

        [Range(1, 256)]
        public int StatusFontSize { get; set; } = 16;

        [Range(1, 256)]
        public int RoleFontSize { get; set; } = 14;

        [Range(1, 256)]
        public int StatFontSize { get; set; } = 15;

        [Range(1, 256)]
        public int ModeFontSize { get; set; } = 13;

        [Range(1, 256)]
        public int OfflineTitleFontSize { get; set; } = 18;

        [Range(1, 256)]
        public int OfflineStatusFontSize { get; set; } = 14;

    }
}
