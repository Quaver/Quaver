using System.Collections.Generic;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Notifications;
using Wobble.Graphics;

namespace Quaver.Shared.Graphics.Overlays.V2Hub.Notifications;

public class NotificationList : PoolableScrollContainer<NotificationInfo>
{
    public NotificationList(List<NotificationInfo> availableItems, int poolSize, int poolStartingIndex, ScalableVector2 size, ScalableVector2 contentSize, bool startFromBottom = false) : base(availableItems, poolSize, poolStartingIndex, size, contentSize, startFromBottom)
    {
        
        CreatePool();
    }

    protected override PoolableSprite<NotificationInfo> CreateObject(NotificationInfo item, int index)
    {
        return new NotificationRow(this, item, index)
        {
            Alpha = 0
        };
    }
    
    
}