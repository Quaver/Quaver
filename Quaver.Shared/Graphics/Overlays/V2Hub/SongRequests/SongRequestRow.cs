using System;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Objects.Twitch;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.V2Hub.SongRequests;

public class SongRequestRow : PoolableSprite<SongRequest>
{
    public override int HEIGHT { get; } = 146;

    private ViewportRoundedButton Row { get; set; }
    private Sprite Icon { get; set; }
    private MarqueeSpriteText RequestBy { get; set; }
    private MarqueeSpriteText Details { get; set; }
    private MarqueeSpriteText CreatorDetails { get; set; }
    
    private ViewportRoundedButton DifficultyDetailsRow { get; set; }
    private Sprite DifficultyIcon { get; set; }
    private MarqueeSpriteText DifficultyDetails { get; set; }
    
    private FlexContainer Layout { get; set; }
    private FlexContainer DetailsLayout { get; set; }
    private FlexContainer DifficultyLayout { get; set; }

    public event Action<SongRequest> OnClick;

    public SongRequestRow(PoolableScrollContainer<SongRequest> container, SongRequest item, int index) : base(container, item, index)
    {
        Size = new ScalableVector2(container.Width, HEIGHT);

        CreateRow();
        CreateIcon();
        CreateDetails();
        CreateDifficultyDetails();

        Layout.RefreshLayout();
        DetailsLayout.RefreshLayout();
        DifficultyLayout.RefreshLayout();
    }

    public override void UpdateContent(SongRequest item, int index)
    {
        Item = item;
        Index = index;

        RequestBy.TextSprite.Text = LocalizationManager.Get("Screen_Hub_RequestDetails", item.TwitchUsername ?? "");
        RequestBy.ResetPosition();
        
        Details.TextSprite.Text = $"{item.Artist} - {item.Title}";
        Details.ResetPosition();
        
        CreatorDetails.TextSprite.Text = item.Creator ?? "";
        CreatorDetails.ResetPosition();
        
        DifficultyIcon.Tint = GetDifficultyColor(Item);
        DifficultyDetails.TextSprite.Tint = GetDifficultyColor(Item);
        DifficultyDetails.TextSprite.Text = $"{item.DifficultyRating} - {item.DifficultyName}";
        DifficultyDetails.ResetPosition();

        var alpha = ((SongRequestList)Container).IsPlayed(item) ? 0.7f : 1f;
        RequestBy.TextSprite.Alpha = alpha;
        Details.TextSprite.Alpha = alpha;
        CreatorDetails.TextSprite.Alpha = alpha;
    }

    private void CreateRow()
    {
        Row = new ViewportRoundedButton(Container)
        {
            Parent = this,
            Size = new ScalableVector2(716, 136),
            CornerRadius = SkinV2BorderRadiusConfig.Normal,
            Tint = ColorHelper.HexToColor("#181E25"),
            PerformHoverFade = false
        };

        Row.Hovered += (sender, args) =>
        {
            Row.Tint = ColorHelper.HexToColor("#3D4B64");
            RequestBy.IsActive = true;
            Details.IsActive = true;
            CreatorDetails.IsActive = true;
            DifficultyDetails.IsActive = true;
        };
        Row.LeftHover += (sender, args) =>
        {
            Row.Tint = ColorHelper.HexToColor("#181E25");
            RequestBy.IsActive = false;
            Details.IsActive = false;
            CreatorDetails.IsActive = false;
            DifficultyDetails.IsActive = false;
        };
        Row.Clicked += (sender, args) => OnClick?.Invoke(Item);
        Row.RightClicked += (sender, args) => OnClick?.Invoke(Item);

        Layout = new FlexContainer
        {
            Parent = Row,
            Position = new ScalableVector2(15, 0),
            Size = new ScalableVector2(Row.Width - 32, Row.Height),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            ColumnGap = 15
        };
    }

    private void CreateIcon()
    {
        Icon = new Sprite
        {
            Parent = Layout,
            Size = new ScalableVector2(45, 34),
            Image = UserInterface.HubSongRequestsV2,
            UsePreviousSpriteBatchOptions = true
        };
        Layout.SetItemOptions(Icon, new FlexItemOptions { Basis = Icon.Width, Shrink = 0 });
    }

