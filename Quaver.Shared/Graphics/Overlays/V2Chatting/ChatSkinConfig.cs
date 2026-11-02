using System.ComponentModel.DataAnnotations;
using Quaver.Shared.Skinning.V2;
using Wobble.Configuration;

namespace Quaver.Shared.Graphics.Overlays.V2Chatting;

public sealed class SkinV2ChatConfig
{
    [Required]
    public SkinV2ChatLayoutConfig Layout { get; set; } = new();

    [Required]
    public SkinV2ChatPanelConfig Panels { get; set; } = new();

    [Required]
    public SkinV2ChatTabsConfig Tabs { get; set; } = new();

    [Required]
    public SkinV2ChatSearchConfig Search { get; set; } = new();

    [Required]
    public SkinV2ChatInputConfig Input { get; set; } = new();
}

public sealed class SkinV2ChatLayoutConfig
{
    [Range(1, 4096)]
    public float PanelHeight { get; set; } = 620;

    [ConfigEditable, Range(360, 720)]
    public float SearchSectionWidth { get; set; } = 480;

    [Range(0, 512)]
    public float Gap { get; set; } = SkinV2MarginsConfig.Md;

    [Range(0, 512)]
    public float Padding { get; set; } = SkinV2MarginsConfig.Md;

    [Range(1, 512)]
    public float HeaderHeight { get; set; } = 40;

    [Range(1, 512)]
    public float HeaderButtonSize { get; set; } = 40;

    [Range(1, 512)]
    public float InputHeight { get; set; } = 40;

    [Range(0, 512)]
    public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

    [Range(1, 10000)]
    public int AnimationDurationMilliseconds { get; set; } = 200;
}

public sealed class SkinV2ChatPanelConfig
{
    [ConfigEditable, SkinColor]
    public string SectionColor { get; set; } = "#273038FF";

    [ConfigEditable, SkinColor]
    public string HeaderColor { get; set; } = "#181E25FF";
}

public sealed class SkinV2ChatTabsConfig
{
    [SkinFont]
    public string Font { get; set; } = SkinV2FontWeightsConfig.Bold;

    [Range(1, 256)]
    public int FontSize { get; set; } = SkinV2FontSizesConfig.TextLg;

    [Range(1, 512)]
    public float IconSize { get; set; } = 30;

    [ConfigEditable, SkinColor]
    public string SelectedColor { get; set; } = "#6B83B2FF";

    [ConfigEditable, SkinColor]
    public string UnselectedColor { get; set; } = "#273038FF";

    [ConfigEditable, SkinColor]
    public string TextColor { get; set; } = "#FFFFFFFF";

    [ConfigEditable, SkinColor]
    public string UnreadTextColor { get; set; } = "#FFE032FF";
}

public sealed class SkinV2ChatSearchConfig
{
    [SkinFont]
    public string Font { get; set; } = SkinV2FontWeightsConfig.Bold;

    [Range(1, 256)]
    public int FontSize { get; set; } = SkinV2FontSizesConfig.TextLg;

    [Range(1, 512)]
    public float IconSize { get; set; } = 18;

    [ConfigEditable, Range(5, 30)]
    public float IconInset { get; set; } = SkinV2MarginsConfig.Md;

    [ConfigEditable, SkinColor]
    public string BackgroundColor { get; set; } = "#273038FF";

    [ConfigEditable, SkinColor]
    public string TextColor { get; set; } = "#FFFFFFFF";

    [ConfigEditable, SkinColor]
    public string PlaceholderColor { get; set; } = "#8CAFEA80";
}

public sealed class SkinV2ChatInputConfig
{
    [SkinFont]
    public string Font { get; set; } = SkinV2FontWeightsConfig.SemiBold;

    [Range(1, 256)]
    public int FontSize { get; set; } = SkinV2FontSizesConfig.TextLg;

    [ConfigEditable, SkinColor]
    public string BackgroundColor { get; set; } = "#181E25FF";

    [ConfigEditable, SkinColor]
    public string TextColor { get; set; } = "#FFFFFFFF";
}
