using System.ComponentModel.DataAnnotations;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble.Configuration;

namespace Quaver.Shared.Screens.V2.Options
{
    /// <summary>
    ///     Skin settings for the V2 options menu.
    /// </summary>
    public sealed class SkinV2OptionsConfig
    {
        [Required]
        public SkinV2OptionsLayoutConfig Layout { get; set; } = new SkinV2OptionsLayoutConfig();

        [Required]
        public SkinV2OptionsBackdropConfig Backdrop { get; set; } = new SkinV2OptionsBackdropConfig();

        [Required]
        public SkinV2OptionsHeaderConfig Header { get; set; } = new SkinV2OptionsHeaderConfig();

        [Required]
        public SkinV2OptionsPanelConfig Panels { get; set; } = new SkinV2OptionsPanelConfig();

        [Required]
        public SkinV2OptionsRailConfig Rail { get; set; } = new SkinV2OptionsRailConfig();

        [Required]
        public SkinV2OptionsCategoryNavigationConfig Categories { get; set; } =
            new SkinV2OptionsCategoryNavigationConfig();

        [Required]
        public SkinV2OptionsPresetConfig Preset { get; set; } = new SkinV2OptionsPresetConfig();

        [Required]
        public SkinV2OptionsRowConfig Rows { get; set; } = new SkinV2OptionsRowConfig();
    }

    public sealed class SkinV2OptionsLayoutConfig
    {
        [Range(0, 2048)]
        public float DialogInset { get; set; } = SkinV2Spacing.Spacing3Xl;

        [Range(1, 8192)]
        public float MaximumDialogWidth { get; set; } = 1268;

        [Range(1, 8192)]
        public float MaximumDialogHeight { get; set; } = 610;

        [Range(1, 8192)]
        public float HeaderHeight { get; set; } = 40;

        [Range(0, 2048)]
        public float PanelGap { get; set; } = SkinV2Spacing.Spacing2Xs;

        [Range(1, 8192)]
        public float LeftRegionWidth { get; set; } = 318;

        [Range(1, 8192)]
        public float CompactLeftRegionWidth { get; set; } = 220;

        [Range(1, 8192)]
        public float CollapsedRailWidth { get; set; } = 60;

        [Range(1, 8192)]
        public float PresetWidth { get; set; } = 204;

        [Range(1, 8192)]
        public float TitleLabelWidth { get; set; } = 100;

        [Range(1, 8192)]
        public float CompactTitleWidth { get; set; } = 90;

        [Range(1, 8192)]
        public float CompactBreakpoint { get; set; } = 900;

        [Range(1, 8192)]
        public float MinimumSearchWidth { get; set; } = 120;
    }

    public sealed class SkinV2OptionsBackdropConfig
    {
        [ConfigEditable]
        [SkinColor]
        public string Color { get; set; } = "#000000FF";

        [Range(0d, 1d)]
        public float Opacity { get; set; } = 0.75f;

        [ConfigEditable]
        [SkinColor]
        public string GapColor { get; set; } = "#000000FF";

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;
    }

    public sealed class SkinV2OptionsHeaderConfig
    {
        [ConfigEditable]
        [SkinColor]
        public string BackgroundColor { get; set; } = "#181E25FF";

        [ConfigEditable]
        [SkinColor]
        public string ActiveTitleColor { get; set; } = "#4B5973FF";

        [ConfigEditable]
        [SkinColor]
        public string TextColor { get; set; } = "#FFFFFFFF";

        /// <summary>
        ///     The description text to the right of the title.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string SubtitleTextColor { get; set; } = "#D9E3F4FF";

        [ConfigEditable]
        [SkinColor]
        public string MutedTextColor { get; set; } = "#8A8A8AFF";

        [SkinFont]
        public string Font { get; set; } = SkinV2FontWeightsConfig.SemiBold;

        [Range(1, 256)]
        public int FontSize { get; set; } = SkinV2FontSizesConfig.TextLg;

        [Range(0, 2048)]
        public float HorizontalPadding { get; set; } = SkinV2Spacing.Spacing2Xs;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;
    }

    public sealed class SkinV2OptionsPanelConfig
    {
        /// <summary>
        ///     The rail background while the rail is closed.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string RailCollapsedColor { get; set; } = "#181E25FF";

        /// <summary>
        ///     The rail background while the rail is open.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string RailExpandedColor { get; set; } = "#273038FF";

        [ConfigEditable]
        [SkinColor]
        public string CategoryColor { get; set; } = "#273038FF";

        [ConfigEditable]
        [SkinColor]
        public string ContentColor { get; set; } = "#273038FF";

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;
    }

    public sealed class SkinV2OptionsRailConfig
    {
        [Range(1, 10000)]
        public int ExpansionDurationMilliseconds { get; set; } = 180;

        [Range(1, 8192)]
        public float ToggleButtonSize { get; set; } = 40;

        [Range(1, 8192)]
        public float ToggleIconSize { get; set; } = SkinV2Spacing.Spacing3Xl;

        [Range(0, 2048)]
        public float ToggleInset { get; set; } = SkinV2Spacing.Spacing2Xs;

