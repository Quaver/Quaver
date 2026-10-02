using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.V2Chatting;

public sealed class ChatChannelTab : RoundedButton
{

    private ulong SteamId { get; }
    private SpriteAlphaMaskBlend Avatar { get; set; }
    private RoundedButton CloseButton { get; set; }

    public ChatChannelTab(ChatChannel channel, User privateUser, EventHandler clickAction)
    {
        CornerRadius = SkinV2BorderRadiusConfig.Normal;
        SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), channel.GetDisplayedName(), 18, Color.White);
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

        SteamId = (ulong)privateUser.OnlineUser.SteamId;
        CreateAvatar();
        PositionContent();
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
            Alignment = Alignment.MidCenter,
            Size = new ScalableVector2(30, 30),
            Image = UserInterface.UnknownAvatar,
            Alpha = 0,
            UsePreviousSpriteBatchOptions = true
        };
    }

    private void PositionContent()
    {
        var contentCenter = -30 / 2;

        if (Avatar == null)
        {
            Label.X = contentCenter;
            return;
        }

        var contentWidth = Avatar.Width + 10 + Label.Width;
        Avatar.X = contentCenter - contentWidth / 2 + Avatar.Width / 2;
        Label.X = Avatar.X + Avatar.Width / 2 + 10 + Label.Width / 2;
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

    public override void Destroy()
    {
        SteamManager.SteamUserAvatarLoaded -= OnSteamUserAvatarLoaded;
        base.Destroy();
    }
}
