using MonoGame.Extended;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Input;

namespace Quaver.Shared.Graphics.Overlays.V2Hub.Notifications;

public class NotificationRow : PoolableSprite<NotificationInfo>
{
    public override int HEIGHT { get; } = 88;
    
    private ViewportRoundedButton Row { get; set; }
    
    private FlexContainer Layout { get; set; }
    
    public NotificationRow(PoolableScrollContainer<NotificationInfo> container, NotificationInfo item, int index) : base(container, item, index)
    {
        CreateRow();
    }

    public override void UpdateContent(NotificationInfo item, int index)
    {
        Item = item;
        Index = index;
    }
    
    private void CreateRow()
    {
        Row = new ViewportRoundedButton(Container)
        {
            Parent = this,
            Size = new ScalableVector2(716, 70),
            CornerRadius = SkinV2BorderRadiusConfig.Normal,
            Tint = ColorHelper.FromHex("#181E25"),
        };
        
        Row.Hovered += (s, e) => Row.Tint = ColorHelper.FromHex("#354451");
        Row.LeftHover += (s, e) => Row.Tint = ColorHelper.FromHex("#181E25");
        
        //Row.Clicked += (s, e) => OnClick?.Invoke(Item);
        //Row.RightClicked += (s, e) => OnClick?.Invoke(Item);
        
        Layout = new FlexContainer
        {
            Parent = Row,
            Position = new ScalableVector2(0, 0),
            Size = new ScalableVector2(Row.Width, Row.Height),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            ColumnGap = 10
        };
    }
    
    private sealed class ViewportRoundedButton : RoundedButton
    {
        private ScrollContainer Viewport { get; }

        public ViewportRoundedButton(ScrollContainer viewport) => Viewport = viewport;

        protected override bool IsMouseInClickArea() =>
            base.IsMouseInClickArea() &&
            GraphicsHelper.RectangleContains(Viewport.ScreenRectangle, MouseManager.CurrentState.Position);
    }
}