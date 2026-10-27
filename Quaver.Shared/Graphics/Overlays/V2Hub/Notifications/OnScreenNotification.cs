using System;
using MonoGame.Extended;
using Quaver.Shared.Graphics.Notifications;
using Wobble.Graphics;

namespace Quaver.Shared.Graphics.Overlays.V2Hub.Notifications;

/// <summary>
///     Compact notification displayed outside of the hub on V2 screens.
/// </summary>
public class OnScreenNotificationV2 : Container
{
    public const float NotificationWidth = NotificationCard.DefaultWidth;

    private NotificationCard Card { get; }

    public event Action<NotificationInfo> OnClick;

    public bool IsCardHovered => Card.IsCardHovered;

    public OnScreenNotificationV2(NotificationInfo item)
    {
        Size = new ScalableVector2(NotificationWidth, NotificationCard.MinimumHeight);

        Card = new NotificationCard(item, NotificationWidth, false, allowInputWhenDialogOpen: true)
        {
            Parent = this
        };
        Card.Clicked += OnCardClicked;

        Size = Card.Size;
    }

    public void UpdateContent(NotificationInfo item)
    {
        Card.UpdateContent(item);
        Size = Card.Size;
    }

    private void OnCardClicked(NotificationInfo item, object sender, EventArgs args)
    {
        item.ClickAction?.Invoke(sender, args);
        OnClick?.Invoke(item);
    }
}
