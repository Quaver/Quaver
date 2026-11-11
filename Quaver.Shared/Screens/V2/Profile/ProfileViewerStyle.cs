using Quaver.Shared.Assets;

namespace Quaver.Shared.Screens.V2.Profile
{
    internal sealed class ProfileViewerStyle
    {
        public static ProfileViewerStyle Default { get; } = new ProfileViewerStyle();

        public float Width { get; } = 526;
        public float Height { get; } = 146;
        public float IdentityHeight { get; } = 100;
        public float PanelGap { get; } = 5;
        public float AnchorGap { get; } = 5;
        public float StatsHeight { get; } = 41;
        public float ContentPadding { get; } = 10;
        public float AvatarSize { get; } = 80;
        public float FlagSize { get; } = 22;
        public float IdentitySpacing { get; } = 5;
        public float InfoGap { get; } = 2;
        public float IdentityRowHeight { get; } = 22;
        public float StatusHeight { get; } = 20;
        public float RoleHeight { get; } = 25;
        public float ActionsWidth { get; } = 190;
        public float ActionButtonSize { get; } = 30;
        public float ActionIconSize { get; } = 26;
        public float ActionSpacing { get; } = 10;
        public float StatHeight { get; } = 30;
        public float StatIconHeight { get; } = CapsuleIcons.IconHeight;
        public float StatContentGap { get; } = 5;
        public float StatHorizontalPadding { get; } = 10;
        public float ModeWidth { get; } = 70;
        public float ModeHeight { get; } = 20;
        public float ModeSelectionWidth { get; } = 40;
        public float ModeSelectionHeight { get; } = 16;
        public float ModeSelectionInset { get; } = 2;
        public float CornerRadius { get; } = 6;
        public float ProfileCoverBrightness { get; } = 0.55f;
        public float AccentSurfaceOpacity { get; } = 0.15f;
        public float AccentButtonLightenAmount { get; } = 0.25f;
        public int AnimationDurationMilliseconds { get; } = 160;
        public string IdentityBackgroundColor { get; } = "#273038FF";
        public string StatsBackgroundColor { get; } = "#273038FF";
        public string ActionButtonColor { get; } = "#4B5973FF";
        public string SelectedActionButtonColor { get; } = "#25C88CFF";
        public string FriendedActionButtonColor { get; } = "#FF326CFF";
        public string DisabledActionButtonColor { get; } = "#181E25FF";
        public string StatPillColor { get; } = "#181E25FF";
        public string ModeBackgroundColor { get; } = "#181E25FF";
        public string ModeSelectionColor { get; } = "#D9E3F4FF";
        public string TextColor { get; } = "#FFFFFFFF";
        public string SecondaryTextColor { get; } = "#D0DBEDFF";
        public string PrimaryFont { get; } = Fonts.InterBold;
        public string SecondaryFont { get; } = Fonts.InterSemiBold;
        public int UsernameFontSize { get; } = 18;
        public int StatusFontSize { get; } = 16;
        public int StatFontSize { get; } = 14;
        public int ModeFontSize { get; } = 12;

        private ProfileViewerStyle()
        {
        }
    }
}
