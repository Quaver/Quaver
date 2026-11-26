using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Shaders;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Selection.UI.List
{
    /// <summary>
    ///     Manages song select scrolling, row pooling, selection and mapset expansion
    /// </summary>
    internal sealed class MapsetList : PoolableScrollContainer<SelectionListEntry>
    {
        private const int ExpansionAnimationMilliseconds = 240;

        private SkinV2SelectionListConfig Config { get; }
        private SkinV2SelectionRowConfig MapsetConfig { get; }
        private SkinV2SelectionRowConfig DifficultyConfig { get; }
        private Texture2D MapsetBackground { get; }
        private Texture2D DifficultyBackground { get; }
        private Func<bool> CanHandleInput { get; }
        private Action<Mapset> SelectMapset { get; }
        private Action<Map> SelectMap { get; }
        private List<Mapset> Mapsets { get; set; } = new();
        private Map SelectedMap { get; set; }
        private Mapset SelectedMapset { get; set; }
        private Mapset ExpandedMapset { get; set; }
        private Dictionary<Mapset, MapsetExpansion> Expansions { get; } = new();
        private Dictionary<Mapset, int> MapsetIndices { get; } = new();
        private List<SelectionListEntry> VisibleEntries { get; } = new();
        private List<MapsetRow> MapsetRows { get; } = new();
        private List<DifficultyRow> DifficultyRows { get; } = new();
        private int VisibleRowCount { get; set; }
        private int MapsetPoolCapacity { get; set; }
        private int DifficultyPoolCapacity { get; set; }
        private List<Mapset> FinishedExpansions { get; } = new();
        private HashSet<string> PendingBanners { get; } = new();

        internal RoundedTextureCache BannerCache { get; } = new(360, 80, 6);
        internal SkinV2SelectionRowConfig MapsetRowConfig => MapsetConfig;
        internal SkinV2SelectionRowConfig DifficultyRowConfig => DifficultyConfig;
        internal bool IsInitialized { get; private set; }
        internal bool CanInteract => IsInitialized && CanHandleInput() && AvailableItems.Count > 0;
        internal int RowGap => Config.Gap;
        internal float DifficultyLongNoteStatWidth { get; private set; }
        internal float DifficultyNotesPerSecondStatWidth { get; private set; }
        internal float DifficultyDurationStatWidth { get; private set; }
        internal float DifficultyBpmStatWidth { get; private set; }
        internal bool ShowDifficultyTimingStats { get; private set; }

        internal MapsetList(SkinV2SelectionListConfig config, SkinV2SelectionRowConfig mapsetConfig, SkinV2SelectionRowConfig difficultyConfig,
            Func<bool> canHandleInput, Action<Mapset> selectMapset, Action<Map> selectMap)
            : base(new List<SelectionListEntry>(), 1, 0, new ScalableVector2(1, 1), new ScalableVector2(1, 1))
        {
            Config = config;
            MapsetConfig = mapsetConfig;
            MapsetBackground = CreateRowBackground(mapsetConfig);

            DifficultyConfig = difficultyConfig;
            DifficultyBackground = difficultyConfig.CornerRadius == mapsetConfig.CornerRadius ? MapsetBackground : CreateRowBackground(difficultyConfig);

            CanHandleInput = canHandleInput;
            SelectMapset = selectMapset;
            SelectMap = selectMap;

            CapturesMouseWheelInput = true;
            ScrollSpeed = config.ScrollSpeed;
            TimeToCompleteScroll = config.ScrollMilliseconds;
            EasingType = Easing.OutQuint;

            V2Scrollbar.Apply(this, config.ScrollbarWidth, SkinV2Color.Parse(config.ScrollbarColor), cornerRadius: config.ScrollbarWidth / 2f);
            SizeChanged += OnSizeChanged;
            BackgroundHelper.BannerLoaded += OnBannerLoaded;
            ModManager.ModsChanged += OnModsChanged;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsInitialized)
                return;

            UpdateExpansions(gameTime);
            InputEnabled = CanInteract && IsHovered();
            base.Update(gameTime);
        }

        public override void DrawToSpriteBatch()
        {
        }

        internal void Initialize(ScalableVector2 viewportSize, List<Mapset> mapsets, Map selected)
        {
            if (IsInitialized)
                throw new InvalidOperationException("The mapset list has already been initialized.");

            Size = viewportSize;
            SetMapsets(mapsets, selected);
            IsInitialized = true;
        }

        internal void SetMapsets(List<Mapset> mapsets, Map selected)
        {
            Mapsets = mapsets;
            SelectedMap = selected;
            SelectedMapset = FindMapset(selected);
            if (ExpandedMapset != SelectedMapset || !Mapsets.Contains(ExpandedMapset))
                ExpandedMapset = null;

            ClearExpansions();
            MapsetIndices.Clear();
            AvailableItems = new List<SelectionListEntry>(mapsets.Count);
            for (var i = 0; i < mapsets.Count; i++)
            {
                MapsetIndices.Add(mapsets[i], i);
                AvailableItems.Add(new SelectionListEntry(mapsets[i]));
            }

            if (ExpandedMapset != null)
            {
                SetExpandedMapset(ExpandedMapset);
                var expansion = Expansions[ExpandedMapset];
                expansion.Height = ExpandedMapset.Maps.Count * (DifficultyConfig.Height + RowGap);
                expansion.Animation = null;
            }

            RecalculateContainerHeight();
            SetScrollImmediately(GetSelectedScrollPosition());
            RefreshPool();
        }

        internal void SetSelectedMap(Map selected, bool animate)
        {
            if (SelectedMap == selected)
                return;

            var previousMapset = SelectedMapset;

            var mapset = FindMapset(selected);
            SelectedMap = selected;
            SelectedMapset = mapset;

            if (ExpandedMapset != null && ExpandedMapset != SelectedMapset)
                SetExpandedMapset(null);

            if (previousMapset == SelectedMapset && ExpandedMapset == null)
                return;

            if (animate)
                ScrollTo(GetSelectedScrollPosition(), Config.ScrollMilliseconds);
            else
            {
                SetScrollImmediately(GetSelectedScrollPosition());
                HandlePoolShifting();
            }
        }

        internal SkinV2SelectionRowConfig GetRowConfig(SelectionListEntry entry) => entry.IsDifficulty ? DifficultyConfig : MapsetConfig;

        internal Texture2D GetRowBackground(SelectionListEntry entry) => entry.IsDifficulty ? DifficultyBackground : MapsetBackground;

        internal Texture2D GetCachedBanner(Mapset mapset)
        {
            if (mapset == null || string.IsNullOrEmpty(mapset.Directory))
                return null;

            return BackgroundHelper.MapsetBanners.TryGetValue(mapset.Directory, out var banner) && banner != null && !banner.IsDisposed ? banner : null;
        }

        internal Texture2D RequestBanner(Mapset mapset)
        {
            var banner = GetCachedBanner(mapset);
            if (banner != null)
                return banner;

            if (mapset != null && !string.IsNullOrEmpty(mapset.Directory) && PendingBanners.Add(mapset.Directory))
                BackgroundHelper.LoadMapsetBanner(mapset);

            return null;
        }

        private void OnBannerLoaded(object sender, BannerLoadedEventArgs args)
        {
            if (IsDisposed || args.Mapset == null)
                return;

            var directory = args.Mapset.Directory;
            AddScheduledUpdate(() =>
            {
                if (IsDisposed)
                    return;

                PendingBanners.Remove(directory);
                if (args.Banner == null || args.Banner.IsDisposed)
                    return;

                foreach (var row in MapsetRows)
                    row.SetBanner(directory, args.Banner);
            });
        }

        private void OnModsChanged(object sender, ModsChangedEventArgs args) => AddScheduledUpdate(() =>
        {
            if (IsDisposed)
                return;

            UpdateDifficultyStatWidths(ExpandedMapset);

            foreach (var row in MapsetRows)
                row.RefreshDynamicContent();

            foreach (var row in DifficultyRows)
                row.RefreshDynamicContent();
        });

        private void UpdateDifficultyStatWidths(Mapset mapset)
        {
            if (mapset?.Maps == null || mapset.Maps.Count == 0)
                return;

            var font = FontManager.GetWobbleFont(DifficultyConfig.Font);
            var previousFontSize = font.FontSize;
            font.FontSize = DifficultyConfig.DifficultyStatFontSize;

            try
            {
                ShowDifficultyTimingStats = mapset.Maps.Select(map => map.SongLength).Distinct().Count() > 1 ||
                                            mapset.Maps.Select(map => map.Bpm).Distinct().Count() > 1;
                
                var longNoteValueWidth = mapset.Maps.Max(map => font.Store.MeasureString(DifficultyRow.GetLongNoteStatText(map)).X);
                var notesPerSecondValueWidth = mapset.Maps.Max(map => font.Store.MeasureString(DifficultyRow.GetNotesPerSecondStatText(map)).X);
                
                DifficultyLongNoteStatWidth = DifficultyConfig.DifficultyStatIconSize + DifficultyConfig.DifficultyStatItemGap + (float) Math.Ceiling(longNoteValueWidth);
                DifficultyNotesPerSecondStatWidth = DifficultyConfig.DifficultyStatIconSize + DifficultyConfig.DifficultyStatItemGap + (float) Math.Ceiling(notesPerSecondValueWidth);
                
                if (ShowDifficultyTimingStats)
                {
                    var durationValueWidth = mapset.Maps.Max(map => font.Store.MeasureString(DifficultyRow.GetDurationStatText(map)).X);
                    DifficultyDurationStatWidth = DifficultyConfig.DifficultyStatIconSize + DifficultyConfig.DifficultyStatItemGap + (float) Math.Ceiling(durationValueWidth);
                    
                    var bpmValueWidth = mapset.Maps.Max(map => font.Store.MeasureString(DifficultyRow.GetBpmStatText(map)).X);
                    DifficultyBpmStatWidth = DifficultyConfig.DifficultyStatIconSize + DifficultyConfig.DifficultyStatItemGap + (float) Math.Ceiling(bpmValueWidth);
                }
            }
            finally
            {
                font.FontSize = previousFontSize;
            }
        }

        private static Texture2D CreateRowBackground(SkinV2SelectionRowConfig config)
        {
            var margin = (int)Math.Ceiling(config.CornerRadius);
            var textureSize = Math.Max(2, margin * 2 + 2);

            return RoundedRectTextureCache.Get(textureSize, textureSize, config.CornerRadius);
        }

        internal bool IsSelected(SelectionListEntry entry) => entry.IsDifficulty ? entry.Difficulty == SelectedMap : entry.Mapset == SelectedMapset;

        internal bool IsInteractive(SelectionListEntry entry) =>
            !entry.IsDifficulty || entry.Mapset == ExpandedMapset && Expansions[entry.Mapset].Animation == null;

        internal void Select(SelectionListEntry entry)
        {
            if (!CanInteract || !IsInteractive(entry))
                return;

            if (entry.IsDifficulty)
            {
                SelectMap(entry.Difficulty);
                if (entry.Difficulty == SelectedMap)
                    ScrollTo(GetSelectedScrollPosition(), Config.ScrollMilliseconds);
            }
            else if (entry.Mapset == SelectedMapset)
            {
                // A row click occurs while the pool is updatin, so we rebind its rows on the next update
                AddScheduledUpdate(() =>
                {
                    if (!IsDisposed && CanInteract && entry.Mapset == SelectedMapset)
                    {
                        SetScrollImmediately(ContentContainer.Y);
                        SetExpandedMapset(ExpandedMapset == entry.Mapset ? null : entry.Mapset);
                    }
                });
            }
            else
                SelectMapset(entry.Mapset);
        }

        internal bool TryMoveDifficultySelection(int direction)
        {
            if (!CanInteract || ExpandedMapset == null || ExpandedMapset != SelectedMapset)
                return false;

            if (Expansions[ExpandedMapset].Animation != null)
                return true;

            var index = ExpandedMapset.Maps.IndexOf(SelectedMap);
            var next = index + direction;
            if (index >= 0 && next >= 0 && next < ExpandedMapset.Maps.Count)
                SelectMap(ExpandedMapset.Maps[next]);

            return true;
        }

        protected override PoolableSprite<SelectionListEntry> CreateObject(SelectionListEntry item, int index) =>
            item.IsDifficulty
                ? new DifficultyRow(this, item, index)
                : new MapsetRow(this, item, index);

        /// <summary>
        ///     Gets a mapset's current vertical position, including animated expansions above it
        /// </summary>
        protected override float GetItemOffset(int index)
        {
            var offset = (float)index * (MapsetConfig.Height + RowGap);
            foreach (var expansion in Expansions.Values)
            {
                if (expansion.MapsetIndex < index)
                    offset += expansion.Height;
            }

            return offset;
        }

        /// <summary>
        ///     Refreshes the visible row pool after scrolling changes the viewport
        /// </summary>
        protected override void HandlePoolShifting() => RefreshVisibleRows();

        /// <summary>
        ///     Finds the mapset at or immediately above a vertical content position
        /// </summary>
        private int FindMapsetAtOffset(float offset)
        {
            var low = 0;
            var high = Mapsets.Count - 1;

            while (low <= high)
            {
                var middle = low + (high - low) / 2;
                if (GetItemOffset(middle) <= offset)
                    low = middle + 1;
                else
                    high = middle - 1;
            }

            return Math.Max(0, high);
        }

        /// <summary>
        ///     Updates the scrollable height and each rows's expansion
        /// </summary>
        public override void RecalculateContainerHeight(bool usePoolCount = false)
        {
            var contentHeight = Math.Max(0, (float)Mapsets.Count * (MapsetConfig.Height + RowGap) - RowGap);
            foreach (var expansion in Expansions.Values)
            {
                contentHeight += expansion.Height;
                expansion.Viewport.Y = GetItemOffset(expansion.MapsetIndex) + MapsetConfig.Height + RowGap;
                expansion.Viewport.Width = Width;
                expansion.Viewport.Height = Math.Max(0, expansion.Height - RowGap);
                expansion.Viewport.Visible = expansion.Viewport.Height > 0;
            }

            ContentContainer.Height = Math.Max(Height, contentHeight);
        }

        private Mapset FindMapset(Map selected) => selected == null ? null : Mapsets.Find(set => set.Maps.Contains(selected));

        private int FindMapsetIndex(Mapset mapset) => mapset != null && MapsetIndices.TryGetValue(mapset, out var index) ? index : -1;

        /// <summary>
        ///     Opens the requested mapset and collapses every other active expansion
        /// </summary>
        private void SetExpandedMapset(Mapset mapset)
        {
            ExpandedMapset = mapset;
            UpdateDifficultyStatWidths(mapset);

            if (mapset != null && !Expansions.ContainsKey(mapset))
                Expansions.Add(mapset, new MapsetExpansion(MapsetIndices[mapset], ContentContainer));

            foreach (var entry in Expansions)
            {
                var expansion = entry.Value;
                var target = entry.Key == mapset ? mapset.Maps.Count * (DifficultyConfig.Height + RowGap) : 0;
                if (expansion.Animation?.End == target)
                    continue;

                expansion.Animation = new Animation(AnimationProperty.Height, Easing.OutCubic, expansion.Height, target, ExpansionAnimationMilliseconds);
            }
        }

        /// <summary>
        ///     Removes all expansion state and returns pooled rows to the main content container
        /// </summary>
        private void ClearExpansions()
        {
            Pool?.ForEach(row => row.Parent = ContentContainer);
            foreach (var expansion in Expansions.Values)
                expansion.Viewport.Destroy();

            Expansions.Clear();
        }

        /// <summary>
        ///     Advances expansion animations and keeps layout, scrolling and visible rows synchronized
        /// </summary>
        private void UpdateExpansions(GameTime gameTime)
        {
            var anchorIndex = FindMapsetIndex(SelectedMapset);
            var anchorOffset = anchorIndex < 0 ? 0 : GetItemOffset(anchorIndex);
            var changed = false;
            FinishedExpansions.Clear();

            foreach (var entry in Expansions)
            {
                var expansion = entry.Value;
                if (expansion.Animation == null)
                    continue;

                changed = true;
                expansion.Height = expansion.Animation.PerformInterpolation(gameTime);
                if (!expansion.Animation.Done)
                    continue;

                expansion.Height = expansion.Animation.End;
                expansion.Animation = null;
                if (entry.Key != ExpandedMapset)
                    FinishedExpansions.Add(entry.Key);
            }

            if (!changed)
                return;

            RecalculateContainerHeight();
            if (anchorIndex >= 0)
                OffsetScroll(anchorOffset - GetItemOffset(anchorIndex));

            RefreshVisibleRows();
            foreach (var mapset in FinishedExpansions)
            {
                Expansions[mapset].Viewport.Destroy();
                Expansions.Remove(mapset);
            }
        }

        /// <summary>
        ///     Moves the current and target scroll positions together to preserve the selected row's anchor
        /// </summary>
        private void OffsetScroll(float offset)
        {
            if (Math.Abs(offset) < 0.001f)
                return;

            var animation = ContentContainer.Animations.Find(item => item.Properties == AnimationProperty.Y && !item.Done);
            var remaining = animation == null ? 0 : (int)Math.Ceiling(animation.Time - animation.CurrentAnimationTime);
            var minimum = Math.Min(0, Height - ContentContainer.Height);
            var target = MathHelper.Clamp(TargetY + offset, minimum, 0);
            SetScrollImmediately(MathHelper.Clamp(ContentContainer.Y + offset, minimum, 0));
            if (remaining > 0)
                ScrollTo(target, remaining);
        }

        /// <summary>
        ///     Parents a pooled row to the main list or its mapset's clipped difficulty area
        /// </summary>
        internal void AttachRow(SelectionListRow row)
        {
            row.Parent = row is DifficultyRow ? Expansions[row.Item.Mapset].Viewport : ContentContainer;
            PositionRow(row);
        }

        /// <summary>
        ///     Positions a row and start the difficulty expansion animations
        /// </summary>
        private void PositionRow(SelectionListRow row)
        {
            var alpha = 1f;
            if (row is DifficultyRow)
            {
                var expansion = Expansions[row.Item.Mapset];
                var offset = row.Item.DifficultyIndex * (DifficultyConfig.Height + RowGap);
                alpha = MathHelper.Clamp((expansion.Height - offset) / (DifficultyConfig.Height + RowGap), 0, 1);
                row.Y = offset - DifficultyConfig.Height * (1 - alpha);
            }
            else
                row.Y = GetItemOffset(row.Index);

            if (row.Alpha != alpha)
                row.Alpha = alpha;
        }

        /// <summary>
        ///     Gets the scroll position that centers the selected mapset or visible difficulty
        /// </summary>
        private float GetSelectedScrollPosition()
        {
            var index = FindMapsetIndex(SelectedMapset);
            if (index < 0)
                return 0;

            var offset = GetItemOffset(index);
            var height = MapsetConfig.Height;
            var difficulty = ExpandedMapset == SelectedMapset ? SelectedMapset.Maps.IndexOf(SelectedMap) : -1;
            if (difficulty >= 0)
            {
                offset += MapsetConfig.Height + RowGap + difficulty * (DifficultyConfig.Height + RowGap);
                height = DifficultyConfig.Height;
            }

            return MathHelper.Clamp(Height / 2f - offset - height / 2f, Math.Min(0, Height - ContentContainer.Height), 0);
        }

        /// <summary>
        ///     Stops scroll animation and applies a scroll position immediately
        /// </summary>
        private void SetScrollImmediately(float y)
        {
            ContentContainer.ClearAnimations();
            ContentContainer.Y = TargetY = PreviousTargetY = PreviousContentContainerY = y;
        }

        private void OnSizeChanged(object sender, ScalableVector2 size)
        {
            if (IsInitialized && !IsDisposed && ContentContainer.Parent == ContentViewport)
                RefreshPool();
        }

        /// <summary>
        ///     Recalculates pool capacity and visible rows after the list size changes
        /// </summary>
        private void RefreshPool()
        {
            ContentContainer.Width = Width;
            var overscanRows = Math.Max(2, Config.OverscanRows) + 2;
            MapsetPoolCapacity = Math.Max(1, (int) Math.Ceiling(Height / Math.Max(1, MapsetConfig.Height + RowGap)) + overscanRows);
            DifficultyPoolCapacity = Math.Max(1, (int) Math.Ceiling(Height / Math.Max(1, DifficultyConfig.Height + RowGap)) + overscanRows);
            PoolSize = MapsetPoolCapacity + DifficultyPoolCapacity;
            
            RecalculateContainerHeight();

            // Resize may make the previous scroll offset invalid. Stop the old animation before clamping
            SetScrollImmediately(MathHelper.Clamp(ContentContainer.Y, Math.Min(0, Height - ContentContainer.Height), 0));
            RefreshVisibleRows(true);
        }

        /// <summary>
        ///     Finds the rows needed at the current scroll position, as well as the ones that might be above and below the screen
        /// </summary>
        private void CollectVisibleRows()
        {
            VisibleEntries.Clear();
            if (Mapsets.Count == 0)
                return;

            var minimumStride = Math.Max(1, Math.Min(MapsetConfig.Height, DifficultyConfig.Height) + RowGap);
            var overscan = Math.Max(2, Config.OverscanRows) / 2 * minimumStride;
            var top = -ContentContainer.Y - overscan;
            var bottom = -ContentContainer.Y + Height + overscan;
            var firstMapset = FindMapsetAtOffset(top);

            for (var i = firstMapset; i < Mapsets.Count; i++)
            {
                var offset = GetItemOffset(i);
                if (offset > bottom)
                    break;

                if (offset + MapsetConfig.Height >= top)
                    VisibleEntries.Add(AvailableItems[i]);

                if (!Expansions.TryGetValue(Mapsets[i], out var expansion) || expansion.Viewport.Height <= 0)
                    continue;

                var groupTop = expansion.Viewport.Y;
                if (groupTop > bottom || groupTop + expansion.Viewport.Height < top)
                    continue;

                var stride = DifficultyConfig.Height + RowGap;
                var first = Math.Max(0, (int)Math.Floor((top - groupTop) / stride));
                var last = Math.Min(Mapsets[i].Maps.Count - 1,
                    (int)Math.Ceiling(Math.Min(expansion.Height, bottom - groupTop + DifficultyConfig.Height) / stride) - 1);

                for (var difficulty = first; difficulty <= last; difficulty++)
                    VisibleEntries.Add(new SelectionListEntry(Mapsets[i], difficulty));
            }
        }

        /// <summary>
        ///     Reuses pooled rows for the collected entries and removes rows that are no longer visible
        /// </summary>
        private void RefreshVisibleRows(bool force = false)
        {
            CollectVisibleRows();

            var unchanged = !force && VisibleRowCount == VisibleEntries.Count;
            for (var i = 0; unchanged && i < VisibleEntries.Count; i++)
                unchanged = Pool[i].Item.HasSameItem(VisibleEntries[i]);

            if (unchanged)
            {
                for (var i = 0; i < VisibleEntries.Count; i++)
                    PositionRow((SelectionListRow)Pool[i]);

                return;
            }

            var availableMapsetRows = new List<MapsetRow>(MapsetRows);
            var availableDifficultyRows = new List<DifficultyRow>(DifficultyRows);
            var activeRows = new SelectionListRow[VisibleEntries.Count];

            // Preserve rows already bound to the same entry so width and selection animations keep running
            for (var i = 0; i < VisibleEntries.Count; i++)
            {
                var item = VisibleEntries[i];
                SelectionListRow row = item.IsDifficulty
                    ? TakeMatchingRow(availableDifficultyRows, item)
                    : TakeMatchingRow(availableMapsetRows, item);

                activeRows[i] = row;
            }

            for (var i = 0; i < VisibleEntries.Count; i++)
            {
                if (activeRows[i] != null)
                    continue;

                var item = VisibleEntries[i];
                if (item.IsDifficulty)
                {
                    activeRows[i] = availableDifficultyRows.Count > 0
                        ? TakeFirst(availableDifficultyRows)
                        : CreateDifficultyRow(item);
                }
                else
                {
                    activeRows[i] = availableMapsetRows.Count > 0
                        ? TakeFirst(availableMapsetRows)
                        : CreateMapsetRow(item);
                }
            }

            for (var i = 0; i < activeRows.Length; i++)
            {
                var item = VisibleEntries[i];
                var row = activeRows[i];
                row.SetActive(true);
                row.UpdateContent(item, MapsetIndices[item.Mapset]);
            }

            DeactivateRows(availableMapsetRows);
            DeactivateRows(availableDifficultyRows);

            TrimPool(MapsetRows, availableMapsetRows, MapsetPoolCapacity);
            TrimPool(DifficultyRows, availableDifficultyRows, DifficultyPoolCapacity);

            Pool = new List<PoolableSprite<SelectionListEntry>>(MapsetRows.Count + DifficultyRows.Count);
            Pool.AddRange(activeRows);
            Pool.AddRange(availableMapsetRows);
            Pool.AddRange(availableDifficultyRows);

            VisibleRowCount = VisibleEntries.Count;
        }

        private T TakeMatchingRow<T>(List<T> rows, SelectionListEntry item) where T : SelectionListRow
        {
            var index = rows.FindIndex(row => row.Item.HasSameItem(item));
            if (index < 0)
                return null;

            var row = rows[index];
            rows.RemoveAt(index);

            return row;
        }

        private static T TakeFirst<T>(List<T> rows)
        {
            var row = rows[0];
            rows.RemoveAt(0);

            return row;
        }

        private MapsetRow CreateMapsetRow(SelectionListEntry item)
        {
            var row = new MapsetRow(this, item, MapsetIndices[item.Mapset])
            {
                DestroyIfParentIsNull = false
            };
            MapsetRows.Add(row);
            AddContainedDrawable(row);

            return row;
        }

        private DifficultyRow CreateDifficultyRow(SelectionListEntry item)
        {
            var row = new DifficultyRow(this, item, MapsetIndices[item.Mapset])
            {
                DestroyIfParentIsNull = false
            };
            DifficultyRows.Add(row);
            AddContainedDrawable(row);

            return row;
        }

        private void DeactivateRows<T>(List<T> rows) where T : SelectionListRow
        {
            foreach (var row in rows)
            {
                row.SetActive(false);
                row.Parent = ContentContainer;
            }
        }

        private static void TrimPool<T>(List<T> pool, List<T> inactiveRows, int capacity)
            where T : SelectionListRow
        {
            while (pool.Count > capacity && inactiveRows.Count > 0)
            {
                var row = inactiveRows[^1];
                inactiveRows.RemoveAt(inactiveRows.Count - 1);
                pool.Remove(row);
                row.Destroy();
            }
        }

        /// <summary>
        ///     Stores the animated height and clipping viewport for one expanded difficulty group
        /// </summary>
        private sealed class MapsetExpansion
        {
            public HorizontalClippingContainer Viewport { get; }
            public int MapsetIndex { get; }
            public float Height { get; set; }
            public Animation Animation { get; set; }

            public MapsetExpansion(int mapsetIndex, Container parent)
            {
                MapsetIndex = mapsetIndex;
                Viewport = new HorizontalClippingContainer
                {
                    Parent = parent,
                    Visible = false,
                    UpdateWhenInvisible = false
                };
            }
        }

        public override void Destroy()
        {
            BackgroundHelper.BannerLoaded -= OnBannerLoaded;
            ModManager.ModsChanged -= OnModsChanged;

            PendingBanners.Clear();
            SizeChanged -= OnSizeChanged;
            base.Destroy();
            BannerCache.Dispose();
        }
    }
}
