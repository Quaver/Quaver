using System.ComponentModel.DataAnnotations;
using Quaver.Shared.Assets;
using Quaver.Shared.Skinning.V2;
using Wobble.Configuration;

namespace Quaver.Shared.Graphics.Overlays.V2Hub;

/// <summary>
///     Skin configuration owned by the V2 online hub.
/// </summary>
public sealed class SkinV2HubConfig
{
    [Required]
    public SkinV2HubLayoutConfig Layout { get; set; } = new();

    [Required]
    public SkinV2HubPanelConfig Panel { get; set; } = new();

    [Required]
    public SkinV2HubTabsConfig Tabs { get; set; } = new();

    [Required]
    public SkinV2HubSectionConfig Sections { get; set; } = new();

    [Required]
    public SkinV2HubUsersConfig Users { get; set; } = new();

    [Required]
    public SkinV2HubNotificationsConfig Notifications { get; set; } = new();

    [Required]
    public SkinV2HubSongRequestsConfig SongRequests { get; set; } = new();
}

public sealed class SkinV2HubLayoutConfig
{
    [ConfigEditable]
    [Range(1, 8192)]
    public float Width { get; set; } = 600;

    [Range(0, 2048)]
    public float NavigationHeaderGap { get; set; } = SkinV2MarginsConfig.Md;

    [Range(1, 10000)]
    public int AnimationDurationMilliseconds { get; set; } = 200;
}

public sealed class SkinV2HubPanelConfig
{
    [ConfigEditable]
    [SkinColor]
    public string NavigationHeaderColor { get; set; } = "#181E25FF";

    [ConfigEditable]
    [SkinColor]
    public string HeaderColor { get; set; } = "#181E25FF";

    [ConfigEditable]
    [SkinColor]
    public string ContentColor { get; set; } = "#273038FF";
}

public sealed class SkinV2HubTabsConfig
{
    [ConfigEditable]
    [SkinAssetPath]
    public string NotificationsIcon { get; set; } = "";

    [ConfigEditable]
    [SkinAssetPath]
    public string UsersIcon { get; set; } = "";

    [ConfigEditable]
    [SkinAssetPath]
    public string SongRequestsIcon { get; set; } = "";

    [Range(1, 8192)]
    public float HeaderHeight { get; set; } = 60;

    [Range(0, 2048)]
    public float Padding { get; set; } = SkinV2MarginsConfig.Md;

    [Range(0, 2048)]
    public float Gap { get; set; } = SkinV2MarginsConfig.Md;

    [Range(1, 8192)]
    public float ButtonWidth { get; set; } = 232;

    [Range(1, 8192)]
    public float ButtonHeight { get; set; } = 40;

    [ConfigEditable]
    [Range(0, 4096)]
    public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

    [Range(1, 8192)]
    public float IconSize { get; set; } = 24;

    [SkinFont]
    public string Font { get; set; } = SkinV2FontWeightsConfig.Bold;

    [ConfigEditable]
    [Range(1, 256)]
    public int FontSize { get; set; } = SkinV2FontSizesConfig.TextLg;

    [ConfigEditable]
    [SkinColor]
    public string SelectedColor { get; set; } = "#6B83B2FF";

    [ConfigEditable]
    [SkinColor]
    public string UnselectedColor { get; set; } = "#273038FF";

    [ConfigEditable]
    [SkinColor]
    public string TextColor { get; set; } = "#FFFFFFFF";
}

public sealed class SkinV2HubSectionConfig
{
    [ConfigEditable]
    [SkinAssetPath]
    public string ClearIcon { get; set; } = "";

    [Range(0, 2048)]
    public float ContentPadding { get; set; } = SkinV2MarginsConfig.Md;

    [Range(1, 8192)]
    public float HeaderHeight { get; set; } = 60;

    [Range(1, 8192)]
    public float ControlHeight { get; set; } = 40;

    [Range(1, 8192)]
    public float ClearButtonWidth { get; set; } = 124;

    [ConfigEditable]
    [Range(0, 4096)]
    public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

    [Range(1, 8192)]
    public float IconSize { get; set; } = 20;

    [SkinFont]
    public string Font { get; set; } = SkinV2FontWeightsConfig.Bold;

    [Range(1, 256)]
    public int FontSize { get; set; } = SkinV2FontSizesConfig.TextLg;

    [Range(1, 256)]
    public int EmptyMessageFontSize { get; set; } = SkinV2FontSizesConfig.Text3Xl;

    [ConfigEditable]
    [SkinColor]
    public string BackgroundColor { get; set; } = "#273038FF";

    [ConfigEditable]
    [SkinColor]
    public string ControlColor { get; set; } = "#181E25FF";

    [ConfigEditable]
    [SkinColor]
    public string ControlHoverColor { get; set; } = "#354451FF";

    [ConfigEditable]
    [SkinColor]
    public string SelectedControlColor { get; set; } = "#6B83B2FF";

    [ConfigEditable]
    [SkinColor]
    public string TextColor { get; set; } = "#FFFFFFFF";

    [ConfigEditable]
    [SkinColor]
    public string AccentTextColor { get; set; } = "#8CAFEAFF";
}

public sealed class SkinV2HubUsersConfig
{
    [ConfigEditable]
    [Range(1, 8192)]
    public float SearchWidth { get; set; } = 464;

    [ConfigEditable]
    [Range(1, 8192)]
    public float FilterWidth { get; set; } = 242;

    [Range(1, 8192)]
    public float SearchIconSize { get; set; } = 18;

    [Range(0, 2048)]
    public float SearchIconInset { get; set; } = SkinV2MarginsConfig.Md;
}

public sealed class SkinV2HubNotificationsConfig
{
    [Range(1, 8192)]
    public float FeedButtonWidth { get; set; } = 237;
}

public sealed class SkinV2HubSongRequestsConfig
{
    [SkinAssetPath]
    public string TwitchIcon { get; set; } = "";

    [Range(1, 8192)]
    public float AlertsButtonWidth { get; set; } = 234;

    [Range(1, 8192)]
    public float TwitchButtonWidth { get; set; } = 232;

    [Range(0, 2048)]
    public float ActionGap { get; set; } = SkinV2MarginsConfig.Md;

    [Range(1, 8192)]
    public float TwitchIconWidth { get; set; } = 18;

    [Range(1, 8192)]
    public float TwitchIconHeight { get; set; } = 20;

    [ConfigEditable]
    [SkinColor]
    public string TwitchColor { get; set; } = "#9146FFFF";
}
