using System;
using Quaver.Shared.Screens.V2.Options.UI;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Options
{
    /// <summary>
    ///     The header: the title panel, the search bar and the preset picker.
    /// </summary>
    internal sealed partial class OptionsDialogV2
    {
        private FlexContainer Header { get; set; }

        /// <summary>
        ///     "Options" on the left part and a short description on the right part.
        /// </summary>
        private SplitRoundedPanel TitleGroup { get; set; }

        private MarqueeSpriteText ActiveTitleText { get; set; }

        private MarqueeSpriteText SubtitleText { get; set; }

        private V2SearchBar SearchBar { get; set; }

        private OptionsPresetDropdown PresetDropdown { get; set; }

        /// <summary>
        ///     Holds the preset dropdown while a search is running, so the search bar gets its space.
        ///     The dropdown has to leave the header completely, or the header still keeps a gap for it.
        /// </summary>
        private Container HeaderDetachedHost { get; set; }

        private FlexItemOptions HeaderTitleOptions { get; set; }

        private FlexItemOptions HeaderPresetOptions { get; set; }

        private void CreateHeader()
        {
            var font = FontManager.GetWobbleFont(Config.Header.Font);

            Header = new FlexContainer
            {
                Parent = RootSurface,
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Stretch,
                ColumnGap = Config.Layout.PanelGap
            };

            TitleGroup = new SplitRoundedPanel(Config.Header.CornerRadius,
                SkinV2Color.Parse(Config.Header.ActiveTitleColor), SkinV2Color.Parse(Config.Header.BackgroundColor))
            {
                Parent = Header
            };
            HeaderTitleOptions = new FlexItemOptions { Basis = Config.Layout.LeftRegionWidth, Shrink = 0 };
            Header.SetItemOptions(TitleGroup, HeaderTitleOptions);

            ActiveTitleText = CreateTitleLabel(font, LocalizationManager.Get("Screen_Main_Options"),
                Config.Header.TextColor);
            SubtitleText = CreateTitleLabel(font, LocalizationManager.Get("Screen_Options_AdjustGameSettings"),
                Config.Header.SubtitleTextColor);

            SearchBar = new V2SearchBar(font, RootConfig.Shared.Search,
                OptionsIconAtlas.GetRegion(OptionsIcons, OptionsIconFrame.Search),
                LocalizationManager.Get("Screen_Options_Searchforoptions"))
            {
                Parent = Header
            };
            SearchBar.SetResultText(LocalizationManager.Get("Screen_Options_OptionsFound", 0));
            SearchBar.StoppedTyping += (sender, query) => PendingNavigationAction = () => ApplySearch(query);
            SearchBar.ClearClicked += (sender, args) => PendingNavigationAction = () => ClearSearch();
            Header.SetItemOptions(SearchBar, new FlexItemOptions
            {
                Basis = Config.Layout.MinimumSearchWidth,
                Grow = 1,
                Shrink = 1
            });

            HeaderDetachedHost = new Container
            {
                Parent = RootSurface,
                Size = new ScalableVector2(0, 0),
                Visible = false
            };

            PresetDropdown = new OptionsPresetDropdown(Config.Layout.PresetWidth, Config.Layout.HeaderHeight,
                Config.Preset)
            {
                Parent = Header
            };
            HeaderPresetOptions = new FlexItemOptions { Basis = Config.Layout.PresetWidth, Shrink = 0 };
            Header.SetItemOptions(PresetDropdown, HeaderPresetOptions);

            ApplyPresetVisibility();
        }

        /// <summary>
        ///     Long title text only scrolls while hovered.
        /// </summary>
        private void UpdateHeader()
        {
            ActiveTitleText.IsActive = ActiveTitleText.IsHovered();
            SubtitleText.IsActive = SubtitleText.Visible && SubtitleText.IsHovered();
        }

        private MarqueeSpriteText CreateTitleLabel(WobbleFontStore font, string text, string color)
        {
            var label = new MarqueeSpriteText(font, text, Config.Header.FontSize, 1)
            {
                Parent = TitleGroup,
                Alignment = Alignment.MidLeft
            };
            label.TextSprite.Tint = SkinV2Color.Parse(color);
            return label;
        }

        /// <summary>
        ///     In compact mode the title uses the whole panel and the description is hidden.
        /// </summary>
        private void LayoutTitleGroup(bool compact)
        {
            var titleWidth = compact ? TitleGroup.Width : Math.Min(Config.Layout.TitleLabelWidth, TitleGroup.Width);
            var subtitleWidth = Math.Max(0, TitleGroup.Width - titleWidth);
            var padding = Config.Header.HorizontalPadding;

            TitleGroup.SplitPosition = titleWidth;

            ActiveTitleText.Position = new ScalableVector2(padding, 0);
            ActiveTitleText.Size = new ScalableVector2(Math.Max(1, titleWidth - padding * 2), TitleGroup.Height);

            SubtitleText.Position = new ScalableVector2(titleWidth + padding, 0);
            SubtitleText.Size = new ScalableVector2(Math.Max(1, subtitleWidth - padding * 2), TitleGroup.Height);
            SubtitleText.Visible = !compact && subtitleWidth > 0;
        }

        /// <summary>
        ///     Takes the preset dropdown out of the header while a search is running, and puts it back
        ///     when the search ends.
        /// </summary>
        private void ApplyPresetVisibility()
        {
            var shown = !SearchActive;

            if (ReferenceEquals(PresetDropdown.Parent, Header) == shown)
                return;

            PresetDropdown.CloseMenu();
            PresetDropdown.Visible = shown;
            PresetDropdown.SetInteractionEnabled(shown);
            PresetDropdown.Parent = shown ? Header : HeaderDetachedHost;

            if (shown)
            {
                PresetDropdown.Size = new ScalableVector2(HeaderPresetOptions.Basis ?? Config.Layout.PresetWidth,
                    Header.Height);
                Header.SetItemOptions(PresetDropdown, HeaderPresetOptions);
            }

            Header.RefreshLayout();
        }
    }
}
