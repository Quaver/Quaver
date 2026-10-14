using System.ComponentModel.DataAnnotations;
using Quaver.Shared.Skinning.V2;
using Wobble.Configuration;

namespace Quaver.Shared.Screens.V2.Options;

public sealed class SkinV2OptionsConfig
{
    [Required]
    public SkinV2OptionsLayoutConfig Layout { get; set; } = new();

    [Required]
    public SkinV2OptionsPanelConfig Panel { get; set; } = new();

    [Required]
    public SkinV2OptionsHeaderConfig Header { get; set; } = new();

    [Required]
    public SkinV2OptionsSidebarConfig Sidebar { get; set; } = new();

    [Required]
    public SkinV2OptionsContentConfig Content { get; set; } = new();
}

public sealed class SkinV2OptionsLayoutConfig
{
    [ConfigEditable, Range(1, 4096)]
    public float Width { get; set; } = 1268;

    [ConfigEditable, Range(1, 4096)]
    public float Height { get; set; } = 622;

    [Range(0, 512)]
    public float WindowPadding { get; set; } = SkinV2Spacing.SpacingBase;

    [Range(0, 512)]
    public float Gap { get; set; } = SkinV2MarginsConfig.Md;

    [ConfigEditable, Range(1, 2048)]
    public float SidebarWidth { get; set; } = 318;

    [Range(1, 512)]
    public float HeaderHeight { get; set; } = 40;

    [Range(1, 512)]
    public float IconRailWidth { get; set; } = 60;

    [Range(0, 512)]
    public float ContentPadding { get; set; } = SkinV2MarginsConfig.Md;

    [ConfigEditable, Range(0, 512)]
    public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

    [Range(1, 10000)]
    public int AnimationDurationMilliseconds { get; set; } = 200;
}

public sealed class SkinV2OptionsPanelConfig
{
    [ConfigEditable, SkinColor]
    public string GapColor { get; set; } = "#424141FF";
}

public sealed class SkinV2OptionsHeaderConfig
{
    [Range(1, 2048)]
    public float TitleWidth { get; set; } = 89;

    [ConfigEditable, Range(1, 4096)]
    public float SearchWidth { get; set; } = 726;

    [ConfigEditable, Range(1, 2048)]
    public float PresetWidth { get; set; } = 242;

    [SkinFont]
    public string Font { get; set; } = SkinV2FontWeightsConfig.Bold;

    [Range(1, 256)]
    public int FontSize { get; set; } = SkinV2FontSizesConfig.TextLg;

    [ConfigEditable, SkinColor]
    public string CategoryColor { get; set; } = "#4B5973FF";

    [ConfigEditable, SkinColor]
    public string BackgroundColor { get; set; } = "#181E25FF";

    [ConfigEditable, SkinColor]
    public string TextColor { get; set; } = "#FFFFFFFF";

    [ConfigEditable, SkinColor]
    public string SecondaryTextColor { get; set; } = "#D9E3F4FF";

    [ConfigEditable, SkinColor]
    public string AccentTextColor { get; set; } = "#8CAFEAFF";

    [ConfigEditable, SkinColor]
    public string HoverColor { get; set; } = "#354451FF";

    [ConfigEditable, SkinColor]
    public string IconColor { get; set; } = "#EBF3FFFF";
}

public sealed class SkinV2OptionsSidebarConfig
{
    [ConfigEditable, SkinColor]
    public string BackgroundColor { get; set; } = "#273038FF";

    [ConfigEditable, SkinColor]
    public string RailColor { get; set; } = "#181E25FF";

    [ConfigEditable, SkinColor]
    public string ButtonColor { get; set; } = "#273038FF";

    [ConfigEditable, SkinColor]
    public string SectionButtonColor { get; set; } = "#181E25FF";

    [ConfigEditable, SkinColor]
    public string SelectedColor { get; set; } = "#6B83B2FF";

    [ConfigEditable, SkinColor]
    public string TextColor { get; set; } = "#D0DBEDFF";

    [ConfigEditable, SkinColor]
    public string SeparatorColor { get; set; } = "#D9E3F440";

    [Range(1, 512)]
    public float ButtonSize { get; set; } = 40;

    [Range(1, 512)]
    public float IconSize { get; set; } = 30;
}

public sealed class SkinV2OptionsContentConfig
{
    [ConfigEditable, SkinColor]
    public string BackgroundColor { get; set; } = "#273038FF";

    [ConfigEditable, SkinColor]
    public string CategoryHeaderColor { get; set; } = "#4B5973FF";

    [ConfigEditable, SkinColor]
    public string SectionHeaderColor { get; set; } = "#181E25FF";

    [ConfigEditable, SkinColor]
    public string TextColor { get; set; } = "#FFFFFFFF";

    [ConfigEditable, SkinColor]
    public string SecondaryTextColor { get; set; } = "#D9E3F4FF";
}
