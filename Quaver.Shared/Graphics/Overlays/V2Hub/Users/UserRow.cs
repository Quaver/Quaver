using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.V2Hub.Users;

public class UserRow : PoolableSprite<User>
{
    public override int HEIGHT { get; } = 80;

    private RoundedButton Row {get; set;}
    private FlexContainer Layout { get; set; }
    private FlexContainer DetailsLayout { get; set; }
    private FlexContainer UserInfoLayout { get; set; }
    private FlexContainer NameLayout { get; set; }

    /// <summary>
    /// </summary>
    private SpriteAlphaMaskBlend Avatar { get; set; }

    /// <summary>
    /// </summary>
    private Sprite Flag { get; set; }

    /// <summary>
    /// </summary>
    private SpriteTextPlus Clan { get; set; }

    /// <summary>
    /// </summary>
    private SpriteTextPlus Username { get; set; }

    /// <summary>
    /// </summary>
    private MarqueeSpriteText Status { get; set; }

    /// <summary>
    /// </summary>
    private Sprite OnlineStatusIcon { get; set; }

    public event Action<User> OnClick;
    
    public UserRow(PoolableScrollContainer<User> container, User item, int index) : base(container, item, index)
    {
        Size = new ScalableVector2(container.Width, HEIGHT);
        
        CreateRow();
        
        CreateAvatar();
        CreateDetailsLayout();
        CreateUserInfoLayout();
        CreateFlag();
        CreateNameLayout();
        CreateClan();
        CreateUsername();
        CreateStatus();
        CreateOnlineStatusIcon();

        Layout.RefreshLayout();
        DetailsLayout.RefreshLayout();
        UserInfoLayout.RefreshLayout();
        NameLayout.RefreshLayout();

        SteamManager.SteamUserAvatarLoaded += OnSteamUserAvatarLoaded;
    }

    public override void UpdateContent(User item, int index)
    {
        Item = item;
        Index = index;

        ScheduleUpdate(() =>
        {
            var steamId = (ulong)item.OnlineUser.SteamId;

            Status.ResetPosition();
            Avatar.ClearAnimations();

            if (SteamManager.UserAvatars.ContainsKey(steamId))
            {
                Avatar.Image = Avatar.PerformBlend(SteamManager.UserAvatars[steamId], UserInterface.HubOnlineAvatarMask);
                Avatar.Alpha = 1;
            }
            else
            {
                Avatar.Alpha = 0;
                SteamManager.SendAvatarRetrievalRequest(steamId);
            }

            if (!item.HasUserInfo)
            {
                Clan.Visible = false;
                Clan.Text = "";
                Username.Text = $"Loading (#{item.OnlineUser.Id})...";
                Username.Tint = Color.White;
                Flag.Region = Flags.GetRegion("XX");
                SetStatusText("Idle");
            }
            else
            {
                var clanTag = item.OnlineUser.ClanTag;
                var hasClan = !string.IsNullOrEmpty(clanTag);

                Clan.Visible = hasClan;
                Clan.Text = hasClan ? $"[{clanTag}] " : "";
                var clanColor = item.OnlineUser.ClanAccentColor;
                Clan.Tint = clanColor != null ? ColorHelper.FromHex(clanColor) : Colors.GetUserChatColor(item.OnlineUser.UserGroups);

                Username.Text = item.OnlineUser.Username;
                Username.Tint = Colors.GetUserChatColor(item.OnlineUser.UserGroups);
                Flag.Region = Flags.GetRegion(item.OnlineUser.CountryFlag);
                UpdateUserStatus();
                
                DetailsLayout.RefreshLayout();
                NameLayout.RefreshLayout();
            }
        });
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
        
        Row.Clicked += (s, e) => OnClick?.Invoke(Item);
        Row.RightClicked += (s, e) => OnClick?.Invoke(Item);
        
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
    private void CreateAvatar()
    {
        Avatar = new SpriteAlphaMaskBlend
        {
            Parent = Layout,
            Size = new ScalableVector2(70, 70),
            Image = UserInterface.UnknownAvatar,
            Alpha = 0,
            UsePreviousSpriteBatchOptions = true
        };

        Layout.SetItemOptions(Avatar, new FlexItemOptions { Basis = Avatar.Width, Shrink = 0 });
    }
    private void CreateDetailsLayout()
    {
        DetailsLayout = new FlexContainer
        {
            Parent = Layout,
            Size = new ScalableVector2(Layout.Width - Avatar.Width - 10, Row.Height - 11),
            Direction = FlexDirection.Column,
            JustifyContent = FlexJustifyContent.Center,
            AlignItems = FlexAlignItems.Stretch,
            RowGap = 10
        };
        Layout.SetItemOptions(DetailsLayout, new FlexItemOptions
        {
            Basis = 0,
            Grow = 1,
            AlignSelf = FlexAlignSelf.FlexEnd
        });
    }
    private void CreateUserInfoLayout()
    {
        UserInfoLayout = new FlexContainer
        {
            Parent = DetailsLayout,
            Size = new ScalableVector2(DetailsLayout.Width, 24),
            Position = new ScalableVector2(0, 10),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            ColumnGap = 5
        };
        DetailsLayout.SetItemOptions(UserInfoLayout, new FlexItemOptions { Basis = 10, Shrink = 0 });
    }
    private void CreateFlag()
    {
        Flag = new Sprite
        {
            Parent = UserInfoLayout,
            Size = new ScalableVector2(24, 24),
            Region = Flags.GetRegion("XX"),
            UsePreviousSpriteBatchOptions = true
        };
        UserInfoLayout.SetItemOptions(Flag, new FlexItemOptions { Basis = Flag.Width, Shrink = 0 });
    }
    private void CreateNameLayout()
    {
        NameLayout = new FlexContainer
        {
            Parent = UserInfoLayout,
            Size = new ScalableVector2(UserInfoLayout.Width - Flag.Width - 5, UserInfoLayout.Height),
            Position = new ScalableVector2(0, 10),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center
        };

        UserInfoLayout.SetItemOptions(NameLayout, new FlexItemOptions { Basis = 0, Grow = 1 });
    }
    private void CreateClan()
    {
        Clan = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), "", 22)
        {
            Parent = NameLayout,
            UsePreviousSpriteBatchOptions = true
        };