    private void CreateDetails()
    {
        DetailsLayout = new FlexContainer
        {
            Parent = Layout,
            Size = new ScalableVector2(Layout.Width - Icon.Width - 16, Row.Height),
            Direction = FlexDirection.Column,
            JustifyContent = FlexJustifyContent.FlexStart,
            AlignItems = FlexAlignItems.Stretch
        };
        Layout.SetItemOptions(DetailsLayout, new FlexItemOptions { Basis = DetailsLayout.Width, Shrink = 0 });

        var topSpacer = new Container
        {
            Parent = DetailsLayout,
            Size = new ScalableVector2(0, 15)
        };
        DetailsLayout.SetItemOptions(topSpacer, new FlexItemOptions { Basis = 15, Shrink = 0 });

        RequestBy = new MarqueeSpriteText(FontManager.GetWobbleFont(Fonts.InterBold), "", 20, DetailsLayout.Width)
        {
            Parent = DetailsLayout,
            Height = 24,
            IsActive = false,
            UsePreviousSpriteBatchOptions = true
        };
        RequestBy.TextSprite.Tint = Color.White;
        DetailsLayout.SetItemOptions(RequestBy, new FlexItemOptions { Shrink = 0 });

        Details = new MarqueeSpriteText(FontManager.GetWobbleFont(Fonts.InterBold), "", 18, DetailsLayout.Width)
        {
            Parent = DetailsLayout,
            Height = 24,
            IsActive = false,
            UsePreviousSpriteBatchOptions = true
        };
        Details.Height = Details.TextSprite.Height + 3;
        Details.TextSprite.Tint = ColorHelper.HexToColor("#D9E3F4");
        DetailsLayout.SetItemOptions(Details, new FlexItemOptions { Shrink = 0 });
        
        CreatorDetails = new MarqueeSpriteText(FontManager.GetWobbleFont(Fonts.InterBold), "", 16, DetailsLayout.Width)
        {
            Parent = DetailsLayout,
            IsActive = false,
            Height = 24,
            UsePreviousSpriteBatchOptions = true
        };
        CreatorDetails.Height = CreatorDetails.TextSprite.Height + 3;
        CreatorDetails.TextSprite.Tint = ColorHelper.HexToColor("#D9E3F4");
        DetailsLayout.SetItemOptions(CreatorDetails, new FlexItemOptions { Shrink = 0 });

        var bottomSpacer = new Container
        {
            Parent = DetailsLayout,
            Size = new ScalableVector2(0, 5)
        };
        DetailsLayout.SetItemOptions(bottomSpacer, new FlexItemOptions { Basis = 5, Shrink = 0 });
    }

    private void CreateDifficultyDetails()
    {
        
        DifficultyDetailsRow = new ViewportRoundedButton(Container)
        {
            Parent = DetailsLayout,
            Size = new ScalableVector2(359, 30),
            CornerRadii = new RoundedRectCornerRadii(15, 15, 15, 15),
            Tint = ColorHelper.HexToColor("#273038"),
            PerformHoverFade = false
        };
        DetailsLayout.SetItemOptions(DifficultyDetailsRow, new FlexItemOptions { Shrink = 0 });
        DifficultyDetailsRow.Clicked += (sender, args) => OnClick?.Invoke(Item);
        DifficultyDetailsRow.RightClicked += (sender, args) => OnClick?.Invoke(Item);
        
        DifficultyLayout = new FlexContainer
        {
            Parent = DifficultyDetailsRow,
            Size = new ScalableVector2(DifficultyDetailsRow.Width - 20, DifficultyDetailsRow.Height),
            Position = new ScalableVector2(10, 0),
            Direction = FlexDirection.Row,
            JustifyContent = FlexJustifyContent.FlexStart,
            AlignItems = FlexAlignItems.Center,
            ColumnGap = 5
        };
        
        DifficultyIcon = new Sprite
        {
            Parent = DifficultyLayout,
            Size = new ScalableVector2(24, 18),
            Image = UserInterface.DifficultyIcon,
            Tint = GetDifficultyColor(Item),
            UsePreviousSpriteBatchOptions = true
        };
        DifficultyLayout.SetItemOptions(DifficultyIcon, new FlexItemOptions { Basis = DifficultyIcon.Width, Shrink = 0 });
        
        DifficultyDetails = new MarqueeSpriteText(FontManager.GetWobbleFont(Fonts.InterBold), "", 14,
            DifficultyLayout.Width - DifficultyIcon.Width - DifficultyLayout.ColumnGap)
        {
            Parent = DifficultyLayout,
            IsActive = false,
            UsePreviousSpriteBatchOptions = true
        };
        DifficultyDetails.TextSprite.Tint = GetDifficultyColor(Item);
        DifficultyLayout.SetItemOptions(DifficultyDetails, new FlexItemOptions { Shrink = 0 });
    }

    private Color GetDifficultyColor(SongRequest request)
    {
        var color = (MapGame)request.Game == MapGame.Osu
            ? ColorHelper.OsuStarRatingToColor((float)request.DifficultyRating)
            : ColorHelper.DifficultyToColor((float)request.DifficultyRating);
        
        return color;
    }

    private sealed class ViewportRoundedButton : RoundedButton
    {
        private ScrollContainer Viewport { get; }

        public ViewportRoundedButton(ScrollContainer viewport) => Viewport = viewport;

        protected override bool IsMouseInClickArea() =>
            base.IsMouseInClickArea() && GraphicsHelper.RectangleContains(Viewport.ScreenRectangle, MouseManager.CurrentState.Position);
    }
}
