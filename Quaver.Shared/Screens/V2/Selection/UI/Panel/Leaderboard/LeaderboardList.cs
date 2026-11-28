using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;

namespace Quaver.Shared.Screens.V2.Selection.UI.Panel.Leaderboard
{
    internal sealed class LeaderboardList : PoolableScrollContainer<Score>
    {
        private SkinV2SelectionLeaderboardConfig Config { get; }
        private bool UsesScrollbar { get; }
        private bool IsActive { get; set; }
        private float ItemStride => Config.RowHeight + Config.Gap;

        internal RoundedTextureCache AvatarCache { get; }
        internal LeaderboardRowsView RowsView { get; }
        internal bool ShowRank { get; }

        internal LeaderboardList(SkinV2SelectionLeaderboardConfig config, bool usesScrollbar = true, bool showRank = true) 
            : base(new List<Score>(), 1, 0, new ScalableVector2(1, 1), new ScalableVector2(1, 1))
        {
            Config = config;
            UsesScrollbar = usesScrollbar;
            ShowRank = showRank;
            AvatarCache = new RoundedTextureCache((int) Config.AvatarSize, (int) Config.AvatarSize, Config.CornerRadius);
            RowsView = new LeaderboardRowsView(ContentContainer, Config);
            Tint = Color.Transparent;
            CapturesMouseWheelInput = UsesScrollbar;
            AllowMiddleMouseDragging = UsesScrollbar;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 220;

            if (UsesScrollbar)
                V2Scrollbar.Apply(this, Config.ScrollbarWidth, SkinV2Color.Parse(Config.ScrollbarColor), SkinV2Color.Parse(Config.ScrollbarTrackColor), Config.ScrollbarWidth / 2f);

            CreatePool();
            RowsView.Resize(ContentContainer.Size);
        }

        public override void Update(GameTime gameTime)
        {
            InputEnabled = UsesScrollbar && IsActive && IsHovered();
            base.Update(gameTime);
        }

        internal void SetActive(bool active) => IsActive = active;

        /// <summary>
        ///     Rebinds all of the rows to a fetched score list
        /// </summary>
        internal void SetScores(List<Score> scores)
        {
            RebuildPool(scores ?? new List<Score>(), true);

            ContentContainer.Animations.Clear();
            ContentContainer.Y = 0;
            TargetY = 0;
            PreviousTargetY = 0;
            PreviousContentContainerY = 0;
        }

        internal void ApplySize(ScalableVector2 size)
        {
            var widthChanged = Math.Abs(Width - size.X.Value) > 0.5f;
            Size = size;
            ContentContainer.Width = UsesScrollbar ? V2Scrollbar.GetContentWidth(this) : Width;

            var requiredPoolSize = CalculatePoolSize(size.Y.Value, Config.RowHeight, Config.Gap);
            if (requiredPoolSize != PoolSize || widthChanged)
            {
                PoolSize = requiredPoolSize;
                RebuildPool(AvailableItems.ToList(), false);
                return;
            }

            RecalculateContainerHeight();
            RowsView.Resize(ContentContainer.Size);
        }

        public override void RecalculateContainerHeight(bool usePoolCount = false)
        {
            var count = usePoolCount ? Pool.Count : AvailableItems.Count;
            var height = count == 0 ? Height : Config.RowHeight * count + Config.Gap * Math.Max(0, count - 1);

            ContentContainer.Height = Math.Max(Height, height);
        }

        protected override float GetItemOffset(int index) => index * ItemStride;

        protected override int GetMiddleObjectIndex() => (int) ((-ContentContainer.Y + Height / 2f - PaddingTop) / ItemStride);

        protected override PoolableSprite<Score> CreateObject(Score item, int index) => new LeaderboardRow(this, item, index, Config);

        public override void Destroy()
        {
            base.Destroy();

            RowsView.Dispose();
            AvatarCache.Dispose();
        }

        private void RebuildPool(List<Score> scores, bool rebuildTextAtlas)
        {
            DestroyPool();

            if (rebuildTextAtlas)
                RowsView.SetScores(scores);

            AvailableItems = scores;
            PoolStartingIndex = 0;

            CreatePool();
            RowsView.Resize(ContentContainer.Size);
        }

        private static int CalculatePoolSize(float height, float rowHeight, float rowGap) => Math.Max(1, (int) Math.Ceiling(Math.Max(1, height) / Math.Max(1, rowHeight + rowGap)) + 2);
    }
}