        NameLayout.SetItemOptions(Clan, new FlexItemOptions { Shrink = 0 });
    }
    private void CreateUsername()
    {
        Username = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), "Loading...", 22)
        {
            Parent = NameLayout,
            UsePreviousSpriteBatchOptions = true
        };
    }
    private void CreateStatus()
    {
        Status = new MarqueeSpriteText(FontManager.GetWobbleFont(Fonts.InterBold), "Idle", 18, DetailsLayout.Width - 50)
        {
            Parent = DetailsLayout,
            IsActive = false,
            UsePreviousSpriteBatchOptions = true,
        };
        Status.TextSprite.Tint = ColorHelper.FromHex("#a6a6a6");
        Row.Hovered += (s, e) => Status.IsActive = true;
        Row.LeftHover += (s, e) => Status.IsActive = false;
        
        DetailsLayout.SetItemOptions(Status, new FlexItemOptions { Shrink = 0, AlignSelf =  FlexAlignSelf.FlexStart });
    }
    private void CreateOnlineStatusIcon()
    {
        OnlineStatusIcon = new Sprite
        {
            Parent = Avatar,
            Alignment = Alignment.BotRight,
            Size = new ScalableVector2(18, 18),
            Position = new ScalableVector2(-1, -1),
            UsePreviousSpriteBatchOptions = true,
            Image = UserInterface.HubOnlineIconV2,
        };
        OnlineStatusIcon.SpriteBatchOptions = RoundedRectShader.CreateScissorSafeOptions();
    }

    public void OnStatusUpdate(UserClientStatus status)
    {
        Item.CurrentStatus = status;
        UpdateUserStatus();
        DetailsLayout.RefreshLayout();
    }
    private void UpdateUserStatus()
    {
        var status = Item.CurrentStatus;

        switch (status.Status)
        {
            case ClientStatus.InMenus:
                SetStatusText("Idle");
                break;
            case ClientStatus.Selecting:
                SetStatusText("Selecting a Song");
                break;
            case ClientStatus.Playing:
                SetStatusText($"Playing {status.Content}");
                break;
            case ClientStatus.Paused:
                SetStatusText("Paused in Gameplay");
                break;
            case ClientStatus.Watching:
                SetStatusText($"Watching {status.Content}");
                break;
            case ClientStatus.Editing:
                SetStatusText($"Editing {status.Content}");
                break;
            case ClientStatus.InLobby:
                SetStatusText("Finding a Multiplayer Game");
                break;
            case ClientStatus.Multiplayer:
                SetStatusText("Playing Multiplayer");
                break;
            case ClientStatus.Listening:
                SetStatusText($"Listening to {status.Content}");
                break;
            default:
                SetStatusText("Idle");
                break;
        }
    }

    private void SetStatusText(string text)
    {
        if (Status.TextSprite.Text == text)
            return;

        Status.TextSprite.Text = text;
        Status.ResetPosition();
    }

    private void OnSteamUserAvatarLoaded(object sender, SteamAvatarLoadedEventArgs e)
    {
        AddScheduledUpdate(() =>
        {
            if (IsDisposed || e.SteamId != (ulong)Item.OnlineUser.SteamId)
                return;
            
            GameBase.Game.ScheduleRenderTargetDraw(() =>
            {
                if (IsDisposed || e.SteamId != (ulong)Item.OnlineUser.SteamId)
                    return;
                
                Avatar.Image = Avatar.PerformBlend(e.Texture, UserInterface.HubOnlineAvatarMask);
                Avatar.Alpha = 1;
            });
        });
    }
    
    public override void Destroy()
    {
        SteamManager.SteamUserAvatarLoaded -= OnSteamUserAvatarLoaded;
        base.Destroy();
    }
}
