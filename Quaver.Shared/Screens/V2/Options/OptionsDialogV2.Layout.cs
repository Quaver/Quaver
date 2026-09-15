using System;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Window;

namespace Quaver.Shared.Screens.V2.Options
{
    /// <summary>
    ///     The panels below the header, and sizing the whole menu to the window.
    /// </summary>
    internal sealed partial class OptionsDialogV2
    {
        /// <summary>
        ///     Everything below the header: the left region and the content panel.
        /// </summary>
        private FlexContainer Body { get; set; }

        /// <summary>
        ///     Room for the closed rail, and the subcategory panel. The rail itself is drawn on top.
        /// </summary>
        private FlexContainer LeftRegion { get; set; }

        /// <summary>
        ///     The panel that holds the subcategory list.
        /// </summary>
        private RoundedPanel CategoryPanel { get; set; }

        private RoundedPanel ContentPanel { get; set; }

        private FlexItemOptions LeftRegionOptions { get; set; }

        /// <summary>
        ///     Keeps room in the left region for the closed rail.
        /// </summary>
        private FlexItemOptions RailSpacerOptions { get; set; }

        private float CurrentLeftRegionWidth { get; set; }

        private float LastWindowWidth { get; set; } = -1;

        private float LastWindowHeight { get; set; } = -1;

        private void CreateBody()
        {
            Body = new FlexContainer
            {
                Parent = RootSurface,
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Stretch,
                ColumnGap = Config.Layout.PanelGap
            };

            LeftRegion = new FlexContainer
            {
                Parent = Body,
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Stretch,
                ColumnGap = Config.Layout.PanelGap
            };
            LeftRegionOptions = new FlexItemOptions { Basis = Config.Layout.LeftRegionWidth, Shrink = 0 };
            Body.SetItemOptions(LeftRegion, LeftRegionOptions);

            var railSpacer = new Container { Parent = LeftRegion };
            RailSpacerOptions = new FlexItemOptions { Basis = Config.Layout.CollapsedRailWidth, Shrink = 0 };
            LeftRegion.SetItemOptions(railSpacer, RailSpacerOptions);

            CategoryPanel = new RoundedPanel(Config.Panels.CornerRadius)
            {
                Parent = LeftRegion,
                Tint = SkinV2Color.Parse(Config.Panels.CategoryColor)
            };
            LeftRegion.SetItemOptions(CategoryPanel, new FlexItemOptions { Basis = 1, Grow = 1, Shrink = 1 });

            ContentPanel = new RoundedPanel(Config.Panels.CornerRadius)
            {
                Parent = Body,
                Tint = SkinV2Color.Parse(Config.Panels.ContentColor)
            };
            Body.SetItemOptions(ContentPanel, new FlexItemOptions { Basis = 1, Grow = 1, Shrink = 1 });
        }

        /// <summary>
        ///     Sizes everything to the window. Does nothing when the window size did not change, unless forced.
        /// </summary>
        private void UpdateResponsiveLayout(bool force = false)
        {
            var windowWidth = WindowManager.Width;
            var windowHeight = WindowManager.Height;

            if (!force && Math.Abs(windowWidth - LastWindowWidth) < 0.001f &&
                Math.Abs(windowHeight - LastWindowHeight) < 0.001f)
                return;

            LastWindowWidth = windowWidth;
            LastWindowHeight = windowHeight;

            Container.Size = new ScalableVector2(windowWidth, windowHeight);
            PreviewRoot.Size = Container.Size;

            if (EditorRoot != null)
                EditorRoot.Size = Container.Size;

            var horizontalInset = Math.Min(Config.Layout.DialogInset, Math.Max(0, (windowWidth - 1) / 2f));
            var verticalInset = Math.Min(Config.Layout.DialogInset, Math.Max(0, (windowHeight - 1) / 2f));
            var rootWidth = Math.Min(Config.Layout.MaximumDialogWidth, Math.Max(1, windowWidth - horizontalInset * 2));
            var rootHeight = Math.Min(Config.Layout.MaximumDialogHeight,
                Math.Max(1, windowHeight - verticalInset * 2));

            var compact = windowWidth <= Config.Layout.CompactBreakpoint;
            var desiredLeftWidth = compact ? Config.Layout.CompactLeftRegionWidth : Config.Layout.LeftRegionWidth;
            var minimumLeftWidth = Config.Layout.CollapsedRailWidth + Config.Layout.PanelGap + 1;
            CurrentLeftRegionWidth = Math.Min(desiredLeftWidth,
                Math.Max(minimumLeftWidth, rootWidth - Config.Layout.PanelGap - 1));

            RootSurface.Position = new ScalableVector2((windowWidth - rootWidth) / 2f, (windowHeight - rootHeight) / 2f);
            RootSurface.Size = new ScalableVector2(rootWidth, rootHeight);
            ContentOverlayHost.Size = RootSurface.Size;

            Header.Size = new ScalableVector2(rootWidth, Math.Min(Config.Layout.HeaderHeight, rootHeight));

            var bodyY = Math.Min(rootHeight, Config.Layout.HeaderHeight + Config.Layout.PanelGap);
            Body.Position = new ScalableVector2(0, bodyY);
            Body.Size = new ScalableVector2(rootWidth, Math.Max(1, rootHeight - bodyY));

            HeaderTitleOptions.Basis = compact
                ? Math.Min(Config.Layout.CompactTitleWidth, CurrentLeftRegionWidth)
                : CurrentLeftRegionWidth;

            var availablePresetWidth = Math.Max(1, rootWidth - HeaderTitleOptions.Basis.Value -
                                                   Config.Layout.PanelGap * 2 - Config.Layout.MinimumSearchWidth);
            HeaderPresetOptions.Basis = Math.Min(Config.Layout.PresetWidth, availablePresetWidth);
            LeftRegionOptions.Basis = CurrentLeftRegionWidth;
            RailSpacerOptions.Basis = Math.Min(Config.Layout.CollapsedRailWidth, CurrentLeftRegionWidth);

            Header.RefreshLayout();
            LayoutTitleGroup(compact);
            Body.RefreshLayout();
            LeftRegion.RefreshLayout();

            SearchBar.Refresh(SearchActive);

            if (!SearchActive)
                PresetDropdown.Size = new ScalableVector2(PresetDropdown.Width, Header.Height);

            RailOverlay.Position = new ScalableVector2(0, bodyY);
            RailOverlay.Animations.Clear();
            RailOverlay.Size = new ScalableVector2(RailExpanded ? CurrentLeftRegionWidth : RailSpacerOptions.Basis.Value,
                Body.Height);
            CategoryPanel.Visible = !RailExpanded;

            var toggleSize = Math.Min(Config.Rail.ToggleButtonSize,
                Math.Max(1, Body.Height - Config.Rail.ToggleInset * 2));
            RailToggle.Size = new ScalableVector2(toggleSize, toggleSize);

            PresetDropdown.CloseMenu();
            UpdateNavigationLayout(true);
            ContentList.UpdateLayout(true);
            UpdateCategoryLabelProgress();
            UpdateEditorLayout();
        }
    }
}
