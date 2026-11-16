using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;

namespace Quaver.Shared.Screens.V2.Selection.UI
{
    internal sealed class SelectionMapsetList : PoolableScrollContainer<Mapset>
    {
        private SkinV2SelectionListConfig Config { get; }
        private SkinV2SelectionMapsetConfig RowConfig { get; }
        private Func<bool> CanHandleInput { get; }
        private Action<Mapset> SelectMapset { get; }

        internal bool IsInitialized { get; private set; }
        internal int SelectedIndex { get; private set; } = -1;
        internal bool CanInteract => IsInitialized && CanHandleInput() && AvailableItems.Count > 0;
        internal int RowStride => Math.Max(1, RowConfig.Height + RowConfig.Gap);

        internal SelectionMapsetList(SkinV2SelectionListConfig config, SkinV2SelectionMapsetConfig rowConfig, Func<bool> canHandleInput, Action<Mapset> selectMapset)
            : base(new List<Mapset>(), 1, 0, new ScalableVector2(1, 1), new ScalableVector2(1, 1))
        {
            Config = config;
            RowConfig = rowConfig;
            CanHandleInput = canHandleInput;
            SelectMapset = selectMapset;

            CapturesMouseWheelInput = true;
            ScrollSpeed = config.ScrollSpeed;
            TimeToCompleteScroll = config.ScrollMilliseconds;
            EasingType = Easing.OutQuint;

            V2Scrollbar.Apply(this, config.ScrollbarWidth, SkinV2Color.Parse(config.ScrollbarColor), Color.Transparent, config.ScrollbarWidth / 2f);
            ShowScrollbarBackground = false;
            ShowScrollbarBackgroundWhenContentFits = false;
            SizeChanged += OnSizeChanged;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsInitialized)
                return;

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
            AvailableItems = mapsets;
            SelectedIndex = FindSelectedIndex(selected);
            RecalculateContainerHeight();
            SetScrollImmediately(GetSelectedScrollPosition());
            RefreshPool();
        }

        internal void SetSelectedMap(Map selected, bool animate)
        {
            var index = FindSelectedIndex(selected);
            if (SelectedIndex == index)
                return;

            SelectedIndex = index;
            if (animate)
                ScrollTo(GetSelectedScrollPosition(), Config.ScrollMilliseconds);
            else
            {
                SetScrollImmediately(GetSelectedScrollPosition());
                HandlePoolShifting();
            }
        }

        internal void Select(Mapset mapset) => SelectMapset(mapset);

        protected override PoolableSprite<Mapset> CreateObject(Mapset item, int index) => new SelectionMapsetRow(this, RowConfig, item, index);

        protected override float GetItemOffset(int index) => index * RowStride;

        protected override int GetMiddleObjectIndex() => (int)Math.Floor((-ContentContainer.Y + Height / 2f) / RowStride);

        public override void RecalculateContainerHeight(bool usePoolCount = false)
        {
            var count = usePoolCount ? Pool.Count : AvailableItems.Count;
            ContentContainer.Height = Math.Max(Height, count == 0 ? 0 : count * RowStride - RowConfig.Gap);
        }

        private int FindSelectedIndex(Map selected) => selected == null ? -1 : AvailableItems.FindIndex(set => set.Maps.Contains(selected));

        private float GetSelectedScrollPosition()
        {
            var y = SelectedIndex < 0 ? 0 : Height / 2f - GetItemOffset(SelectedIndex) - RowConfig.Height / 2f;
            return MathHelper.Clamp(y, Math.Min(0, Height - ContentContainer.Height), 0);
        }

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

        private void RefreshPool()
        {
            ContentContainer.Width = Width;
            PoolSize = Math.Max(1, (int)Math.Ceiling(Height / RowStride) + Math.Max(2, Config.OverscanRows));
            RecalculateContainerHeight();

            // Resize may make the previous scroll offset invalid. Stop the old animation before clamping.
            SetScrollImmediately(MathHelper.Clamp(ContentContainer.Y, Math.Min(0, Height - ContentContainer.Height), 0));
            PoolStartingIndex = DesiredPoolStartingIndex(GetMiddleObjectIndex());

            var required = Math.Min(PoolSize, AvailableItems.Count);
            if (Pool == null || Pool.Count != required)
            {
                if (Pool != null)
                    DestroyPool();
                CreatePool();
            }
            else
            {
                for (var i = 0; i < Pool.Count; i++)
                {
                    var index = PoolStartingIndex + i;
                    Pool[i].Y = GetItemOffset(index);
                    Pool[i].UpdateContent(AvailableItems[index], index);
                }
            }
        }

        public override void Destroy()
        {
            SizeChanged -= OnSizeChanged;
            base.Destroy();
        }
    }
}
