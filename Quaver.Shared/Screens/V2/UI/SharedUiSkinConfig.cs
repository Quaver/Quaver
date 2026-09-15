using System.ComponentModel.DataAnnotations;
using Quaver.Shared.Assets;
using Quaver.Shared.Skinning.V2;
using Wobble.Configuration;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     Shared visual and interaction defaults for V2 dropdown selectors.
    /// </summary>
    public sealed class SkinV2DropdownConfig
    {
        [Range(1, 8192)]
        public float Height { get; set; } = 34;

        [Range(0, 2048)]
        public float HorizontalPadding { get; set; } = SkinV2Spacing.SpacingXs;

        [Range(0, 2048)]
        public float MenuGap { get; set; } = 0;

        [Range(0, 2048)]
        public float MenuPadding { get; set; } = SkinV2MarginsConfig.Sm;

        [Range(0, 2048)]
        public float ItemSpacing { get; set; } = 0;

        [Range(1, 8192)]
        public float ItemHeight { get; set; } = 32;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [Range(1, 8192)]
        public float IconSize { get; set; } = 17;

        [Range(1, 8192)]
        public float ChevronSize { get; set; } = 18;

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
        public string ItemColor { get; set; } = "#0F2C44FF";

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
        public string ScrollbarColor { get; set; } = "#FFFFFF80";
    }

    /// <summary>
    ///     Shared visual defaults for V2 on/off toggles.
    /// </summary>
    public sealed class SkinV2ToggleConfig
    {
        [Range(1, 8192)]
        public float Width { get; set; } = 58;

        [Range(1, 8192)]
        public float Height { get; set; } = 20;

        /// <summary>
        ///     The space between the edge of the toggle and the inner pill, at the top and bottom.
        /// </summary>
        [Range(0, 2048)]
        public float InnerInset { get; set; } = 2.5f;

        /// <summary>
        ///     The width of the inner pill. It is the same for "On" and "Off", so the pill does not change shape.
        /// </summary>
        [Range(1, 8192)]
        public float InnerWidth { get; set; } = 31.49f;

        [Range(1, 256)]
        public int FontSize { get; set; } = 11;

        /// <summary>
        ///     The track colors while off, from left (under the inner pill) to right.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string TrackOffStartColor { get; set; } = "#FF3A6FFF";

        [ConfigEditable]
        [SkinColor]
        public string TrackOffEndColor { get; set; } = "#273038FF";

        /// <summary>
        ///     The track colors while on, from left to right (under the inner pill).
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string TrackOnStartColor { get; set; } = "#273038FF";

        [ConfigEditable]
        [SkinColor]
        public string TrackOnEndColor { get; set; } = "#25C88CFF";

        /// <summary>
        ///     The inner pill's color. It stays the same; the "On" and "Off" text changes color instead.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string InnerPillColor { get; set; } = "#FFFFFFFF";
    }

    /// <summary>
    ///     Shared look of V2 buttons. The size is set by the code that creates the button.
    /// </summary>
    public sealed class SkinV2ButtonConfig
    {
        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [Range(1, 256)]
        public int FontSize { get; set; } = 18;

        [ConfigEditable]
        [SkinColor]
        public string BackgroundColor { get; set; } = "#6B83B2FF";

        [ConfigEditable]
        [SkinColor]
        public string TextColor { get; set; } = "#FFFFFFFF";
    }

    /// <summary>
    ///     Shared look of V2 sliders and their value box.
    /// </summary>
    public sealed class SkinV2SliderConfig
    {
        [Range(1, 8192)]
        public float TrackWidth { get; set; } = 222;

        [Range(1, 8192)]
        public float TrackHeight { get; set; } = 22;

        /// <summary>
        ///     The gap between a slider's bar and its value badge.
        /// </summary>
        [Range(0, 2048)]
        public float ValueGap { get; set; } = SkinV2Spacing.Spacing2Xs;

        [Range(1, 8192)]
        public float ValueWidth { get; set; } = 73;

        [Range(1, 8192)]
        public float ValueHeight { get; set; } = 30;

        [Range(0, 2048)]
        public float ValueTextPadding { get; set; } = SkinV2MarginsConfig.Sm;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [Range(1, 256)]
        public int FontSize { get; set; } = 18;

        /// <summary>
        ///     The slider bar behind the fill.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string BackgroundColor { get; set; } = "#273038FF";

        /// <summary>
        ///     The fill that grows from the left to show the value.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string BarColor { get; set; } = "#6B83B2FF";

        /// <summary>
        ///     The background of the value box.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string ValueColor { get; set; } = "#273038FF";

        /// <summary>
        ///     The text in the value box, both when shown and while typing.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string ValueTextColor { get; set; } = "#6B83B2FF";
    }

    /// <summary>
    ///     Shared look of the V2 search bar: the text box, and the clear button and result text shown
    ///     while a search is running.
    /// </summary>
    public sealed class SkinV2SearchConfig
    {
        [ConfigEditable]
        [SkinColor]
        public string BackgroundColor { get; set; } = "#181E25FF";

        [ConfigEditable]
        [SkinColor]
        public string TextColor { get; set; } = "#FFFFFFFF";

        [ConfigEditable]
        [SkinColor]
        public string PlaceholderColor { get; set; } = "#4B5973FF";

        [SkinColor]
        public string CursorColor { get; set; } = "#FFFFFFFF";

        [SkinColor]
        public string IconColor { get; set; } = "#EBF3FFFF";

        [SkinFont]
        public string Font { get; set; } = SkinV2FontWeightsConfig.SemiBold;

        [Range(1, 256)]
        public int FontSize { get; set; } = SkinV2FontSizesConfig.TextLg;

        [Range(1, 8192)]
        public float IconSize { get; set; } = SkinV2Spacing.Spacing3Xl;

        [Range(0, 2048)]
        public float HorizontalPadding { get; set; } = SkinV2Spacing.Spacing2Xs;

        [Range(0, 2048)]
        public float TextLeftInset { get; set; } = 44;

        [Range(0, 2048)]
        public float ResultRightInset { get; set; } = SkinV2Spacing.Spacing2Xs;

        [Range(1, 8192)]
        public float ResultWidth { get; set; } = 180;

        [ConfigEditable]
        [SkinColor]
        public string ResultTextColor { get; set; } = "#8A8A8AFF";

        /// <summary>
        ///     How long to wait after the last key press before searching, so a word runs one search
        ///     instead of one per letter.
        /// </summary>
        [Range(0, 5000)]
        public int DebounceMilliseconds { get; set; } = 400;

        /// <summary>
        ///     The round clear button, shown while a search is running.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string ClearButtonColor { get; set; } = "#D9E3F4FF";

        [ConfigEditable]
        [SkinColor]
        public string ClearIconColor { get; set; } = "#181E25FF";

        [Range(1, 8192)]
        public float ClearButtonSize { get; set; } = 20;

        [Range(1, 8192)]
        public float ClearIconSize { get; set; } = 8;

        /// <summary>
        ///     The line between the clear button and the result text.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string SeparatorColor { get; set; } = "#D9E3F480";

        [Range(1, 2048)]
        public float SeparatorWidth { get; set; } = 1;

        [Range(1, 2048)]
        public float SeparatorHeight { get; set; } = 20;

        [Range(0, 2048)]
        public float SeparatorGap { get; set; } = SkinV2Spacing.Spacing2Xs;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;
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
        public string OfflineStatusColor { get; set; } = "#828E99FF";
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
