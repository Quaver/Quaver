using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Downloading.UI
{
    /// <summary>
    ///     Search and filter header for the first V2 Download screen slice.
    /// </summary>
    internal sealed class DownloadingSearchPanel : Sprite
    {
        private DownloadingSearchState State { get; }

        private SkinV2DownloadingConfig Config { get; }

        private WobbleFontStore FieldFont { get; }

        private WobbleFontStore ButtonFont { get; }

        private Container LayoutRoot { get; set; }

        private FlexContainer TopRow { get; set; }

        private FlexContainer ExtraRow { get; set; }

        private RoundedButton ExpandButton { get; set; }

        private bool LayoutDirty { get; set; }

        private bool IsNarrow { get; set; }

        private float ExpansionProgress { get; set; }

        private float LastLayoutWidth { get; set; } = -1;

        private List<float> TopRowItemBases { get; } = new List<float>();

        private List<float> ExtraRowItemBases { get; } = new List<float>();

        private FlexItemOptions SearchItemOptions { get; set; }

        private int TopRowLineCount { get; set; } = 1;

        private int ExtraRowLineCount { get; set; } = 1;

        public DownloadingSearchPanel(float width, DownloadingSearchState state,
            SkinV2DownloadingConfig config)
        {
            State = state;
            Config = config;
            FieldFont = FontManager.GetWobbleFont(config.Field.Font);
            ButtonFont = FontManager.GetWobbleFont(config.Button.Font);
            Size = new ScalableVector2(width, config.SearchArea.CompactHeight);
            Tint = SkinV2Color.Parse(config.SearchArea.BackgroundColor);
            ExpansionProgress = state.MapsetsExpanded.Value ? 1 : 0;

            State.ActiveTab.ValueChanged += OnTabChanged;
            State.MapsetsExpanded.ValueChanged += OnExpansionChanged;
            State.ShowOwnedMapsets.ValueChanged += OnOwnedChanged;
            State.ShowOwnedPlaylists.ValueChanged += OnOwnedChanged;

            RebuildLayout();
        }

        public override void Update(GameTime gameTime)
        {
            if (LayoutDirty)
                RebuildLayout();

            UpdateResponsiveLayout();
            UpdateExpansion(gameTime);
            base.Update(gameTime);

            if (ExtraRow != null)
                ApplyAlpha(ExtraRow, ExpansionProgress);
        }

        public override void Destroy()
        {
            State.ActiveTab.ValueChanged -= OnTabChanged;
            State.MapsetsExpanded.ValueChanged -= OnExpansionChanged;
            State.ShowOwnedMapsets.ValueChanged -= OnOwnedChanged;
            State.ShowOwnedPlaylists.ValueChanged -= OnOwnedChanged;
            base.Destroy();
        }

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();
            if (Config != null)
            {
                var texture = RoundedRectTextureCache.Get(Width, Height,
                    Config.SearchArea.CornerRadius);
                if (Image != texture)
                    Image = texture;
            }
        }

        private void RebuildLayout()
        {
            LayoutDirty = false;
            LayoutRoot?.Destroy();
            TopRowItemBases.Clear();
            ExtraRowItemBases.Clear();
            SearchItemOptions = null;
            LayoutRoot = new Container
            {
                Parent = this,
                Size = Size
            };

            TopRow = CreateFlexRow(LayoutRoot);
            if (State.ActiveTab.Value == DownloadSearchTab.Mapsets)
            {
                BuildMapsetTopRow();
                ExtraRow = CreateFlexRow(LayoutRoot);
                ExtraRow.DrawOrder = 0;
                BuildMapsetExtraRow();
            }
            else
            {
                BuildPlaylistTopRow();
                ExtraRow = null;
            }

            TopRow.DrawOrder = 10;
            LastLayoutWidth = -1;
            UpdateResponsiveLayout(true);
            UpdateExpandIcon();
        }

        private FlexContainer CreateFlexRow(Drawable parent) => new FlexContainer
        {
            Parent = parent,
            Direction = FlexDirection.Row,
            Wrap = FlexWrap.Wrap,
            JustifyContent = FlexJustifyContent.FlexStart,
            AlignItems = FlexAlignItems.Center,
            AlignContent = FlexAlignContent.FlexStart,
            RowGap = Config.SearchArea.RowGap,
            ColumnGap = Config.SearchArea.ColumnGap
        };

        private void BuildMapsetTopRow()
        {
            AddSearchBox(TopRow, State.MapsetQuery, "Screen_Download_SearchMaps");
            AddFixed(TopRow, CreateDifficultyRange(), GetDifficultyRangeWidth());
            AddFixed(TopRow, CreateToggle(
                "Screen_Download_OwnedMaps", Config.Button.OwnedMapsetsWidth,
                State.ShowOwnedMapsets.Value,
                () => State.ShowOwnedMapsets.Value = !State.ShowOwnedMapsets.Value));
            AddFixed(TopRow, CreateTabs());
            AddFixed(TopRow, CreateKeymodeDropdown(), Config.Button.KeymodeWidth);
            AddFixed(TopRow, CreateRankedDropdown(), Config.Button.RankedWidth);

            ExpandButton = CreateButton(string.Empty, Config.Button.ExpandWidth, false,
                () => State.MapsetsExpanded.Value = !State.MapsetsExpanded.Value,
                GlobalIcons.Get(State.MapsetsExpanded.Value
                    ? GlobalIcon.LessOptions
                    : GlobalIcon.MoreOptions));
            AddFixed(TopRow, ExpandButton, Config.Button.ExpandWidth);
        }

        private void BuildMapsetExtraRow()
        {
            AddFixed(ExtraRow, CreateNumericPair("Screen_Download_LN",
                    State.MinimumLongNotePercentage, State.MaximumLongNotePercentage,
                    "Screen_Download_MinPercent", "Screen_Download_MaxPercent"),
                184);
            AddFixed(ExtraRow, CreateNumericPair("Screen_Download_NPS",
                    State.MinimumNotesPerSecond, State.MaximumNotesPerSecond,
                    "Screen_Download_Min", "Screen_Download_Max"),
                184);
            AddFixed(ExtraRow, CreateNumericPair("Screen_Download_BPM",
                    State.MinimumBpm, State.MaximumBpm,
                    "Screen_Download_Min", "Screen_Download_Max"),
                184);

            var spacer = new Container
            {
                Parent = ExtraRow,
                Size = new ScalableVector2(1, Config.Button.Height)
            };
            ExtraRow.SetItemOptions(spacer, new FlexItemOptions { Basis = 1, Grow = 1, Shrink = 1 });
            ExtraRowItemBases.Add(1);

            AddFixed(ExtraRow, CreateStaticSelector("Screen_Download_AnyLength",
                Config.Button.StaticSelectorWidth));
            AddFixed(ExtraRow, CreateStaticSelector("Screen_Download_AnyCombo",
                Config.Button.StaticSelectorWidth));
            AddFixed(ExtraRow, CreateStaticSelector("Screen_Download_Artist",
                Config.Button.SortWidth, GlobalIcons.Get(GlobalIcon.ReverseSortDescending)));
        }

        private void BuildPlaylistTopRow()
        {
            AddSearchBox(TopRow, State.PlaylistQuery, "Screen_Download_SearchPlaylists");
            AddFixed(TopRow, CreateKeymodeDropdown(), Config.Button.KeymodeWidth);
            AddFixed(TopRow, CreateRankedDropdown(), Config.Button.RankedWidth);
            AddFixed(TopRow, CreateToggle(
                "Screen_Download_OwnedPlaylists", Config.Button.OwnedPlaylistsWidth,
                State.ShowOwnedPlaylists.Value,
                () => State.ShowOwnedPlaylists.Value = !State.ShowOwnedPlaylists.Value));
            AddFixed(TopRow, CreateTabs());
            AddFixed(TopRow, CreateStaticSelector("Screen_Download_AnyMapCount",
                Config.Button.StaticSelectorWidth));
            AddFixed(TopRow, CreateStaticSelector("Screen_Download_Artist",
                Config.Button.SortWidth, GlobalIcons.Get(GlobalIcon.ReverseSortDescending)));
        }

        private void AddSearchBox(FlexContainer parent, Bindable<string> query, string placeholderKey)
        {
            var textbox = new DownloadingSearchQueryTextbox(query,
                LocalizationManager.Get(placeholderKey), FieldFont, Config.Field)
            {
                Parent = parent
            };
            SearchItemOptions = new FlexItemOptions
            {
                Basis = Config.Field.SearchWidth,
                Grow = 1,
                Shrink = 1
            };
            parent.SetItemOptions(textbox, SearchItemOptions);
            TopRowItemBases.Add(Config.Field.SearchWidth);
        }

        private FlexContainer CreateDifficultyRange()
        {
            var group = new FlexContainer
            {
                Size = new ScalableVector2(GetDifficultyRangeWidth(), Config.Field.Height),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Center,
                ColumnGap = Config.SearchArea.ColumnGap
            };
            var minimum = new DownloadingNumericTextbox(State.MinimumDifficulty,
                string.Empty, FieldFont, Config.Field, Config.Field.NumericWidth,
                "00.00", true, value => Math.Min(value, State.MaximumDifficulty.Value))
            {
                Parent = group
            };
            AddFixed(group, minimum, Config.Field.NumericWidth);

            var slider = new DownloadingRangeSlider(State.MinimumDifficulty,
                State.MaximumDifficulty, Config.Range)
            {
                Parent = group
            };
            AddFixed(group, slider, Config.Range.Width);

            var maximum = new DownloadingNumericTextbox(State.MaximumDifficulty,
                string.Empty, FieldFont, Config.Field, Config.Field.NumericWidth,
                "00.00", true, value => Math.Max(value, State.MinimumDifficulty.Value))
            {
                Parent = group
            };
            AddFixed(group, maximum, Config.Field.NumericWidth);
            return group;
        }

        private FlexContainer CreateNumericPair(string labelKey, BindableFloat minimum,
            BindableFloat maximum, string minimumPlaceholderKey, string maximumPlaceholderKey)
        {
            var group = new FlexContainer
            {
                Size = new ScalableVector2(184, Config.Field.Height),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Center,
                ColumnGap = Config.SearchArea.ColumnGap
            };
            var label = new SpriteTextPlus(ButtonFont, LocalizationManager.Get(labelKey),
                Config.Button.FontSize)
            {
                Parent = group,
                Tint = SkinV2Color.Parse(Config.Button.TextColor)
            };
            group.SetItemOptions(label, new FlexItemOptions { Shrink = 0 });

            var minimumTextbox = new DownloadingNumericTextbox(minimum,
                LocalizationManager.Get(minimumPlaceholderKey), FieldFont, Config.Field,
                Config.Field.NumericCompactWidth, normalize: value => Math.Min(value, maximum.Value))
            {
                Parent = group
            };
            AddFixed(group, minimumTextbox, Config.Field.NumericCompactWidth);

            var maximumTextbox = new DownloadingNumericTextbox(maximum,
                LocalizationManager.Get(maximumPlaceholderKey), FieldFont, Config.Field,
                Config.Field.NumericCompactWidth, normalize: value => Math.Max(value, minimum.Value))
            {
                Parent = group
            };
            AddFixed(group, maximumTextbox, Config.Field.NumericCompactWidth);
            return group;
        }

        private FlexContainer CreateTabs()
        {
            var tabs = new FlexContainer
            {
                Size = new ScalableVector2(GetTabsWidth(), Config.Button.Height),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Center,
                ColumnGap = 0
            };
            var mapsets = CreateButton("Screen_Download_Mapsets", Config.Button.MapsetsTabWidth,
                State.ActiveTab.Value == DownloadSearchTab.Mapsets,
                () => State.ActiveTab.Value = DownloadSearchTab.Mapsets);
            mapsets.Parent = tabs;
            AddFixed(tabs, mapsets);

            var playlists = CreateButton("Screen_Selection_Playlists",
                Config.Button.PlaylistsTabWidth,
                State.ActiveTab.Value == DownloadSearchTab.Playlists,
                () => State.ActiveTab.Value = DownloadSearchTab.Playlists);
            playlists.Parent = tabs;
            AddFixed(tabs, playlists);
            tabs.Width = mapsets.Width + playlists.Width;
            return tabs;
        }

        private DownloadingSearchDropdown<int> CreateKeymodeDropdown() =>
            new DownloadingSearchDropdown<int>(Config.Button.KeymodeWidth, State.Keymode,
                GetKeymodeOptions(), ButtonFont, Config.Button, Config.Dropdown);

        private DownloadingSearchDropdown<DownloadSearchRankedStatus> CreateRankedDropdown() =>
            new DownloadingSearchDropdown<DownloadSearchRankedStatus>(
                Config.Button.RankedWidth, State.RankedStatus, GetRankedOptions(),
                ButtonFont, Config.Button, Config.Dropdown);

        private RoundedButton CreateToggle(string localizationKey, float width, bool active,
            Action clicked) => CreateButton(localizationKey, width, active, clicked);

        private RoundedButton CreateStaticSelector(string localizationKey, float width,
            TextureRegion? icon = null) =>
            CreateButton(localizationKey, width, false, null, icon);

        private RoundedButton CreateButton(string localizationKey, float width, bool active,
            Action clicked, TextureRegion? icon = null)
        {
            var button = new RoundedButton(clicked == null
                ? null
                : (EventHandler) ((sender, args) => clicked()))
            {
                Size = new ScalableVector2(width, Config.Button.Height),
                CornerRadius = Config.Button.CornerRadius,
                Tint = SkinV2Color.Parse(active
                    ? Config.Button.ActiveColor
                    : Config.Button.BackgroundColor),
                PerformHoverFade = true
            };

            if (icon.HasValue)
                button.SetIcon(icon.Value, new Vector2(Config.Button.IconSize, Config.Button.IconSize));
            if (!string.IsNullOrEmpty(localizationKey))
                button.SetLabel(ButtonFont, LocalizationManager.Get(localizationKey),
                    Config.Button.FontSize, SkinV2Color.Parse(active
                        ? Config.Button.ActiveTextColor
                        : Config.Button.TextColor));

            if (button.Label != null)
            {
                var contentWidth = button.Label.Width + Config.Button.HorizontalPadding * 2;
                if (button.Icon != null)
                    contentWidth += button.Icon.Width + 8;
                button.Width = Math.Max(button.Width, contentWidth);
            }

            return button;
        }

        private void UpdateResponsiveLayout(bool force = false)
        {
            if (!force && Math.Abs(Width - LastLayoutWidth) < 0.001f)
                return;

            LastLayoutWidth = Width;
            IsNarrow = Width < Config.Layout.ReflowBreakpoint;
            var padding = Config.SearchArea.Padding;
            var contentWidth = Math.Max(1, Width - padding * 2);
            var searchBasis = IsNarrow
                ? Config.Field.SearchMinimumWidth
                : Config.Field.SearchWidth;
            if (SearchItemOptions != null)
                SearchItemOptions.Basis = searchBasis;
            if (TopRowItemBases.Count > 0)
                TopRowItemBases[0] = searchBasis;

            TopRowLineCount = CountWrappedLines(TopRowItemBases, contentWidth);
            ExtraRowLineCount = CountWrappedLines(ExtraRowItemBases, contentWidth);
            var compactHeight = GetCompactHeight();
            var expandedHeight = GetExpandedHeight();
            var topHeight = Math.Max(1, compactHeight - padding * 2);

            LayoutRoot.Size = new ScalableVector2(Width, Height);
            TopRow.Position = new ScalableVector2(padding, padding);
            TopRow.Size = new ScalableVector2(contentWidth, topHeight);
            TopRow.RefreshLayout();

            if (ExtraRow != null)
            {
                ExtraRow.Position = new ScalableVector2(padding,
                    padding + topHeight + Config.SearchArea.RowGap);
                ExtraRow.Size = new ScalableVector2(contentWidth,
                    Math.Max(1, expandedHeight - compactHeight - Config.SearchArea.RowGap));
                ExtraRow.RefreshLayout();
            }

            UpdatePanelHeight();
        }

        private void UpdateExpansion(GameTime gameTime)
        {
            if (State.ActiveTab.Value != DownloadSearchTab.Mapsets)
            {
                ExpansionProgress = 0;
                UpdatePanelHeight();
                return;
            }

            var target = State.MapsetsExpanded.Value ? 1f : 0f;
            if (Math.Abs(target - ExpansionProgress) > 0.001f)
            {
                var change = (float) (gameTime.ElapsedGameTime.TotalMilliseconds /
                                      Math.Max(1, Config.SearchArea.ExpansionDurationMilliseconds));
                ExpansionProgress = target > ExpansionProgress
                    ? Math.Min(target, ExpansionProgress + change)
                    : Math.Max(target, ExpansionProgress - change);
            }
            else
                ExpansionProgress = target;

            if (ExtraRow != null)
                ExtraRow.Visible = ExpansionProgress > 0.001f;
            UpdatePanelHeight();
        }

        private void UpdatePanelHeight()
        {
            var compact = GetCompactHeight();
            var targetHeight = State.ActiveTab.Value == DownloadSearchTab.Playlists
                ? compact
                : Microsoft.Xna.Framework.MathHelper.Lerp(compact, GetExpandedHeight(),
                    ExpansionProgress);
            if (Math.Abs(Height - targetHeight) > 0.001f)
            {
                Height = targetHeight;
                LayoutRoot.Height = targetHeight;
            }
        }

        private float GetCompactHeight()
        {
            var baseHeight = State.ActiveTab.Value == DownloadSearchTab.Playlists
                ? Config.SearchArea.PlaylistHeight
                : Config.SearchArea.CompactHeight;
            return baseHeight + Math.Max(0, TopRowLineCount - 1) *
                (Config.Button.Height + Config.SearchArea.RowGap);
        }

        private float GetExpandedHeight() =>
            Config.SearchArea.ExpandedHeight +
            Math.Max(0, TopRowLineCount - 1) *
            (Config.Button.Height + Config.SearchArea.RowGap) +
            Math.Max(0, ExtraRowLineCount - 1) *
            (Config.Button.Height + Config.SearchArea.RowGap);

        private int CountWrappedLines(IReadOnlyList<float> itemBases, float availableWidth)
        {
            if (itemBases.Count == 0)
                return 1;

            var lineCount = 1;
            var occupiedWidth = 0f;
            foreach (var itemBasis in itemBases)
            {
                var requiredWidth = occupiedWidth +
                    (occupiedWidth > 0 ? Config.SearchArea.ColumnGap : 0) + itemBasis;
                if (occupiedWidth > 0 && requiredWidth > availableWidth)
                {
                    lineCount++;
                    occupiedWidth = itemBasis;
                    continue;
                }

                occupiedWidth = requiredWidth;
            }

            return lineCount;
        }

        private float GetDifficultyRangeWidth() =>
            Config.Field.NumericWidth * 2 + Config.Range.Width +
            Config.SearchArea.ColumnGap * 2;

        private float GetTabsWidth() =>
            Config.Button.MapsetsTabWidth + Config.Button.PlaylistsTabWidth;

        private void UpdateExpandIcon()
        {
            if (ExpandButton == null)
                return;

            ExpandButton.SetIcon(GlobalIcons.Get(State.MapsetsExpanded.Value
                    ? GlobalIcon.LessOptions
                    : GlobalIcon.MoreOptions),
                new Vector2(Config.Button.IconSize, Config.Button.IconSize));
        }

        private void OnTabChanged(object sender, BindableValueChangedEventArgs<DownloadSearchTab> args) =>
            LayoutDirty = true;

        private void OnExpansionChanged(object sender, BindableValueChangedEventArgs<bool> args) =>
            UpdateExpandIcon();

        private void OnOwnedChanged(object sender, BindableValueChangedEventArgs<bool> args) =>
            LayoutDirty = true;

        private void AddFixed(FlexContainer parent, Drawable child, float basis)
        {
            if (child.Parent == null)
                child.Parent = parent;
            parent.SetItemOptions(child, new FlexItemOptions
            {
                Basis = basis,
                Grow = 0,
                Shrink = 0
            });

            if (parent == TopRow)
                TopRowItemBases.Add(basis);
            else if (parent == ExtraRow)
                ExtraRowItemBases.Add(basis);
        }

        private void AddFixed(FlexContainer parent, Drawable child) =>
            AddFixed(parent, child, child.Width);

        private static IReadOnlyList<KeyValuePair<int, string>> GetKeymodeOptions()
        {
            var options = new List<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>(0,
                    LocalizationManager.Get("Screen_Download_AllKeymodes"))
            };
            options.AddRange(ModeHelper.AllModes.Select(mode =>
                new KeyValuePair<int, string>((int) mode,
                    LocalizationManager.Get("Screen_Download_" +
                                            ModeHelper.ToLongHand(mode).Replace(" ", string.Empty)))));
            return options;
        }

        private static IReadOnlyList<KeyValuePair<DownloadSearchRankedStatus, string>>
            GetRankedOptions() => new[]
        {
            new KeyValuePair<DownloadSearchRankedStatus, string>(
                DownloadSearchRankedStatus.All,
                LocalizationManager.Get("Screen_Download_All")),
            new KeyValuePair<DownloadSearchRankedStatus, string>(
                DownloadSearchRankedStatus.Unranked,
                LocalizationManager.Get("Screen_Download_Unranked")),
            new KeyValuePair<DownloadSearchRankedStatus, string>(
                DownloadSearchRankedStatus.Ranked,
                LocalizationManager.Get("Screen_Download_Ranked")),
            new KeyValuePair<DownloadSearchRankedStatus, string>(
                DownloadSearchRankedStatus.ClanRanked,
                LocalizationManager.Get("Screen_Download_ClanRanked"))
        };

        private static void ApplyAlpha(Drawable drawable, float alpha)
        {
            if (drawable is DownloadingSearchTextbox textbox)
            {
                textbox.Alpha = alpha;
                textbox.InputText.Alpha = alpha;
                return;
            }

            if (drawable is Sprite sprite)
            {
                sprite.Alpha = alpha;
                return;
            }

            foreach (var child in drawable.Children)
                ApplyAlpha(child, alpha);
        }
    }
}
