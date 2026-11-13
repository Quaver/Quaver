using Quaver.Shared.Assets;

namespace Quaver.Shared.Screens.V2.Profile
{
    internal sealed class ProfileViewerStyle
    {
        public static ProfileViewerStyle Default { get; } = new ProfileViewerStyle();

        public float Width { get; } = 580;
        public float Height { get; } = 131;
        public float IdentityHeight { get; } = 81;
        public float PanelGap { get; } = 0;
        public float AnchorGap { get; } = 5;
        public float StatsHeight { get; } = 50;
        public float ContentPadding { get; } = 15;
        public float AvatarSize => IdentityContentHeight;
        public float FlagSize { get; } = 22;
        public float IdentitySpacing { get; } = 5;
        public float InfoGap { get; } = 2;
        public float IdentityRowHeight { get; } = 22;
        public float StatusHeight { get; } = 20;
        public float RoleHeight { get; } = 25;
        public float IdentityContentHeight => IdentityRowHeight + StatusHeight + RoleHeight + InfoGap * 2;
        public float ActionsWidth { get; } = 150;
        public float ActionButtonSize { get; } = 30;
        public float ActionIconSize { get; } = 26;
        public float ActionSpacing { get; } = 10;
        public float StatHeight { get; } = 30;
        public float StatIconHeight { get; } = CapsuleIcons.IconHeight;
        public float StatContentGap { get; } = 5;
        public float StatHorizontalPadding { get; } = 10;
        public float StatSpacing { get; } = 10;
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
        public string MutualFriendActionButtonColor { get; } = "#D55DEAFF";
        public string DisabledActionButtonColor { get; } = "#181E25FF";
        public string StatPillColor { get; } = "#181E25FF";
        public string TextColor { get; } = "#FFFFFFFF";
        public string SecondaryTextColor { get; } = "#D0DBEDFF";
        public string PrimaryFont { get; } = Fonts.InterBold;
        public string SecondaryFont { get; } = Fonts.InterSemiBold;
        public int UsernameFontSize { get; } = 18;
        public int StatusFontSize { get; } = 16;
        public int StatFontSize { get; } = 14;

        private ProfileViewerStyle()
        {
        }
    }
}
