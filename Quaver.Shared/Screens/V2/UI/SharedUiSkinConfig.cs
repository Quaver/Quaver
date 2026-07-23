using System.ComponentModel.DataAnnotations;
using Quaver.Shared.Assets;
using Quaver.Shared.Skinning.V2;
using Wobble.Configuration;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     Skin configuration owned by the persistent V2 navigation and account UI.
    /// </summary>
    public sealed class SkinV2NavigationConfig
    {
        /// <summary>
        ///     Shared inset around navbar buttons. This controls horizontal edge padding and,
        ///     together with the configured button size, the vertical bar padding.
        /// </summary>
        [ConfigEditable]
        [Range(0, 2048)]
        public float EdgePadding { get; set; } = SkinV2Spacing.SpacingXs;

        [ConfigEditable]
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
        [ConfigEditable]
        public SkinV2ProfileConfig Profile { get; set; } = new SkinV2ProfileConfig();

        [Required]
        [ConfigEditable]
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
        public float Size { get; set; } = 46;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [Range(1, 8192)]
        public float IconWidth { get; set; } = 30;

        [Range(1, 8192)]
        public float IconHeight { get; set; } = 25;

        [SkinColor]
        public string BackgroundColor { get; set; } = "#1F88FF26";

        [SkinColor]
        public string ForegroundColor { get; set; } = "#D9E3F4";
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
        public string TextColor { get; set; } = "#FFFFFFFF";

        [SkinColor]
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

        [Range(1, 8192)]
        public int IconCellSize { get; set; } = 40;

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
