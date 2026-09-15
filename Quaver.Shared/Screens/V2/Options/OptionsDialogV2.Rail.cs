using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Screens.V2.Options.UI;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Input;

namespace Quaver.Shared.Screens.V2.Options
{
    /// <summary>
    ///     The category rail. While closed it shows only icons. When opened it slides over the
    ///     subcategory panel and shows the category names.
    /// </summary>
    internal sealed partial class OptionsDialogV2
    {
        private RoundedPanel RailOverlay { get; set; }

        /// <summary>
        ///     The arrow button at the bottom of the rail that opens and closes it.
        /// </summary>
        private RoundedButton RailToggle { get; set; }

        private bool RailExpanded { get; set; }

        private Color RailCollapsedColor { get; set; }

        private Color RailExpandedColor { get; set; }

        private Color RailToggleCollapsedColor { get; set; }

        private Color RailToggleExpandedColor { get; set; }

        private Color RailToggleIconCollapsedColor { get; set; }

        private Color RailToggleIconExpandedColor { get; set; }

        private void CreateRailOverlay()
        {
            RailCollapsedColor = SkinV2Color.Parse(Config.Panels.RailCollapsedColor);
            RailExpandedColor = SkinV2Color.Parse(Config.Panels.RailExpandedColor);
            RailToggleCollapsedColor = SkinV2Color.Parse(Config.Rail.ToggleCollapsedColor);
            RailToggleExpandedColor = SkinV2Color.Parse(Config.Rail.ToggleExpandedColor);
            RailToggleIconCollapsedColor = SkinV2Color.Parse(Config.Rail.ToggleIconCollapsedColor);
            RailToggleIconExpandedColor = SkinV2Color.Parse(Config.Rail.ToggleIconExpandedColor);

            RailOverlay = new RoundedPanel(Config.Panels.CornerRadius)
            {
                Parent = RootSurface,
                Tint = RailCollapsedColor,
                DrawOrder = 100
            };

            RailToggle = new RoundedButton((sender, args) => SetRailExpanded(!RailExpanded, true))
            {
                Parent = RailOverlay,
                Alignment = Alignment.BotLeft,
                Position = new ScalableVector2(Config.Rail.ToggleInset, -Config.Rail.ToggleInset),
                Size = new ScalableVector2(Config.Rail.ToggleButtonSize, Config.Rail.ToggleButtonSize),
                CornerRadius = Config.Rail.ToggleCornerRadius,
                Tint = RailToggleCollapsedColor,
                PerformHoverFade = true,
                Depth = -100
            };

            UpdateRailIcon();
            RailToggle.Icon.Tint = RailToggleIconCollapsedColor;
        }

        /// <summary>
        ///     Closes the rail when clicking outside it. Hides the subcategory panel once the rail has
        ///     finished opening over it.
        /// </summary>
        private void UpdateRail()
        {
            if (!RailExpanded)
                return;

            if (MouseManager.IsUniqueClick(MouseButton.Left) && RootSurface.IsHovered() && !RailOverlay.IsHovered())
            {
                SetRailExpanded(false, true);
                return;
            }

            if (RailOverlay.Animations.Count == 0)
                CategoryPanel.Visible = false;
        }

        private void SetRailExpanded(bool expanded, bool animate)
        {
            if (RailExpanded == expanded)
                return;

            RailExpanded = expanded;

            if (!expanded)
                CategoryPanel.Visible = true;

            RailOverlay.Animations.Clear();

            var targetWidth = expanded
                ? CurrentLeftRegionWidth
                : RailSpacerOptions.Basis ?? Config.Layout.CollapsedRailWidth;

            if (animate && V2PerformanceMode.TransitionsEnabled)
            {
                RailOverlay.ChangeWidthTo((int) targetWidth, Easing.OutCubic, Config.Rail.ExpansionDurationMilliseconds);
            }
            else
            {
                RailOverlay.Width = targetWidth;
                CategoryPanel.Visible = !expanded;
            }

            UpdateCategoryLabelProgress();
            UpdateRailIcon();
        }

        private void UpdateRailIcon() =>
            RailToggle.SetIcon(OptionsIconAtlas.GetRegion(OptionsIcons,
                    RailExpanded ? OptionsIconFrame.Collapse : OptionsIconFrame.Expand),
                new Vector2(Config.Rail.ToggleIconSize, Config.Rail.ToggleIconSize));

        /// <summary>
        ///     Blends the rail colors and fades in the category names, based on how far the rail is open.
        /// </summary>
        private void UpdateCategoryLabelProgress()
        {
            if (CategoryButtons.Count == 0)
                return;

            var collapsedWidth = RailSpacerOptions.Basis ?? Config.Layout.CollapsedRailWidth;
            var widthRange = Math.Max(0.001f, CurrentLeftRegionWidth - collapsedWidth);

            var progress = MathHelper.Clamp((RailOverlay.Width - collapsedWidth) / widthRange, 0, 1);

            var revealProgress = Math.Max(0.001f, Config.Categories.LabelRevealProgress);
            var labelProgress = MathHelper.SmoothStep(0, 1, MathHelper.Clamp(progress / revealProgress, 0, 1));

            RailOverlay.Tint = Color.Lerp(RailCollapsedColor, RailExpandedColor, progress);
            RailToggle.Tint = Color.Lerp(RailToggleCollapsedColor, RailToggleExpandedColor, progress);
            RailToggle.Icon.Tint = Color.Lerp(RailToggleIconCollapsedColor, RailToggleIconExpandedColor, progress);

            foreach (var button in CategoryButtons)
            {
                button.SetLabelExpansionProgress(labelProgress);
                button.SetIdleColorProgress(progress);
            }

            SearchRailButton?.SetLabelExpansionProgress(labelProgress);
        }
    }
}
