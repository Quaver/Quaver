using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Server.Client;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning.V2;
using Steamworks;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.UI
{
    internal sealed class ProfileSummary : RoundedButton
    {
        private SkinV2ProfileConfig Config { get; }

        private Texture2D OfflineAvatar { get; }

        private SpriteAlphaMaskBlend Avatar { get; }

        private Sprite Flag { get; }

        private ClanTag Clan { get; }

        private SpriteTextPlus Username { get; }

        private Sprite StatusIcon { get; }

        private Texture2D CurrentAvatarSource { get; set; }

        private bool LastConnected { get; set; }

        private object LastUser { get; set; }

        private string LastUsername { get; set; }

        private float LastWidth { get; set; }

        public ProfileSummary(SkinV2ProfileConfig config, Color backgroundColor, Texture2D offlineAvatar, float buttonSize)
        {
            Config = config;
            OfflineAvatar = offlineAvatar;
            Size = new ScalableVector2(Config.Width, buttonSize);
            Tint = backgroundColor;
            CornerRadius = Config.CornerRadius;
            PerformHoverFade = true;

            Avatar = new SpriteAlphaMaskBlend
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(buttonSize, buttonSize),
                Image = OfflineAvatar,
                Alpha = 0,
                UsePreviousSpriteBatchOptions = true
            };

            var statusTexture = UserInterface.HubOnlineIconV2;
            var statusSize = buttonSize * statusTexture.Width / UserInterface.HubOnlineAvatarMask.Width;
            StatusIcon = new Sprite
            {
                Parent = Avatar,
                Alignment = Alignment.BotRight,
                Position = new ScalableVector2(-1, -1),
                Size = new ScalableVector2(statusSize, statusSize),
                Image = statusTexture,
                UsePreviousSpriteBatchOptions = true
            };
            StatusIcon.SpriteBatchOptions = RoundedRectShader.CreateScissorSafeOptions();

            Flag = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Config.FlagX,
                Size = new ScalableVector2(Config.FlagSize, Config.FlagSize),
                Region = Flags.GetRegion("XX"),
                Visible = false
            };

            Clan = new ClanTag(Config.UsernameFontSize)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Flag.X + Flag.Width + Config.TextSpacing
            };

            Username = new SpriteTextPlus(FontManager.GetWobbleFont(Config.UsernameFont), string.Empty,
                Config.UsernameFontSize)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Tint = SkinV2Color.Parse(Config.TextColor)
            };

            ConfigManager.Username.ValueChanged += OnUsernameChanged;
            OnlineManager.Status.ValueChanged += OnOnlineStatusChanged;
            SteamManager.SteamUserAvatarLoaded += OnSteamAvatarLoaded;

            UpdateProfile();
        }

        public override void Update(GameTime gameTime)
        {
            var connected = OnlineManager.Connected;
            var username = GetDisplayUsername(connected);
            if (connected != LastConnected || !ReferenceEquals(LastUser, OnlineManager.Self) ||
                LastUsername != username || Math.Abs(Width - LastWidth) > 0.5f)
                UpdateProfile();

            base.Update(gameTime);
        }

        public override void Destroy()
        {
            ConfigManager.Username.ValueChanged -= OnUsernameChanged;
            OnlineManager.Status.ValueChanged -= OnOnlineStatusChanged;
            SteamManager.SteamUserAvatarLoaded -= OnSteamAvatarLoaded;
            base.Destroy();
        }

        private void UpdateProfile()
        {
            var connected = OnlineManager.Connected;
            var user = OnlineManager.Self;
            var username = GetDisplayUsername(connected);

            UpdateAvatar(GetAvatar());
            StatusIcon.Tint = connected ? Color.White : SkinV2Color.Parse(Config.OfflineStatusColor);

            if (connected)
            {
                Flag.Region = Flags.GetRegion(user?.OnlineUser?.CountryFlag ?? "XX");
                Flag.Visible = true;
                Clan.UpdateFromUser(user?.OnlineUser, SkinV2Color.Parse(Config.TextColor));
            }
            else
            {
                Flag.Visible = false;
                Clan.Clear();
            }

            Clan.X = Flag.Visible ? Flag.X + Flag.Width + Config.TextSpacing : Config.FlagX - 1;
            var usernameX = Clan.Visible ? Clan.X + Clan.Width + Config.TextSpacing - 1 : Clan.X;
            Username.X = usernameX;
            Username.Text = username;
            Username.TruncateWithEllipsis((int) Math.Max(40, Width - usernameX - Config.UsernameRightPadding));

            LastConnected = connected;
            LastUser = user;
            LastUsername = username;
            LastWidth = Width;
        }

        private static string GetDisplayUsername(bool connected) => connected ? OnlineManager.Self?.OnlineUser?.Username ?? ConfigManager.Username.Value ?? "Player" : "Login";

        private Texture2D GetAvatar()
        {
            var image = OfflineAvatar;

            if (OnlineManager.Status.Value == ConnectionStatus.Connected && SteamManager.UserAvatars != null)
            {
                var id = SteamUser.GetSteamID().m_SteamID;
                if (SteamManager.UserAvatars.TryGetValue(id, out var avatar))
                    image = avatar;
            }

            return image;
        }

        private void UpdateAvatar(Texture2D image)
        {
            if (ReferenceEquals(CurrentAvatarSource, image))
                return;

            CurrentAvatarSource = image;
            Avatar.Alpha = 0;
            GameBase.Game.ScheduleRenderTargetDraw(() =>
            {
                if (IsDisposed || !ReferenceEquals(CurrentAvatarSource, image))
                    return;

                Avatar.Image = Avatar.PerformBlend(image, UserInterface.HubOnlineAvatarMask);
                Avatar.Alpha = 1;
            });
        }

        private void OnUsernameChanged(object sender, BindableValueChangedEventArgs<string> args) => UpdateProfile();

        private void OnOnlineStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> args) => UpdateProfile();

        private void OnSteamAvatarLoaded(object sender, SteamAvatarLoadedEventArgs args)
        {
            if (SteamUser.GetSteamID().m_SteamID == args.SteamId)
                AddScheduledUpdate(() =>
                {
                    if (!IsDisposed)
                        UpdateProfile();
                });
        }
    }
}
