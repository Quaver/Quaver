using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Server.Client;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.V2Chatting;

public sealed class ChatChannelTab : RoundedButton
{
    public float PreferredWidth => CloseButton == null ? 180 : Math.Max(180, Label.X + Label.Width + 10 + CloseButton.Width + 10);

    private int UserId { get; }
    private ulong SteamId { get; }
    private OnlineClient SubscribedClient { get; }
    private SpriteAlphaMaskBlend Avatar { get; set; }
    private Sprite OnlineStatusIcon { get; set; }
    private RoundedButton CloseButton { get; set; }

    public ChatChannelTab(ChatChannel channel, User privateUser, EventHandler clickAction)
    {
        CornerRadius = SkinV2BorderRadiusConfig.Normal;
        SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), channel.GetDisplayedName(), 18, Color.White);
        Label.Alignment = Alignment.MidLeft;
        Label.X = 10;
        Clicked += (sender, args) =>
        {
            if (CloseButton?.IsHovered != true)
                clickAction?.Invoke(sender, args);
        };

        if (!channel.IsPrivate)
            return;

        CreateCloseButton(channel);

        if (privateUser?.OnlineUser == null)
        {
            PositionContent();
            return;
        }

        UserId = privateUser.OnlineUser.Id;
        SteamId = (ulong)privateUser.OnlineUser.SteamId;
        SubscribedClient = OnlineManager.Client;

        CreateAvatar();
        CreateOnlineStatusIcon();
        PositionContent();

        UpdateOnlineStatus(OnlineManager.Connected && OnlineManager.OnlineUsers.ContainsKey(UserId));

        if (SubscribedClient != null)
        {
            SubscribedClient.OnUserConnected += OnUserConnected;
            SubscribedClient.OnUserDisconnected += OnUserDisconnected;
            SubscribedClient.OnUsersOnline += OnUsersOnline;
        }

        OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
        SteamManager.SteamUserAvatarLoaded += OnSteamUserAvatarLoaded;
        LoadAvatar();
    }

    private void CreateCloseButton(ChatChannel channel)
    {
        CloseButton = new RoundedButton((sender, args) => channel.Close())
        {
            Parent = this,
            Alignment = Alignment.MidRight,
            Position = new ScalableVector2(-10, 0),
            Size = new ScalableVector2(20, 20),
            CornerRadius = 10,
            Tint = ColorHelper.FromHex("#FFFFFF"),
            Depth = -1,
        };
        CloseButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), "×", 20, ColorHelper.FromHex("#181E25"));
        CloseButton.Label.Y -= 1;
        CloseButton.Label.Alignment = Alignment.MidCenter;
    }

    private void CreateAvatar()
    {
        Avatar = new SpriteAlphaMaskBlend
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            Size = new ScalableVector2(30, 30),
            Position = new ScalableVector2(10, 0),
            Image = UserInterface.UnknownAvatar,
            Alpha = 0,
            UsePreviousSpriteBatchOptions = true
        };
    }

    private void CreateOnlineStatusIcon()
    {
        var texture = UserInterface.HubOnlineIconV2;
        var size = Avatar.Width * texture.Width / UserInterface.HubOnlineAvatarMask.Width;

        OnlineStatusIcon = new Sprite
        {
            Parent = Avatar,
            Alignment = Alignment.BotRight,
            Position = new ScalableVector2(-1, -1),
            Size = new ScalableVector2(size, size),
            Image = texture,
            UsePreviousSpriteBatchOptions = true
        };
        OnlineStatusIcon.SpriteBatchOptions = RoundedRectShader.CreateScissorSafeOptions();
    }

    private void PositionContent()
    {
        if (Avatar == null)
        {
            Label.Alignment = Alignment.MidLeft;
            Label.X = 10;
            return;
        }

        Avatar.X = 10;
        Label.Alignment = Alignment.MidLeft;
        Label.X = Avatar.X + Avatar.Width + 10;
    }

    private void LoadAvatar()
    {
        if (SteamManager.UserAvatars.TryGetValue(SteamId, out var texture))
        {
            ScheduleAvatarUpdate(texture);
            return;
        }

        SteamManager.SendAvatarRetrievalRequest(SteamId);
    }

    private void OnSteamUserAvatarLoaded(object sender, SteamAvatarLoadedEventArgs e)
    {
        if (e.SteamId != SteamId)
            return;

        AddScheduledUpdate(() => ScheduleAvatarUpdate(e.Texture));
    }

    private void ScheduleAvatarUpdate(Microsoft.Xna.Framework.Graphics.Texture2D texture)
    {
        GameBase.Game.ScheduleRenderTargetDraw(() =>
        {
            if (IsDisposed || Avatar.IsDisposed)
                return;

            Avatar.Image = Avatar.PerformBlend(texture, UserInterface.HubOnlineAvatarMask);
            Avatar.Alpha = 1;
        });
    }

    private void OnUserConnected(object sender, UserConnectedEventArgs e)
    {
        if (e.User?.OnlineUser?.Id == UserId)
            ScheduleOnlineStatusUpdate(true);
    }

    private void OnUserDisconnected(object sender, UserDisconnectedEventArgs e)
    {
        if (e.UserId == UserId)
            ScheduleOnlineStatusUpdate(false);
    }

    private void OnUsersOnline(object sender, UsersOnlineEventArgs e)
    {
        if (e.UserIds.Contains(UserId))
            ScheduleOnlineStatusUpdate(true);
    }

    private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
    {
        if (e.Value != ConnectionStatus.Connected)
            ScheduleOnlineStatusUpdate(false);
    }

    private void ScheduleOnlineStatusUpdate(bool online) => AddScheduledUpdate(() => UpdateOnlineStatus(online));

    private void UpdateOnlineStatus(bool online)
    {
        if (!IsDisposed && OnlineStatusIcon != null)
            OnlineStatusIcon.Tint = online ? Color.White : ColorHelper.FromHex("#828E99");
    }

    public override void Destroy()
    {
        if (SubscribedClient != null)
        {
            SubscribedClient.OnUserConnected -= OnUserConnected;
            SubscribedClient.OnUserDisconnected -= OnUserDisconnected;
            SubscribedClient.OnUsersOnline -= OnUsersOnline;
        }

        OnlineManager.Status.ValueChanged -= OnConnectionStatusChanged;
        SteamManager.SteamUserAvatarLoaded -= OnSteamUserAvatarLoaded;
        base.Destroy();
    }
}
