using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Modifiers.Mods;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.Selection.UI;
using Quaver.Shared.Screens.V2.Selection.UI.Panel.Leaderboard;
using Quaver.Shared.Screens.V2.Selection.UI.Panel.Modifiers;
using Quaver.Shared.Skinning.V2;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Selection.UI.Panel
{
    /// <summary>
    ///     Owns the Song Select left-side tabs and its content
    /// </summary>
    internal sealed class Panel : Container
    {
        private const float MapViewButtonSize = 40;

        private Bindable<SelectContainerPanel> ActivePanel { get; }
        private SkinV2SelectionLeftPanelConfig Config { get; }
        private FlexContainer Layout { get; }
        private Container Header { get; }
        private Container HeaderGap { get; }
        private FlexContainer HeaderLayout { get; }
        private Container Content { get; }
        private Container FooterGap { get; }
        private Container Footer { get; }
        private Container LeaderboardFooterContent { get; }
        private Container ModifiersFooterContent { get; }
        private RoundedButton LeaderboardButton { get; }
        private RoundedButton ModifiersButton { get; }
        private RoundedButton MapViewButton { get; }
        private float ModifiersButtonBaseWidth { get; }
        private List<Sprite> ModifierIcons { get; } = new();
        private Container LeaderboardContent { get; }
        private Container ModifiersContent { get; }
        private Container MapPreviewContent { get; }
        private LeaderboardPanel LeaderboardPanel { get; }
        private ModifiersPanel ModifiersPanel { get; }

        internal Panel(Bindable<SelectContainerPanel> activePanel,
            SkinV2SelectionLeftPanelConfig config,
            SkinV2SelectionLeaderboardConfig leaderboardConfig,
            SkinV2SelectionModifiersConfig modifiersConfig, Container overlayHost)
        {
            ActivePanel = activePanel;
            Config = config;

            Layout = new FlexContainer
            {
                Parent = this,
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Stretch
            };
            Header = CreatePanel(Layout, Config.HeaderColor);
            Layout.SetItemOptions(Header, new FlexItemOptions { Basis = Config.HeaderHeight, Shrink = 0 });

            HeaderLayout = new FlexContainer
            {
                Parent = Header,
                Position = new ScalableVector2(Config.Padding, 0),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Center,
                JustifyContent = FlexJustifyContent.FlexStart,
                ColumnGap = Config.ButtonGap
            };

            LeaderboardButton = CreateTabButton(SelectionLocalization.Get("Leaderboard"), SelectContainerPanel.Leaderboard);
            ModifiersButton = CreateTabButton(SelectionLocalization.Get("Modifiers"), SelectContainerPanel.Modifiers);
            ModifiersButtonBaseWidth = ModifiersButton.Width;
            ModifiersButton.WidthMode = ButtonSizeMode.Fixed;
            RefreshModifierIcons();
            
            MapViewButton = new RoundedButton((sender, args) => ActivePanel.Value = SelectContainerPanel.MapPreview)
            {
                Parent = Header,
                Alignment = Alignment.MidRight,
                X = -Config.Padding,
                Size = new ScalableVector2(MapViewButtonSize, MapViewButtonSize),
                CornerRadius = Config.CornerRadius,
                PerformHoverFade = false
            };
            MapViewButton.SetIcon(GlobalIcons.Get(GlobalIcon.ViewMap), new Vector2(MapViewButtonSize, MapViewButtonSize));

            HeaderGap = new Container { Parent = Layout };
            Layout.SetItemOptions(HeaderGap, new FlexItemOptions { Basis = Config.Gap, Shrink = 0 });

            Content = CreatePanel(Layout, Config.ContentColor);
            Layout.SetItemOptions(Content, new FlexItemOptions { Basis = 0, Grow = 1, Shrink = 1 });
            Content.SizeChanged += OnContentSizeChanged;

            LeaderboardContent = new Container { Parent = Content };
            ModifiersContent = new Container { Parent = Content };
            MapPreviewContent = new Container { Parent = Content };

            FooterGap = new Container { Parent = Layout };
            Layout.SetItemOptions(FooterGap, new FlexItemOptions { Basis = 0, Shrink = 0 });

            Footer = CreatePanel(Layout, Config.FooterColor);
            Layout.SetItemOptions(Footer, new FlexItemOptions { Basis = 0, Shrink = 0 });
            Footer.SizeChanged += OnFooterSizeChanged;
            LeaderboardFooterContent = new Container { Parent = Footer };
            ModifiersFooterContent = new Container { Parent = Footer };

            LeaderboardPanel = new LeaderboardPanel(leaderboardConfig, overlayHost, LeaderboardFooterContent)
            {
                Parent = LeaderboardContent,
                Size = LeaderboardContent.Size
            };

            ModifiersPanel = new ModifiersPanel(modifiersConfig, overlayHost,
                ModifiersFooterContent)
            {
                Parent = ModifiersContent,
                Size = ModifiersContent.Size
            };

            SizeChanged += OnSizeChanged;
            ActivePanel.ValueChanged += OnActivePanelChanged;
            ModManager.ModsChanged += OnModsChanged;

            RefreshSelectedPanel();
        }

        private RoundedButton CreateTabButton(string label, SelectContainerPanel panel)
        {
            var button = new RoundedButton((sender, args) => ActivePanel.Value = panel)
            {
                Parent = HeaderLayout,
                Size = new ScalableVector2(1, Config.ButtonHeight),
                WidthMode = ButtonSizeMode.Auto,
                AutoSizePadding = new Vector2(Config.ButtonHorizontalPadding * 2, 0),
                CornerRadius = Config.CornerRadius,
                PerformHoverFade = false
            };
            button.SetLabel(FontManager.GetWobbleFont(Config.Font), label, Config.FontSize, SkinV2Color.Parse(Config.TextColor));
            button.Label.Alignment = Alignment.MidLeft;
            button.Label.X = Config.ButtonHorizontalPadding;
            HeaderLayout.SetItemOptions(button, new FlexItemOptions { Basis = button.Width, Shrink = 0 });

            return button;
        }

        /// <summary>
        ///     Rebuilds the modifier icons inside the tab button.
        /// </summary>
        private void RefreshModifierIcons()
        {
            foreach (var icon in ModifierIcons)
                icon.Destroy();
            ModifierIcons.Clear();

            var modifiers = ModManager.GetModsList(ModManager.Mods);
            var iconWidth = GlobalGameplayAssets.ModDisplaySize.X;
            var iconHeight = GlobalGameplayAssets.ModDisplaySize.Y;

            for (var i = 0; i < modifiers.Count; i++)
            {
                var icon = new Sprite
                {
                    Parent = ModifiersButton,
                    Alignment = Alignment.MidLeft,
                    Size = new ScalableVector2(iconWidth, iconHeight),
                    Region = GetModifierIcon(modifiers[i]),
                    UsePreviousSpriteBatchOptions = true,
                    Alpha = 1
                };
                ModifierIcons.Add(icon);
            }

            LayoutModifierIcons();
        }

        /// <summary>
        ///     Fits the modifier icon list in the header section button
        /// </summary>
        private void LayoutModifierIcons()
        {
            var count = ModifierIcons.Count;
            var iconWidth = GlobalGameplayAssets.ModDisplaySize.X;
            
            var defaultIconStep = iconWidth - 6;
            var desiredIconsWidth = count == 0 ? 0 : iconWidth + defaultIconStep * (count - 1) + Config.ButtonHorizontalPadding;
            
            var desiredButtonWidth = ModifiersButtonBaseWidth + desiredIconsWidth;
            var maximumButtonWidth = HeaderLayout.Width > 0 ? Math.Max(ModifiersButtonBaseWidth, HeaderLayout.Width - LeaderboardButton.Width - Config.ButtonGap) : desiredButtonWidth;
            var buttonWidth = Math.Min(desiredButtonWidth, maximumButtonWidth);
            
            var availableIconWidth = Math.Max(0, buttonWidth - ModifiersButtonBaseWidth - Config.ButtonHorizontalPadding);
            var iconStep = count <= 1 ? 0 : Math.Min(defaultIconStep, Math.Max(0, (availableIconWidth - iconWidth) / (count - 1)));

            for (var i = 0; i < count; i++)
                ModifierIcons[i].X = ModifiersButtonBaseWidth + iconStep * i;

            ModifiersButton.Width = buttonWidth;
            HeaderLayout.SetItemOptions(ModifiersButton, new FlexItemOptions { Basis = ModifiersButton.Width, Shrink = 0 });

            HeaderLayout.RefreshLayout();
        }

        /// <summary>
        ///     Returns the active atlas icon for a modifier
        /// </summary>
        private static TextureRegion GetModifierIcon(ModIdentifier identifier)
        {
            var modifier = ModManager.CurrentModifiersList.Find(x => x.ModIdentifier == identifier);
            return modifier is ModSpeed ? GlobalGameplayAssets.GetRate(ModHelper.GetRateFromMods(ModManager.Mods)) : GlobalGameplayAssets.GetMod(identifier);
        }

        private Container CreatePanel(FlexContainer parent, string color)
        {
            var panel = new Container { Parent = parent };
            var margin = (int) Math.Ceiling(Config.CornerRadius);
            var textureSize = Math.Max(2, margin * 2 + 2);
            var background = new NineSliceSprite
            {
                Parent = panel,
                Image = RoundedRectTextureCache.Get(textureSize, textureSize, Config.CornerRadius),
                Margins = new SliceMargins(margin),
                Tint = SkinV2Color.Parse(color),
                UsePreviousSpriteBatchOptions = true
            };
            panel.SizeChanged += (sender, size) => background.Size = size;

            return panel;
        }

        private void OnSizeChanged(object sender, ScalableVector2 size)
        {
            Layout.Size = size;
            HeaderLayout.Size = new ScalableVector2(Math.Max(1, Width - Config.Padding * 2 - MapViewButtonSize - Config.ButtonGap), Config.HeaderHeight);

            LayoutModifierIcons();
            Layout.RefreshLayout();
        }

        private void OnContentSizeChanged(object sender, ScalableVector2 size)
        {
            LeaderboardContent.Size = size;
            ModifiersContent.Size = size;
            MapPreviewContent.Size = size;
            LeaderboardPanel.Size = size;
            ModifiersPanel.Size = size;
        }

        private void OnFooterSizeChanged(object sender, ScalableVector2 size)
        {
            LeaderboardFooterContent.Size = size;
            ModifiersFooterContent.Size = size;
        }

        private void OnActivePanelChanged(object sender,
            BindableValueChangedEventArgs<SelectContainerPanel> args) => RefreshSelectedPanel();

        private void OnModsChanged(object sender, ModsChangedEventArgs args) => RefreshModifierIcons();

        private void RefreshSelectedPanel()
        {
            var leaderboardSelected = ActivePanel.Value == SelectContainerPanel.Leaderboard;
            var modifiersSelected = ActivePanel.Value == SelectContainerPanel.Modifiers;
            var mapPreviewSelected = ActivePanel.Value == SelectContainerPanel.MapPreview;

            LeaderboardButton.Tint = SkinV2Color.Parse(leaderboardSelected ? Config.SelectedButtonColor : Config.UnselectedButtonColor);
            ModifiersButton.Tint = SkinV2Color.Parse(modifiersSelected ? Config.SelectedButtonColor : Config.UnselectedButtonColor);
            MapViewButton.Tint = SkinV2Color.Parse(mapPreviewSelected ? Config.SelectedButtonColor : Config.UnselectedButtonColor);

            LeaderboardContent.Visible = leaderboardSelected;
            LeaderboardContent.UpdateWhenInvisible = leaderboardSelected;
            LeaderboardPanel.SetActive(leaderboardSelected);

            ModifiersContent.Visible = modifiersSelected;
            ModifiersContent.UpdateWhenInvisible = modifiersSelected;

            MapPreviewContent.Visible = mapPreviewSelected;
            MapPreviewContent.UpdateWhenInvisible = mapPreviewSelected;

            var footerHeight = leaderboardSelected
                ? Config.LeaderboardFooterHeight
                : modifiersSelected ? Config.ModifiersFooterHeight : 0;
            var footerVisible = footerHeight > 0;
            Layout.SetItemOptions(Content, leaderboardSelected
                ? new FlexItemOptions { Basis = Config.LeaderboardContentHeight, Grow = 1, Shrink = 1 }
                : new FlexItemOptions { Basis = 0, Grow = 1, Shrink = 1 });
            
            Layout.SetItemOptions(FooterGap, new FlexItemOptions { Basis = footerVisible ? Config.Gap : 0, Shrink = 0 });
            Layout.SetItemOptions(Footer, new FlexItemOptions { Basis = footerHeight, Shrink = 0 });

            Footer.Visible = footerVisible;
            Footer.UpdateWhenInvisible = footerVisible;

            LeaderboardFooterContent.Visible = leaderboardSelected;
            LeaderboardFooterContent.UpdateWhenInvisible = leaderboardSelected;

            ModifiersFooterContent.Visible = modifiersSelected;
            ModifiersFooterContent.UpdateWhenInvisible = modifiersSelected;

            Layout.RefreshLayout();
        }

        public override void Destroy()
        {
            ActivePanel.ValueChanged -= OnActivePanelChanged;
            ModManager.ModsChanged -= OnModsChanged;

            base.Destroy();
        }
    }
}