        /// <summary>
        ///     The background of the rail's open/close button while the rail is closed.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string ToggleCollapsedColor { get; set; } = "#273038FF";

        /// <summary>
        ///     The background of the rail's open/close button while the rail is open.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string ToggleExpandedColor { get; set; } = "#181E25FF";

        [SkinColor]
        public string ToggleIconCollapsedColor { get; set; } = "#FFFFFFFF";

        [SkinColor]
        public string ToggleIconExpandedColor { get; set; } = "#FFFFFFFF";

        [Range(0, 4096)]
        public float ToggleCornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;
    }

    public sealed class SkinV2OptionsCategoryNavigationConfig
    {
        [SkinAssetPath]
        public string IconAtlas { get; set; } = "";

        [SkinFont]
        public string Font { get; set; } = SkinV2FontWeightsConfig.SemiBold;

        [Range(1, 256)]
        public int FontSize { get; set; } = SkinV2FontSizesConfig.TextLg;

        [Range(1, 8192)]
        public float ButtonHeight { get; set; } = 40;

        [Range(1, 8192)]
        public float IconSize { get; set; } = 30;

        [Range(0, 2048)]
        public float PanelInset { get; set; } = SkinV2Spacing.Spacing2Xs;

        [Range(0, 2048)]
        public float RowSpacing { get; set; } = SkinV2Spacing.Spacing2Xs;

        [Range(0, 2048)]
        public float LabelSpacing { get; set; } = SkinV2Spacing.Spacing2Xs;

        [Range(0, 2048)]
        public float HorizontalPadding { get; set; } = SkinV2Spacing.Spacing2Xs;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [Range(0, 128)]
        public float ScrollbarWidth { get; set; } = 3;

        /// <summary>
        ///     How far open the rail is (0 to 1) when the category names are fully shown.
        /// </summary>
        [Range(0.01d, 1d)]
        public float LabelRevealProgress { get; set; } = 1;

        [ConfigEditable]
        [SkinColor]
        public string ForegroundColor { get; set; } = "#FFFFFFFF";

        [ConfigEditable]
        [SkinColor]
        public string SelectedForegroundColor { get; set; } = "#FFFFFFFF";

        /// <summary>
        ///     An unselected category button's background while the rail is closed.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string RailButtonCollapsedColor { get; set; } = "#273038FF";

        /// <summary>
        ///     An unselected category button's background while the rail is open.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string RailButtonExpandedColor { get; set; } = "#181E25FF";

        [ConfigEditable]
        [SkinColor]
        public string RailButtonSelectedColor { get; set; } = "#6B83B2FF";

        [ConfigEditable]
        [SkinColor]
        public string SubcategoryButtonColor { get; set; } = "#181E25FF";

        [ConfigEditable]
        [SkinColor]
        public string SubcategoryButtonSelectedColor { get; set; } = "#6B83B2FF";

        [ConfigEditable]
        [SkinColor]
        public string ScrollbarColor { get; set; } = "#A7A7A7FF";

        [Range(1, 8192)]
        public float SearchResetIconSize { get; set; } = 30;

        /// <summary>
        ///     The color of the category icons and names while a search is running.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string SearchDimmedForegroundColor { get; set; } = "#D9E3F480";

        /// <summary>
        ///     The line in the rail between the pinned entry and the categories.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string SearchSeparatorColor { get; set; } = "#D9E3F480";

        [Range(1, 2048)]
        public float SearchSeparatorThickness { get; set; } = 1;
    }

    public sealed class SkinV2OptionsPresetConfig
    {
        [ConfigEditable]
        [SkinColor]
        public string BackgroundColor { get; set; } = "#181E25FF";

        [ConfigEditable]
        [SkinColor]
        public string TextColor { get; set; } = "#6B83B2FF";

        [ConfigEditable]
        [SkinColor]
        public string MenuColor { get; set; } = "#273038FF";

        [ConfigEditable]
        [SkinColor]
        public string ItemColor { get; set; } = "#181E25FF";

        [ConfigEditable]
        [SkinColor]
        public string SelectedItemColor { get; set; } = "#6B83B2FF";

        [ConfigEditable]
        [SkinColor]
        public string SelectedTextColor { get; set; } = "#FFFFFFFF";

        [SkinFont]
        public string Font { get; set; } = SkinV2FontWeightsConfig.SemiBold;

        [Range(1, 256)]
        public int FontSize { get; set; } = SkinV2FontSizesConfig.TextLg;

        [Range(1, 8192)]
        public float IconSize { get; set; } = SkinV2Spacing.SpacingBase;

        [Range(0, 2048)]
        public float HorizontalPadding { get; set; } = SkinV2Spacing.Spacing2Xs;

        [Range(0, 2048)]
        public float MenuGap { get; set; } = SkinV2MarginsConfig.Sm;

        [Range(0, 2048)]
        public float MenuPadding { get; set; } = SkinV2MarginsConfig.Sm;

        [Range(0, 2048)]
        public float ItemSpacing { get; set; } = 2;

