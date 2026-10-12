using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.V2Hub.Notifications;

public class NotificationRow : PoolableSprite<NotificationInfo>
{
    private const float MeasurementWidth = 701;
    private const float RowSpacing = 10;
    private const float ActionsHeight = 40;

    public override int HEIGHT => Container == null ? GetCardHeight(Item) : GetHeight(Item);

    public static int GetHeight(NotificationInfo item) => GetCardHeight(item) + (int)RowSpacing;

    private NotificationCard Card { get; set; }

    private FlexContainer ActionsLayout { get; set; }

    private ViewportRoundedButton AcceptButton { get; set; }

    private ViewportRoundedButton DenyButton { get; set; }

    private ViewportRoundedButton CloseButton { get; set; }

    private bool UpdatingSize { get; set; }

    public event Action<NotificationInfo> OnClick;

    public bool IsCardHovered => Card.IsCardHovered || CloseButton.IsHovered;

    public NotificationRow(PoolableScrollContainer<NotificationInfo> container, NotificationInfo item, int index)
        : base(container, item, index)
    {
        Size = new ScalableVector2(container?.Width ?? MeasurementWidth, container == null ? GetCardHeight(item) : GetHeight(item));

        if (container == null)
            Alpha = 0;

        Card = new NotificationCard(item, Width, true, container, container == null)
        {
            Parent = this
        };
        Card.Clicked += OnCardClicked;

        CreateActions();
        CreateCloseButton();
    }

    protected override void OnRectangleRecalculated()
    {
        base.OnRectangleRecalculated();
        if (IsDisposed || UpdatingSize || Card == null || Item == null)
            return;

        UpdateContent(Item, Index);
    }

    public override void UpdateContent(NotificationInfo item, int index)
    {
        Item = item;
        Index = index;

        var isInvite = item.Type == NotificationType.MultiplayerInvite;
        var footerHeight = isInvite ? ActionsHeight : 0;
        var cardHeight = GetCardHeight(item);

        Card.Width = Width;
        Card.UpdateContent(item, footerHeight, cardHeight);

        ActionsLayout.Visible = isInvite;
        ActionsLayout.Size = Card.Footer.Size;
        if (isInvite)
            ActionsLayout.RefreshLayout();
        else
        {
            AcceptButton.ResetInteractionState();
            DenyButton.ResetInteractionState();
        }

        UpdatingSize = true;
        Size = new ScalableVector2(Width, Card.Height + (Container == null ? 0 : RowSpacing));
        UpdatingSize = false;
    }

    private void OnCardClicked(NotificationInfo item, object sender, EventArgs args)
    {
        if (Container != null && item.Type == NotificationType.MultiplayerInvite)
            return;

        OnClick?.Invoke(item);
        item.ClickAction?.Invoke(sender, args);
    }

    private void CreateActions()
    {
        ActionsLayout = new FlexContainer
        {
            Parent = Card.Footer,
            Size = new ScalableVector2(Card.Footer.Width, ActionsHeight),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            ColumnGap = 10,
            Visible = false,
            UpdateWhenInvisible = false
        };

        AcceptButton = new ViewportRoundedButton(Container)
        {
            Parent = ActionsLayout,
            Size = new ScalableVector2(204, ActionsHeight),
            CornerRadius = SkinV2BorderRadiusConfig.Normal,
            Tint = ColorHelper.FromHex("#6B83B2"),
            Depth = -1
        };
        ActionsLayout.SetItemOptions(AcceptButton, new FlexItemOptions { Basis = AcceptButton.Width, Shrink = 0 });
        AcceptButton.Clicked += (sender, args) =>
        {
            var clickedItem = Item;
            clickedItem.ClickAction?.Invoke(sender, args);
            NotificationManager.MarkClicked(clickedItem);
            OnClick?.Invoke(clickedItem);
        };
        AcceptButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Hub_NotificationAccept"), 18, Color.White);

        DenyButton = new ViewportRoundedButton(Container)
        {
            Parent = ActionsLayout,
            Size = new ScalableVector2(204, ActionsHeight),
            CornerRadius = SkinV2BorderRadiusConfig.Normal,
            Tint = ColorHelper.FromHex("#6B83B2"),
            Depth = -1
        };
        ActionsLayout.SetItemOptions(DenyButton, new FlexItemOptions { Basis = DenyButton.Width, Shrink = 0 });
        DenyButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold),
            LocalizationManager.Get("Screen_Hub_NotificationDeny"), 18, Color.White);
        DenyButton.Clicked += (sender, args) =>
        {
            var clickedItem = Item;
            NotificationManager.MarkClicked(clickedItem);
            OnClick?.Invoke(clickedItem);
        };
    }

    private void CreateCloseButton()
    {
        CloseButton = new ViewportRoundedButton(Container)
        {
            Parent = Card,
            Alignment = Alignment.TopRight,
            Position = new ScalableVector2(-10, 10),
            Size = new ScalableVector2(20, 20),
            CornerRadius = 10,
            Tint = Color.Transparent,
            Depth = -1,
            AllowInputWhenDialogOpen = Container == null
        };
        CloseButton.SetIcon(GlobalIcons.Get(GlobalIcon.Remove), new Vector2(40, 40));
        CloseButton.Icon.Tint = ColorHelper.FromHex("#6B83B2");
        CloseButton.Clicked += (sender, args) => OnClick?.Invoke(Item);
    }

    private static int GetCardHeight(NotificationInfo item)
    {
        var footerHeight = item.Type == NotificationType.MultiplayerInvite ? ActionsHeight : 0;
        return (int)Math.Ceiling(NotificationCard.MeasureHeight(item, MeasurementWidth, true, footerHeight));
    }

    private sealed class ViewportRoundedButton : RoundedButton
    {
        private ScrollContainer Viewport { get; }

        public ViewportRoundedButton(ScrollContainer viewport) => Viewport = viewport;

        protected override bool IsMouseInClickArea() =>
            base.IsMouseInClickArea() &&
            (Viewport == null || GraphicsHelper.RectangleContains(Viewport.ScreenRectangle, MouseManager.CurrentState.Position));
    }
}
