using System;
using System.Collections.Generic;
using Quaver.Server.Client.Objects.Twitch;
using Quaver.Shared.Graphics.Containers;
using Wobble.Graphics;

namespace Quaver.Shared.Graphics.Overlays.V2Hub.SongRequests;

public class SongRequestList : PoolableScrollContainer<SongRequest>
{
    private HashSet<SongRequest> PlayedRequests { get; } = new();

    public event Action<SongRequest> OnRowClicked;

    public SongRequestList(List<SongRequest> availableItems, int poolSize, ScalableVector2 size) : base(availableItems, poolSize, 0, size, size)
    {
        CreatePool();
    }

    protected override PoolableSprite<SongRequest> CreateObject(SongRequest item, int index)
    {
        var row = new SongRequestRow(this, item, index)
        {
            Alpha = 0
        };
        row.OnClick += request => OnRowClicked?.Invoke(request);
        return row;
    }

    public bool IsPlayed(SongRequest request) => PlayedRequests.Contains(request);

    public void MarkPlayed(SongRequest request)
    {
        PlayedRequests.Add(request);

        foreach (var row in Pool)
        {
            if (row.Item == request)
                row.UpdateContent(request, row.Index);
        }
    }

    public void RefreshFeed(List<SongRequest> requests)
    {
        var previousY = ContentContainer.Y;
        DestroyPool();
        AvailableItems = new List<SongRequest>(requests);
        PlayedRequests.RemoveWhere(request => !AvailableItems.Contains(request));
        PoolStartingIndex = 0;
        ContentContainer.Animations.Clear();
        ContentContainer.Y = 0;
        TargetY = 0;
        PreviousTargetY = 0;
        PreviousContentContainerY = 0;
        CreatePool();

        var minY = Math.Min(0f, Height - ContentContainer.Height);
        var y = Math.Clamp(previousY, minY, 0f);
        ContentContainer.Y = y;
        TargetY = y;
        PreviousTargetY = y;
        HandlePoolShifting();
        PreviousContentContainerY = y;
    }

    public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
    {
        InputEnabled = IsHovered();
        base.Update(gameTime);
    }
}
