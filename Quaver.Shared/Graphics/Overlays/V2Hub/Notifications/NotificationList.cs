using System;
using System.Collections.Generic;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Notifications;
using Wobble.Graphics;

namespace Quaver.Shared.Graphics.Overlays.V2Hub.Notifications;

public class NotificationList : PoolableScrollContainer<NotificationInfo>
{
    private float[] ItemOffsets { get; set; }

    public event Action<NotificationInfo> OnRowClicked;

    public NotificationList(List<NotificationInfo> availableItems, int poolSize, int poolStartingIndex, ScalableVector2 size, ScalableVector2 contentSize, bool startFromBottom = false) : base(availableItems, poolSize, poolStartingIndex, size, contentSize, startFromBottom)
    {
        RebuildItemOffsets();
        CreatePool();
    }

    private void RebuildItemOffsets()
    {
        ItemOffsets = new float[AvailableItems.Count + 1];

        for (var i = 0; i < AvailableItems.Count; i++)
            ItemOffsets[i + 1] = ItemOffsets[i] + NotificationRow.GetHeight(AvailableItems[i]);
    }

    protected override float GetItemOffset(int index) => ItemOffsets[Math.Clamp(index, 0, AvailableItems.Count)];

    protected override int GetMiddleObjectIndex()
    {
        var position = -ContentContainer.Y + Height / 2 - PaddingTop;
        var index = Array.BinarySearch(ItemOffsets, position);

        if (index < 0)
            index = ~index - 1;

        return Math.Clamp(index, 0, AvailableItems.Count - 1);
    }

    public override void RecalculateContainerHeight(bool usePoolCount = false)
    {
        var height = ItemOffsets[AvailableItems.Count] + PaddingTop + PaddingBottom;
        ContentContainer.Height = Math.Max(height, Height);
    }

    protected override PoolableSprite<NotificationInfo> CreateObject(NotificationInfo item, int index)
    {
        var row = new NotificationRow(this, item, index)
        {
            Alpha = 0
        };

        row.OnClick += OnRowClick;
        return row;
    }
    
    public void ChangeFeed(List<NotificationInfo> newPool)
    {
        DestroyPool();
        AvailableItems = new List<NotificationInfo>(newPool);
        RebuildItemOffsets();
        PoolStartingIndex = 0;
        ContentContainer.Animations.Clear();
        ContentContainer.Y = 0;
        TargetY = 0;
        PreviousTargetY = 0;
        PreviousContentContainerY = 0;
        CreatePool();
    }

    public void RefreshFeed(List<NotificationInfo> feed)
    {
        var previousY = ContentContainer.Y;
        ChangeFeed(feed);

        var minY = Math.Min(0f, Height - ContentContainer.Height);
        var y = Math.Clamp(previousY, minY, 0f);
        ContentContainer.Y = y;
        TargetY = y;
        PreviousTargetY = y;
        HandlePoolShifting();
        PreviousContentContainerY = y;
    }

    private void OnRowClick(NotificationInfo item)
    {
        OnRowClicked?.Invoke(item);
    }
    
}