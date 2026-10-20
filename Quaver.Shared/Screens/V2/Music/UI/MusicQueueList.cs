using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Screens.V2.UI;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;

namespace Quaver.Shared.Screens.V2.Music.UI
{
    internal sealed class MusicQueueList : PoolableScrollContainer<Mapset>
    {
        private SkinV2MusicQueueListConfig ListConfig { get; }

        private SkinV2MusicQueueRowConfig RowConfig { get; }

        private Action<Mapset> Selected { get; }

        private float ItemStride => RowConfig.Height + ListConfig.RowGap;

        public MusicQueueList(ScalableVector2 size, SkinV2MusicQueueListConfig listConfig, SkinV2MusicQueueRowConfig rowConfig, Action<Mapset> selected)
            : base(new List<Mapset>(), CalculatePoolSize(size.Y.Value, rowConfig.Height, listConfig.RowGap), 0, size, size)
        {
            ListConfig = listConfig;
            RowConfig = rowConfig;
            Selected = selected;
            Tint = Color.Transparent;
            PaddingBottom = (int) Math.Round(ListConfig.ContentPadding);
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 220;
            InputEnabled = true;
            V2Scrollbar.Apply(this);
            CreatePool();
        }

        public override void Update(GameTime gameTime)
        {
            InputEnabled = IsHovered();
            base.Update(gameTime);
        }

        public void SetItems(List<Mapset> items, bool focusCurrent = false)
        {
            DestroyPool();
            AvailableItems = items ?? new List<Mapset>();
            PoolStartingIndex = 0;
            ContentContainer.Animations.Clear();
            ContentContainer.Y = 0;
            TargetY = 0;
            PreviousTargetY = 0;
            PreviousContentContainerY = 0;
            CreatePool();

            if (focusCurrent)
                FocusCurrentSong();
        }

        public void ApplySize(ScalableVector2 size)
        {
            Size = size;
            var requiredPoolSize = CalculatePoolSize(size.Y.Value, RowConfig.Height, ListConfig.RowGap);
            if (requiredPoolSize > PoolSize)
            {
                PoolSize = requiredPoolSize;
                SetItems(AvailableItems.ToList());
                return;
            }

            RecalculateContainerHeight();
        }

        public void RefreshSelection()
        {
            foreach (var row in Pool.OfType<MusicQueueRow>())
                row.RefreshSelection();
        }

        public override void RecalculateContainerHeight(bool usePoolCount = false)
        {
            var count = usePoolCount ? Pool.Count : AvailableItems.Count;
            var contentHeight = count == 0 ? Height : RowConfig.Height * count + ListConfig.RowGap * Math.Max(0, count - 1) + PaddingBottom;
            ContentContainer.Height = Math.Max(Height, contentHeight);
        }

        protected override float GetItemOffset(int index) => index * ItemStride;

        protected override int GetMiddleObjectIndex() => (int) ((-ContentContainer.Y + Height / 2 - PaddingTop) / ItemStride);

        protected override PoolableSprite<Mapset> CreateObject(Mapset item, int index) => new MusicQueueRow(this, item, index, RowConfig, Selected);

        private void FocusCurrentSong()
        {
            var selected = MapManager.Selected?.Value;
            if (selected == null)
                return;

            var index = AvailableItems.FindIndex(mapset => mapset.Maps.Contains(selected));
            if (index < 0)
                return;

            var centeredY = Height / 2f - RowConfig.Height / 2f - GetItemOffset(index);
            centeredY = MathHelper.Clamp(centeredY, Math.Min(0, Height - ContentContainer.Height), 0);
            ContentContainer.Y = centeredY;
            TargetY = centeredY;
            PreviousTargetY = centeredY;

            HandlePoolShifting();
            PreviousContentContainerY = centeredY;
        }

        private static int CalculatePoolSize(float height, float rowHeight, float rowGap) => Math.Max(1, (int) Math.Ceiling(Math.Max(1, height) / Math.Max(1, rowHeight + rowGap)) + 3);
    }
}
