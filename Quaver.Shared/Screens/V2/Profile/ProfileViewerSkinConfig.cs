using System.ComponentModel.DataAnnotations;
using Quaver.Shared.Assets;
using Quaver.Shared.Skinning.V2;
using Wobble.Configuration;

namespace Quaver.Shared.Screens.V2.Profile
{
    public sealed class SkinV2ProfileViewerConfig
    {
        [Range(1, 8192)]
        public float Width { get; set; } = 526;

        [Range(1, 8192)]
        public float Height { get; set; } = 146;

        [Range(1, 8192)]
        public float IdentityHeight { get; set; } = 100;

        [Range(0, 2048)]
        public float PanelGap { get; set; } = SkinV2MarginsConfig.Sm;

        [Range(1, 8192)]
        public float StatsHeight { get; set; } = 41;

        [Range(0, 2048)]
        public float ContentPadding { get; set; } = SkinV2MarginsConfig.Md;

        [Range(1, 8192)]
        public float AvatarSize { get; set; } = 80;

        [Range(1, 8192)]
        public float FlagSize { get; set; } = 22;

        [Range(0, 2048)]
        public float IdentitySpacing { get; set; } = SkinV2MarginsConfig.Sm;

        [Range(0, 2048)]
        public float InfoGap { get; set; } = 2;

        [Range(1, 8192)]
        public float IdentityRowHeight { get; set; } = 22;

        [Range(1, 8192)]
        public float StatusHeight { get; set; } = 20;

        [Range(1, 8192)]
        public float RoleHeight { get; set; } = 25;

        [Range(1, 8192)]
        public float ActionsWidth { get; set; } = 190;

        [Range(1, 8192)]
        public float ActionButtonSize { get; set; } = 30;

        [Range(1, 8192)]
        public float ActionIconSize { get; set; } = 26;

        [Range(0, 2048)]
        public float ActionSpacing { get; set; } = SkinV2MarginsConfig.Md;

        [Range(1, 8192)]
        public float StatHeight { get; set; } = 30;

        [Range(1, 8192)]
        public float StatIconHeight { get; set; } = CapsuleIcons.IconHeight;

        [Range(0, 128)]
        public float StatContentGap { get; set; } = 5;

        [Range(0, 128)]
        public float StatHorizontalPadding { get; set; } = SkinV2MarginsConfig.Md;

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

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [Range(0d, 1d)]
        public float DimOpacity { get; set; } = 0.85f;

        [Range(0d, 1d)]
        public float ProfileCoverBrightness { get; set; } = 0.55f;

        [Range(0d, 1d)]
        public float AccentSurfaceOpacity { get; set; } = 0.15f;

        [Range(0d, 1d)]
        public float AccentButtonLightenAmount { get; set; } = 0.25f;

        [Range(1, 10000)]
        public int AnimationDurationMilliseconds { get; set; } = 160;

        [ConfigEditable]
        [SkinAssetPath]
        public string FallbackCover { get; set; } = "";

        [ConfigEditable]
        [SkinColor]
        public string IdentityBackgroundColor { get; set; } = "#273038FF";

        [ConfigEditable]
        [SkinColor]
        public string StatsBackgroundColor { get; set; } = "#273038FF";

        [ConfigEditable]
        [SkinColor]
        public string ActionButtonColor { get; set; } = "#4B5973FF";

        [ConfigEditable]
        [SkinColor]
        public string SelectedActionButtonColor { get; set; } = "#25C88CFF";

        [ConfigEditable]
        [SkinColor]
        public string FriendedActionButtonColor { get; set; } = "#FF326CFF";

        [ConfigEditable]
        [SkinColor]
        public string DisabledActionButtonColor { get; set; } = "#181E25FF";

        [ConfigEditable]
        [SkinColor]
        public string StatPillColor { get; set; } = "#181E25FF";

        [ConfigEditable]
        [SkinColor]
        public string ModeBackgroundColor { get; set; } = "#181E25FF";

        [ConfigEditable]
        [SkinColor]
        public string ModeSelectionColor { get; set; } = "#D9E3F4FF";

        [ConfigEditable]
        [SkinColor]
        public string TextColor { get; set; } = "#FFFFFFFF";

        [ConfigEditable]
        [SkinColor]
        public string SecondaryTextColor { get; set; } = "#D0DBEDFF";

        [SkinFont]
        public string PrimaryFont { get; set; } = SkinV2FontWeightsConfig.Bold;

        [SkinFont]
        public string SecondaryFont { get; set; } = SkinV2FontWeightsConfig.SemiBold;

        [Range(1, 256)]
        public int UsernameFontSize { get; set; } = SkinV2FontSizesConfig.TextLg;

        [Range(1, 256)]
        public int StatusFontSize { get; set; } = SkinV2FontSizesConfig.TextBase;

        [Range(1, 256)]
        public int StatFontSize { get; set; } = SkinV2FontSizesConfig.TextSm;

        [Range(1, 256)]
        public int ModeFontSize { get; set; } = SkinV2FontSizesConfig.TextXs;
    }
}