        [Range(1, 8192)]
        public float ItemHeight { get; set; } = 36;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;
    }

    /// <summary>
    ///     Skin settings for the option rows, section headers and dividers.
    /// </summary>
    public sealed class SkinV2OptionsRowConfig
    {
        [Range(1, 8192)]
        public float RowHeight { get; set; } = 40;

        [Range(0, 2048)]
        public float RowSpacing { get; set; } = SkinV2Spacing.Spacing2Xs;

        [Range(0, 2048)]
        public float ContentInset { get; set; } = SkinV2Spacing.Spacing2Xs;

        [Range(0, 2048)]
        public float HorizontalPadding { get; set; } = SkinV2MarginsConfig.Md;

        /// <summary>
        ///     The gap between a control and its label bar. For controls that sit on the label bar
        ///     (toggles, sliders, key fields) it is the gap to the bar's right edge.
        /// </summary>
        [Range(0, 2048)]
        public float ControlGap { get; set; } = SkinV2Spacing.Spacing2Xs;

        [Range(0, 4096)]
        public float CornerRadius { get; set; } = SkinV2BorderRadiusConfig.Normal;

        [ConfigEditable]
        [SkinColor]
        public string BackgroundColor { get; set; } = "#181E25FF";

        [SkinFont]
        public string LabelFont { get; set; } = SkinV2FontWeightsConfig.SemiBold;

        [Range(1, 256)]
        public int LabelFontSize { get; set; } = 18;

        [ConfigEditable]
        [SkinColor]
        public string LabelColor { get; set; } = "#FFFFFFFF";

        [SkinFont]
        public string SectionHeaderFont { get; set; } = SkinV2FontWeightsConfig.Bold;

        [Range(1, 256)]
        public int SectionHeaderFontSize { get; set; } = SkinV2FontSizesConfig.TextLg;

        [ConfigEditable]
        [SkinColor]
        public string SectionHeaderTextColor { get; set; } = "#FFFFFFFF";

        /// <summary>
        ///     The background behind "{Category} Settings" in a section header.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string SectionHeaderCategoryColor { get; set; } = "#4B5973FF";

        /// <summary>
        ///     The background behind the subcategory name in a section header.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string SectionHeaderSubcategoryColor { get; set; } = "#181E25FF";

        /// <summary>
        ///     The color of the subcategory name in a section header.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string SectionHeaderSubcategoryTextColor { get; set; } = "#D9E3F4FF";

        [Range(1, 256)]
        public int ControlFontSize { get; set; } = 18;

        [ConfigEditable]
        [SkinColor]
        public string ControlTextColor { get; set; } = "#FFFFFFFF";

        [Range(1, 8192)]
        public float DropdownWidth { get; set; } = 190;

        /// <summary>
        ///     The normal width of a keybind field. It grows to fit longer binds.
        /// </summary>
        [Range(1, 8192)]
        public float KeybindFieldWidth { get; set; } = 190;

        /// <summary>
        ///     The text color of a keybind or key layout field.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string KeybindFieldTextColor { get; set; } = "#8CAFEAFF";

        [Range(1, 8192)]
        public float KeybindHelpIconSize { get; set; } = 20;

        [Range(1, 256)]
        public int KeybindHelpIconFontSize { get; set; } = 14;

        [ConfigEditable]
        [SkinColor]
        public string KeybindHelpIconColor { get; set; } = "#D9E3F4FF";

        [ConfigEditable]
        [SkinColor]
        public string KeybindHelpIconTextColor { get; set; } = "#181E25FF";

        /// <summary>
        ///     The text color of a key field while it waits for a key, or when its bind is also used by
        ///     another action.
        /// </summary>
        [ConfigEditable]
        [SkinColor]
        public string KeybindAlertColor { get; set; } = "#FF3A6FFF";

        /// <summary>
        ///     The normal width of a key layout field. It grows to fit longer layouts.
        /// </summary>
        [Range(1, 8192)]
        public float GameModeKeyLayoutFieldWidth { get; set; } = 190;

        /// <summary>
        ///     The thickness of the line between groups of rows.
        /// </summary>
        [Range(1, 128)]
        public float DividerThickness { get; set; } = 1;

        [ConfigEditable]
        [SkinColor]
        public string DividerColor { get; set; } = "#D9E3F440";

        [Range(1, 128)]
        public float ScrollbarWidth { get; set; } = 3;

        [ConfigEditable]
        [SkinColor]
        public string ScrollbarColor { get; set; } = "#A7A7A7FF";

        [Required]
        public SkinV2DropdownConfig Dropdown { get; set; } = new SkinV2DropdownConfig
        {
            FontSize = 18,
            TriggerColor = "#181E25FF",
            ItemColor = "#181E25FF",
            HoverColor = "#606060FF",
            SelectedItemColor = "#6B83B2FF",
            TextColor = "#6B83B2FF",
            SelectedTextColor = "#FFFFFFFF",
            IconColor = "#8CAFEAFF",
            DividerColor = "#FFFFFF40",
            ScrollbarColor = "#A7A7A7FF"
        };
    }
}
