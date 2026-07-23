using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Client;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Menu.Border.Components.Users;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Main.UI;
using Quaver.Shared.Screens.Options;
using Quaver.Shared.Skinning;
using Quaver.Shared.Skinning.V2;
using Steamworks;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Navigation;
using Wobble.Graphics.UI.Tooltips;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.V2.UI
{
    internal enum NavigationIcon
    {
        Volume = 0,
        Power = 1,
        Donate = 2,
        Settings = 3,
        List = 4,
        Menu = 5,
        Chat = 6,
        Tools = 7,
        Music = 8,
        Website = 9,
        Discord = 10,
        GitHub = 11
    }

    /// <summary>
    ///     Shared top and bottom chrome for replacement screens.
    /// </summary>
    internal sealed class ScreenNavigation : Container
    {
        public const string ElementKey = "quaver-screen-navigation";

        private const int IconPitch = 39;
        private const int IconWidth = 32;
        private const int IconHeight = 25;

        private SkinStoreV2Lease Skin { get; }

        private SkinV2NavigationConfig Config { get; }

        private NavigationBar TopBar { get; }

        private NavigationBar BottomBar { get; }

        private ProfileControl ProfileButton { get; }

        private RoundedButton HubButton { get; }

        private Texture2D HubListIcon { get; }

        private Texture2D HubMenuIcon { get; }

        private OnlineHub SubscribedOnlineHub { get; set; }

        private List<Texture2D> OwnedIcons { get; } = new List<Texture2D>();

        private ScreenNavigation()
        {
            Skin = SkinManager.AcquireV2();
            Config = Skin.Config.Shared.Navigation;
            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);

            TopBar = CreateBar(Alignment.TopLeft, Config.Bar);
            BottomBar = CreateBar(Alignment.BotLeft, Config.Footer);

            AddIconButton(TopBar, NavigationBarRegion.Left, NavigationIcon.Music,
                LocalizationManager.Get("Screen_Main_Menu_Jukebox"), OpenMusicPlayer,
                TooltipAnchor.BottomCenter);
            AddIconButton(TopBar, NavigationBarRegion.Left, NavigationIcon.Chat,
                LocalizationManager.Get("Screen_Options_ToggleChatOverlay"), ToggleChat,
                TooltipAnchor.BottomCenter);
            AddIconButton(TopBar, NavigationBarRegion.Left, NavigationIcon.Donate,
                LocalizationManager.Get("Screen_Main_Menu_Donate"), ShowDonateMessage,
                TooltipAnchor.BottomCenter);

            ProfileButton = new ProfileControl(Config.Profile,
                SkinV2Color.Parse(Config.Button.BackgroundColor),
                UserInterface.OfflineAvatar,
                Config.Button.Size);
            TopBar.Add(NavigationBarRegion.Right, ProfileButton);
            HubListIcon = LoadIcon(NavigationIcon.List);
            HubMenuIcon = LoadIcon(NavigationIcon.Menu);
            HubButton = AddIconButton(TopBar, NavigationBarRegion.Right, HubMenuIcon,
                "Online Hub", ToggleOnlineHub, TooltipAnchor.BottomCenter);

            AddIconButton(BottomBar, NavigationBarRegion.Left, NavigationIcon.Website,
                LocalizationManager.Get("Screen_Main_Menu_Website"),
                () => BrowserHelper.OpenURL("https://quavergame.com"), TooltipAnchor.TopCenter);
            AddIconButton(BottomBar, NavigationBarRegion.Left, NavigationIcon.Discord,
                LocalizationManager.Get("Screen_Main_Menu_Discord"),
                () => BrowserHelper.OpenURL("https://discord.gg/quaver", true), TooltipAnchor.TopCenter);
            AddIconButton(BottomBar, NavigationBarRegion.Left, NavigationIcon.GitHub,
                LocalizationManager.Get("Screen_Main_Menu_GitHub"),
                () => BrowserHelper.OpenURL("https://github.com/Quaver"), TooltipAnchor.TopCenter);

            AddIconButton(BottomBar, NavigationBarRegion.Right, NavigationIcon.Volume,
                LocalizationManager.Get("Screen_Options_Volume"), ShowVolume, TooltipAnchor.TopCenter);
            AddIconButton(BottomBar, NavigationBarRegion.Right, NavigationIcon.Settings,
                LocalizationManager.Get("Screen_Main_Options"),
                () => DialogManager.Show(new OptionsDialog()), TooltipAnchor.TopCenter);
            AddIconButton(BottomBar, NavigationBarRegion.Right, NavigationIcon.Power,
                LocalizationManager.Get("Screen_Main_QuitGame"),
                () => DialogManager.Show(new QuitDialog()), TooltipAnchor.TopCenter);

        }

        public static ScreenNavigation EnsureAttached(Container parent)
        {
            if (ScreenManager.TryGetElement<ScreenNavigation>(ElementKey, out var navigation))
            {
                if (navigation.Skin.Generation == SkinManager.SkinV2?.Generation)
                {
                    navigation.ResetTransientState();
                    navigation.ResizeToWindow();
                    return navigation;
                }

                ScreenManager.RemoveElement(ElementKey);
            }

            navigation = new ScreenNavigation { Parent = parent };
            ScreenManager.RegisterElement(ElementKey, navigation);
            return navigation;
        }

        public override void Update(GameTime gameTime)
        {
            ResizeToWindow();
            EnsureOnlineHubSubscription();
            base.Update(gameTime);
        }

        public override void Destroy()
        {
            if (SubscribedOnlineHub != null)
                SubscribedOnlineHub.UnreadStateChanged -= OnHubUnreadStateChanged;

            base.Destroy();

            foreach (var icon in OwnedIcons)
                icon?.Dispose();

            OwnedIcons.Clear();
            Skin.Dispose();
        }

        private NavigationBar CreateBar(Alignment alignment, SkinV2NavigationBarConfig config) => new NavigationBar(
            WindowManager.Width, Config.Button.Size + Config.EdgePadding * 2, Color.Transparent)
        {
            Parent = this,
            Alignment = alignment,
            EdgePadding = Config.EdgePadding,
            ItemSpacing = Config.ItemSpacing,
            Background = SkinV2Background.Create(Skin, config.Background)
        };

        private RoundedButton AddIconButton(NavigationBar bar, NavigationBarRegion region,
            NavigationIcon icon, string tooltip, Action action, TooltipAnchor tooltipAnchor)
            => AddIconButton(bar, region, LoadIcon(icon), tooltip, action, tooltipAnchor);

        private RoundedButton AddIconButton(NavigationBar bar, NavigationBarRegion region,
            Texture2D icon, string tooltip, Action action, TooltipAnchor tooltipAnchor)
        {
            var button = bar.AddRoundedButton(region, new NavigationBarButtonOptions
            {
                Icon = icon,
                IconSize = new Vector2(Config.Button.IconWidth, Config.Button.IconHeight),
                Width = Config.Button.Size,
                Height = Config.Button.Size,
                CornerRadius = Config.Button.CornerRadius,
                BackgroundColor = SkinV2Color.Parse(Config.Button.BackgroundColor),
                ForegroundColor = SkinV2Color.Parse(Config.Button.ForegroundColor),
                ClickAction = (sender, args) => action()
            });

            button.AddTooltip(new TooltipOptions(tooltip)
            {
                Anchor = tooltipAnchor,
                MaximumWidth = 240
            });

            return button;
        }

        private void EnsureOnlineHubSubscription()
        {
            var onlineHub = (GameBase.Game as QuaverGame)?.OnlineHub;
            if (ReferenceEquals(SubscribedOnlineHub, onlineHub))
                return;

            if (SubscribedOnlineHub != null)
                SubscribedOnlineHub.UnreadStateChanged -= OnHubUnreadStateChanged;

            SubscribedOnlineHub = onlineHub;
            if (SubscribedOnlineHub != null)
                SubscribedOnlineHub.UnreadStateChanged += OnHubUnreadStateChanged;

            UpdateHubIcon();
        }

        private void OnHubUnreadStateChanged(object sender, EventArgs args) => UpdateHubIcon();

        private void UpdateHubIcon()
        {
            var icon = SubscribedOnlineHub?.HasUnreadSections == true ? HubListIcon : HubMenuIcon;
            if (HubButton.Icon.Image != icon)
                HubButton.Icon.Image = icon;
        }

        private Texture2D LoadIcon(NavigationIcon icon)
        {
            var source = TextureManager.Load("Quaver.Resources/Textures/UI/Screens/Main/icons.png");
            var sourceRectangle = new Rectangle(0, (int) icon * IconPitch, IconWidth, IconHeight);
            var pixels = new Color[IconWidth * IconHeight];
            source.GetData(0, sourceRectangle, pixels, 0, pixels.Length);

            var texture = new Texture2D(GameBase.Game.GraphicsDevice, IconWidth, IconHeight);
            texture.SetData(pixels);
            OwnedIcons.Add(texture);
            return texture;
        }

        private void ResizeToWindow()
        {
            if (Math.Abs(Width - WindowManager.Width) > 0.001f ||
                Math.Abs(Height - WindowManager.Height) > 0.001f)
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);

            if (Math.Abs(TopBar.Width - WindowManager.Width) > 0.001f)
            {
                TopBar.Width = WindowManager.Width;
                BottomBar.Width = WindowManager.Width;
            }
        }

        private void ResetTransientState()
        {
            ProfileButton.ResetTransientState();
        }

        private static void OpenMusicPlayer()
        {
            if (GameBase.Game is QuaverGame game)
                game.CurrentScreen?.Exit(() => QuaverScreenFactory.CreateMusicPlayer());
        }

        private static void ToggleChat()
        {
            if (!(GameBase.Game is QuaverGame game) || game.OnlineChat == null)
                return;

            if (game.OnlineChat.IsOpen)
                game.OnlineChat.Close();
            else
                game.OnlineChat.Open();
        }

        private static void ShowDonateMessage() => NotificationManager.Show(NotificationLevel.Info,
            "Donating is currently unavailable from in-game and can only be done on the website.\n\n" +
            "We are working on adding this back soon.");

        private static void ShowVolume()
        {
            if (GameBase.Game is QuaverGame game)
                game.VolumeController?.Show();
        }

        private static void ToggleOnlineHub()
        {
            if (DialogManager.Dialogs.Count == 0)
            {
                DialogManager.Show(new OnlineHubDialog());
                return;
            }

            var topDialog = DialogManager.Dialogs[DialogManager.Dialogs.Count - 1];
            if (topDialog is OnlineHubDialog dialog)
                dialog.Close();
        }

        /// <summary>
        ///     Replacement-screen account control. This deliberately avoids the legacy menu-border drawable,
        ///     whose transparent root sprite is styled by the legacy header.
        /// </summary>
        private sealed class ProfileControl : RoundedButton
        {
            private SkinV2ProfileConfig Config { get; }

            private Texture2D OfflineAvatar { get; }

            private RoundedAvatar Avatar { get; }

            private Sprite Flag { get; }

            private ClanTag Clan { get; }

            private SpriteTextPlus Username { get; }

            private RoundedButton StatusBorder { get; }

            private RoundedButton StatusDot { get; }

            private bool IsOpen { get; set; }

            private bool LastConnected { get; set; }

            private object LastUser { get; set; }

            private string LastUsername { get; set; }

            public ProfileControl(SkinV2ProfileConfig config, Color backgroundColor, Texture2D offlineAvatar,
                float buttonSize)
            {
                Config = config;
                OfflineAvatar = offlineAvatar;
                Size = new ScalableVector2(Config.Width, buttonSize);
                Tint = backgroundColor;
                CornerRadius = Config.CornerRadius;
                PerformHoverFade = true;

                Avatar = new RoundedAvatar(buttonSize, Config.CornerRadius, GetAvatar())
                {
                    Parent = this,
                    Alignment = Alignment.MidLeft,
                    X = 0
                };

                StatusBorder = new RoundedButton
                {
                    Parent = Avatar,
                    Alignment = Alignment.BotRight,
                    Position = new ScalableVector2(0, 0),
                    Size = new ScalableVector2(Config.StatusBorderSize, Config.StatusBorderSize),
                    Tint = backgroundColor,
                    IsClickable = false,
                    PerformHoverFade = false
                };

                StatusDot = new RoundedButton
                {
                    Parent = StatusBorder,
                    Alignment = Alignment.MidCenter,
                    Size = new ScalableVector2(Config.StatusDotSize, Config.StatusDotSize),
                    IsClickable = false,
                    PerformHoverFade = false
                };

                Flag = new Sprite
                {
                    Parent = this,
                    Alignment = Alignment.MidLeft,
                    X = Config.FlagX,
                    Size = new ScalableVector2(Config.FlagSize, Config.FlagSize),
                    Image = Flags.Get("XX"),
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

                Clicked += (sender, args) => ToggleAccountDropdown();
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
                    LastUsername != username)
                    UpdateProfile();

                if (IsOpen && MouseManager.IsUniqueClick(MouseButton.Left) && !IsHovered &&
                    GameBase.Game is QuaverGame game &&
                    game.CurrentScreen?.ActiveLoggedInUserDropdown?.IsHovered() != true)
                    ToggleAccountDropdown();

                if (DialogManager.Dialogs.Count == 0 && KeyboardManager.IsUniqueKeyPress(Keys.F10))
                    ToggleAccountDropdown();

                base.Update(gameTime);
            }

            public override void Destroy()
            {
                ConfigManager.Username.ValueChanged -= OnUsernameChanged;
                OnlineManager.Status.ValueChanged -= OnOnlineStatusChanged;
                SteamManager.SteamUserAvatarLoaded -= OnSteamAvatarLoaded;
                base.Destroy();
            }

            public void ResetTransientState() => IsOpen = false;

            private void ToggleAccountDropdown()
            {
                if (!(GameBase.Game is QuaverGame game) || game.CurrentScreen == null)
                    return;

                IsOpen = !IsOpen;

                if (!IsOpen)
                {
                    game.CurrentScreen.ActiveLoggedInUserDropdown?.Close();
                    return;
                }

                if (game.CurrentScreen.ActiveLoggedInUserDropdown == null)
                {
                    game.CurrentScreen.ActivateLoggedInUserDropdown(new LoggedInUserDropdown(),
                        new ScalableVector2(
                            AbsolutePosition.X + AbsoluteSize.X - LoggedInUserDropdown.ContainerSize.X.Value,
                            AbsolutePosition.Y + AbsoluteSize.Y + Config.DropdownGap));
                    return;
                }

                game.CurrentScreen.ActiveLoggedInUserDropdown.Open();
            }

            private void UpdateProfile()
            {
                var connected = OnlineManager.Connected;
                var user = OnlineManager.Self;
                var username = GetDisplayUsername(connected);

                Avatar.AvatarSprite.Image = GetAvatar();
                StatusDot.Tint = connected
                    ? Color.White
                    : SkinV2Color.Parse(Config.OfflineStatusColor);

                if (connected)
                {
                    Flag.Image = Flags.Get(user?.OnlineUser?.CountryFlag ?? "XX");
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
                Username.TruncateWithEllipsis((int) Math.Max(40,
                    Config.Width - usernameX - Config.UsernameRightPadding));

                LastConnected = connected;
                LastUser = user;
                LastUsername = username;
            }

            private static string GetDisplayUsername(bool connected) => connected
                ? OnlineManager.Self?.OnlineUser?.Username ?? ConfigManager.Username.Value ?? "Player"
                : "Login";

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

            private void OnUsernameChanged(object sender, BindableValueChangedEventArgs<string> args) =>
                UpdateProfile();

            private void OnOnlineStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> args) =>
                UpdateProfile();

            private void OnSteamAvatarLoaded(object sender, SteamAvatarLoadedEventArgs args)
            {
                if (SteamUser.GetSteamID().m_SteamID == args.SteamId)
                    Avatar.AvatarSprite.Image = args.Texture;
            }
        }

        private sealed class RoundedAvatar : SpriteMaskContainer
        {
            public Sprite AvatarSprite { get; }

            public RoundedAvatar(float size, float cornerRadius, Texture2D image)
            {
                Size = new ScalableVector2(size, size);
                Image = RoundedRectTextureCache.Get(size, size, cornerRadius);

                AvatarSprite = new Sprite
                {
                    Alignment = Alignment.TopLeft,
                    Size = Size,
                    Image = image
                };

                AddContainedSprite(AvatarSprite);
            }
        }
    }
}
