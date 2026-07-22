using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Server.Client;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Menu.Border.Components.Users;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Steamworks;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     Account dropdown used by replacement-screen navigation.
    /// </summary>
    internal sealed class LoggedInUserDropdown : LoggedInUserDropdownBase
    {
        public static ScalableVector2 ContainerSize { get; } = new ScalableVector2(526, 145);

        private const float ConnectedHeight = 145;
        private const float OfflineHeight = 68;
        private const int IconCellSize = 40;

        private static readonly Color UpperPanelColor = ColorHelper.HexToColor("#555555");
        private static readonly Color ActionPanelColor = ColorHelper.HexToColor("#444444");
        private static readonly Color ActionButtonColor = ColorHelper.HexToColor("#999999");
        private static readonly Color StatsPanelColor = ColorHelper.HexToColor("#8D8D8D");
        private static readonly Color StatPillColor = ColorHelper.HexToColor("#CDCDCD");
        private static readonly Color RolePillColor = ColorHelper.HexToColor("#929292");
        private static readonly Color ModeBackgroundColor = ColorHelper.HexToColor("#555555");

        public override ImageButton Button { get; }

        private Sprite ScreenDarkness { get; }

        private Container ConnectedPanel { get; }

        private Container OfflinePanel { get; }

        private FlexContainer ConnectedLayout { get; }

        private FlexContainer UpperLayout { get; }

        private FlexContainer ProfileLayout { get; }

        private FlexContainer InfoLayout { get; }

        private FlexContainer IdentityLayout { get; }

        private FlexContainer ActionsLayout { get; }

        private FlexContainer ActionButtonsLayout { get; }

        private FlexContainer StatsLayout { get; }

        private FlexContainer OfflineLayout { get; }

        private FlexContainer OfflineInfoLayout { get; }

        private FlexContainer OfflineActionLayout { get; }

        private Container UsernameHost { get; }

        private Container StatusHost { get; }

        private Container ClanUsernameSpacer { get; }

        private FlexItemOptions LoginButtonOptions { get; }

        private FlexItemOptions LoginWheelOptions { get; }

        private RoundedAvatar Avatar { get; }

        private Sprite Flag { get; }

        private ClanTag Clan { get; }

        private SpriteTextPlus Username { get; }

        private SpriteTextPlus Status { get; }

        private RoundedButton RolePill { get; }

        private RoundedButton ProfileButton { get; }

        private RoundedButton LogoutButton { get; }

        private RoundedButton RankPill { get; }

        private RoundedButton RatingPill { get; }

        private RoundedButton AccuracyPill { get; }

        private RoundedButton ModeButton { get; }

        private Sprite ModeSelection { get; }

        private SpriteTextPlus ModeLabel { get; }

        private RoundedAvatar OfflineAvatar { get; }

        private SpriteTextPlus OfflineTitle { get; }

        private SpriteTextPlus OfflineStatus { get; }

        private RoundedButton LoginButton { get; }

        private LoadingWheel LoginWheel { get; }

        private List<Texture2D> OwnedIcons { get; } = new List<Texture2D>();

        private User User { get; set; }

        private OnlineClient SubscribedClient { get; set; }

        private GameMode ActiveMode { get; set; } = GameMode.Keys4;

        private bool IsOpen { get; set; }

        private bool StateDirty { get; set; } = true;

        private float ActiveContentHeight { get; set; } = OfflineHeight;

        public LoggedInUserDropdown()
            : base(new ScalableVector2(ContainerSize.X.Value, 0), ContainerSize)
        {
            Tint = Color.Black;
            Alpha = 0;
            Scrollbar.Visible = false;

            if (ConfigManager.SelectedGameMode != null)
                ActiveMode = ConfigManager.SelectedGameMode.Value;

            var game = GameBase.Game as QuaverGame;
            ScreenDarkness = new Sprite
            {
                Parent = game?.CurrentScreen?.View.Container,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height),
                Tint = Color.Black,
                Alpha = 0
            };

            Button = new ImageButton(UserInterface.BlankBox)
            {
                Parent = this,
                Alpha = 0,
                UsePreviousSpriteBatchOptions = true
            };

            ConnectedPanel = CreateConnectedPanel();
            OfflinePanel = CreateOfflinePanel();

            ConnectedLayout = CreateFlex(ConnectedPanel, ContainerSize, FlexDirection.Column,
                FlexJustifyContent.FlexStart, FlexAlignItems.Stretch, 5);
            UpperLayout = CreateFlex(ConnectedLayout, new ScalableVector2(526, 100), FlexDirection.Row,
                FlexJustifyContent.FlexStart, FlexAlignItems.Stretch);
            ConnectedLayout.SetItemOptions(UpperLayout, FixedBasis(100));
            ProfileLayout = CreateFlex(UpperLayout, new ScalableVector2(394, 100), FlexDirection.Row,
                FlexJustifyContent.FlexStart, FlexAlignItems.Center);
            UpperLayout.SetItemOptions(ProfileLayout, FixedBasis(394));
            ActionsLayout = CreateFlex(UpperLayout, new ScalableVector2(132, 100), FlexDirection.Column,
                FlexJustifyContent.FlexStart, FlexAlignItems.Stretch);
            UpperLayout.SetItemOptions(ActionsLayout, FixedBasis(132));

            CreateSpacer(ProfileLayout, 10, 100);
            Avatar = CreateAvatar(ProfileLayout, 80, GetAvatar());
            CreateSpacer(ProfileLayout, 10, 100);
            InfoLayout = CreateFlex(ProfileLayout, new ScalableVector2(284, 80), FlexDirection.Column,
                FlexJustifyContent.Center, FlexAlignItems.FlexStart, 2);
            ProfileLayout.SetItemOptions(InfoLayout, FixedBasis(284));
            CreateSpacer(ProfileLayout, 10, 100);

            IdentityLayout = CreateFlex(InfoLayout, new ScalableVector2(284, 22), FlexDirection.Row,
                FlexJustifyContent.FlexStart, FlexAlignItems.Center);
            InfoLayout.SetItemOptions(IdentityLayout, FixedBasis(22));
            Flag = new Sprite
            {
                Parent = IdentityLayout,
                Size = new ScalableVector2(22, 22),
                Image = Flags.Get("XX"),
                UsePreviousSpriteBatchOptions = true
            };
            CreateSpacer(IdentityLayout, 6, 22);
            Clan = new ClanTag(18)
            {
                Parent = IdentityLayout,
                UsePreviousSpriteBatchOptions = true
            };
            ClanUsernameSpacer = CreateSpacer(IdentityLayout, 0, 22);
            UsernameHost = new Container
            {
                Parent = IdentityLayout,
                Size = new ScalableVector2(40, 22)
            };
            IdentityLayout.SetItemOptions(UsernameHost, new FlexItemOptions
            {
                Basis = 40,
                Grow = 1,
                Shrink = 1
            });
            Username = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), string.Empty, 18)
            {
                Parent = UsernameHost,
                Tint = Color.White,
                UsePreviousSpriteBatchOptions = true
            };

            StatusHost = new Container
            {
                Parent = InfoLayout,
                Size = new ScalableVector2(284, 20)
            };
            InfoLayout.SetItemOptions(StatusHost, FixedBasis(20));
            Status = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), string.Empty, 16)
            {
                Parent = StatusHost,
                Tint = Color.White,
                UsePreviousSpriteBatchOptions = true
            };

            RolePill = CreateRolePill(InfoLayout);
            InfoLayout.SetItemOptions(RolePill, FixedBasis(25));

            CreateSpacer(ActionsLayout, 132, 60);
            ActionButtonsLayout = CreateFlex(ActionsLayout, new ScalableVector2(132, 30), FlexDirection.Row,
                FlexJustifyContent.FlexStart, FlexAlignItems.Center);
            ActionsLayout.SetItemOptions(ActionButtonsLayout, FixedBasis(30));
            CreateSpacer(ActionButtonsLayout, 52, 30);
            ProfileButton = CreateActionButton(ActionButtonsLayout, CropIcon(0, 280), OpenProfile);
            CreateSpacer(ActionButtonsLayout, 10, 30);
            LogoutButton = CreateActionButton(ActionButtonsLayout, CropIcon(40, 280), Logout);
            CreateSpacer(ActionButtonsLayout, 10, 30);
            CreateSpacer(ActionsLayout, 132, 10);

            StatsLayout = CreateFlex(ConnectedLayout, new ScalableVector2(526, 40), FlexDirection.Row,
                FlexJustifyContent.FlexStart, FlexAlignItems.Center);
            ConnectedLayout.SetItemOptions(StatsLayout, FixedBasis(40));
            CreateSpacer(StatsLayout, 10, 40);
            RankPill = CreateStatPill(StatsLayout, 121, CropIcon(0, 0));
            CreateSpacer(StatsLayout, 10, 40);
            RatingPill = CreateStatPill(StatsLayout, 95, CropIcon(0, 120));
            CreateSpacer(StatsLayout, 10, 40);
            AccuracyPill = CreateStatPill(StatsLayout, 102, CropIcon(40, 240));
            var statsSpacer = CreateSpacer(StatsLayout, 1, 40);
            StatsLayout.SetItemOptions(statsSpacer, new FlexItemOptions { Basis = 1, Grow = 1, Shrink = 1 });

            ModeButton = new RoundedButton((sender, args) => ToggleMode())
            {
                Parent = StatsLayout,
                Size = new ScalableVector2(70, 20),
                CornerRadius = 10,
                Tint = ModeBackgroundColor,
                UsePreviousSpriteBatchOptions = true
            };
            StatsLayout.SetItemOptions(ModeButton, FixedBasis(70));
            ModeSelection = CreateRoundedSprite(40, 16, 8, Color.White);
            ModeSelection.Parent = ModeButton;
            ModeSelection.Alignment = Alignment.MidLeft;
            ModeSelection.X = 2;
            ModeLabel = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), "4K", 13)
            {
                Parent = ModeSelection,
                Alignment = Alignment.MidCenter,
                Tint = ActionPanelColor,
                UsePreviousSpriteBatchOptions = true
            };
            CreateSpacer(StatsLayout, 10, 40);

            OfflineLayout = CreateFlex(OfflinePanel, new ScalableVector2(526, OfflineHeight), FlexDirection.Row,
                FlexJustifyContent.FlexStart, FlexAlignItems.Center);
            CreateSpacer(OfflineLayout, 10, OfflineHeight);
            OfflineAvatar = CreateAvatar(OfflineLayout, 48, UserInterface.OfflineAvatar);
            CreateSpacer(OfflineLayout, 12, OfflineHeight);
            OfflineInfoLayout = CreateFlex(OfflineLayout, new ScalableVector2(100, 48), FlexDirection.Column,
                FlexJustifyContent.Center, FlexAlignItems.FlexStart, 2);
            OfflineLayout.SetItemOptions(OfflineInfoLayout,
                new FlexItemOptions { Basis = 100, Grow = 1, Shrink = 1 });
            OfflineTitle = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), "Login", 18)
            {
                Parent = OfflineInfoLayout,
                Tint = Color.White,
                UsePreviousSpriteBatchOptions = true
            };
            OfflineStatus = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold), string.Empty, 14)
            {
                Parent = OfflineInfoLayout,
                Tint = Color.White,
                UsePreviousSpriteBatchOptions = true
            };
            CreateSpacer(OfflineLayout, 10, OfflineHeight);
            OfflineActionLayout = CreateFlex(OfflineLayout, new ScalableVector2(40, 40), FlexDirection.Row,
                FlexJustifyContent.Center, FlexAlignItems.Center);
            OfflineLayout.SetItemOptions(OfflineActionLayout, FixedBasis(40));
            LoginButton = new RoundedButton((sender, args) => OnlineManager.Login())
            {
                Parent = OfflineActionLayout,
                Size = new ScalableVector2(40, 40),
                CornerRadius = 5,
                Tint = ActionButtonColor,
                UsePreviousSpriteBatchOptions = true
            };
            LoginButton.SetIcon(CropIcon(0, 320), new Vector2(24, 24));
            LoginButtonOptions = FixedBasis(40);
            OfflineActionLayout.SetItemOptions(LoginButton, LoginButtonOptions);
            LoginWheel = new LoadingWheel
            {
                Parent = OfflineActionLayout,
                Size = new ScalableVector2(24, 24),
                Visible = false,
                UsePreviousSpriteBatchOptions = true
            };
            LoginWheelOptions = FixedBasis(0);
            OfflineActionLayout.SetItemOptions(LoginWheel, LoginWheelOptions);
            CreateSpacer(OfflineLayout, 10, OfflineHeight);

            AddContainedDrawable(ConnectedPanel);
            AddContainedDrawable(OfflinePanel);
            RefreshLayouts();

            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
            if (ConfigManager.SelectedGameMode != null)
                ConfigManager.SelectedGameMode.ValueChanged += OnSelectedModeChanged;
            SteamManager.SteamUserAvatarLoaded += OnSteamAvatarLoaded;

            SubscribeToCurrentClient();
            RefreshState();
            Open();
        }

        public override void Update(GameTime gameTime)
        {
            ScreenDarkness.Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            SubscribeToCurrentClient();

            if (StateDirty || !ReferenceEquals(User, OnlineManager.Self))
                RefreshState();

            var childHovered = ConnectedPanel.Visible &&
                               (ProfileButton.IsHovered || LogoutButton.IsHovered || ModeButton.IsHovered) ||
                               OfflinePanel.Visible && LoginButton.Visible && LoginButton.IsHovered;
            Button.Depth = childHovered ? 1 : 0;
            Button.Size = new ScalableVector2(ContainerSize.X.Value, ActiveContentHeight);

            base.Update(gameTime);
        }

        public override void Destroy()
        {
            OnlineManager.Status.ValueChanged -= OnConnectionStatusChanged;
            if (ConfigManager.SelectedGameMode != null)
                ConfigManager.SelectedGameMode.ValueChanged -= OnSelectedModeChanged;
            SteamManager.SteamUserAvatarLoaded -= OnSteamAvatarLoaded;
            UnsubscribeFromClient();
            ScreenDarkness.Destroy();

            base.Destroy();

            foreach (var icon in OwnedIcons)
                icon?.Dispose();

            OwnedIcons.Clear();
        }

        public override void Open()
        {
            IsOpen = true;
            RefreshState();
            ClearAnimations();
            ChangeHeightTo((int) ActiveContentHeight, Easing.OutQuint, 450);

            ScreenDarkness.ClearAnimations();
            ScreenDarkness.FadeTo(0.75f, Easing.Linear, 200);
        }

        public override void Close()
        {
            IsOpen = false;
            ClearAnimations();
            ChangeHeightTo(0, Easing.OutQuint, 550);

            ScreenDarkness.ClearAnimations();
            ScreenDarkness.FadeTo(0, Easing.Linear, 200);
        }

        private Container CreateConnectedPanel()
        {
            var panel = new Container
            {
                Size = ContainerSize
            };

            var backgroundLayout = CreateFlex(panel, ContainerSize, FlexDirection.Column,
                FlexJustifyContent.FlexStart, FlexAlignItems.Stretch, 5);
            var upperBackgroundLayout = CreateFlex(backgroundLayout, new ScalableVector2(526, 100),
                FlexDirection.Row, FlexJustifyContent.FlexStart, FlexAlignItems.Stretch);
            backgroundLayout.SetItemOptions(upperBackgroundLayout, FixedBasis(100));

            var profileBackground = CreateRoundedSprite(394, 100, 6, UpperPanelColor);
            profileBackground.Parent = upperBackgroundLayout;
            upperBackgroundLayout.SetItemOptions(profileBackground, FixedBasis(394));

            var actionsBackground = CreateRoundedSprite(132, 100, 6, ActionPanelColor);
            actionsBackground.Parent = upperBackgroundLayout;
            upperBackgroundLayout.SetItemOptions(actionsBackground, FixedBasis(132));

            var statsBackground = CreateRoundedSprite(526, 40, 6, StatsPanelColor);
            statsBackground.Parent = backgroundLayout;
            backgroundLayout.SetItemOptions(statsBackground, FixedBasis(40));
            return panel;
        }

        private Container CreateOfflinePanel()
        {
            var panel = new Container
            {
                Size = new ScalableVector2(526, OfflineHeight)
            };

            var backgroundLayout = CreateFlex(panel, new ScalableVector2(526, OfflineHeight), FlexDirection.Row,
                FlexJustifyContent.FlexStart, FlexAlignItems.Stretch);
            var background = CreateRoundedSprite(526, OfflineHeight, 6, UpperPanelColor);
            background.Parent = backgroundLayout;
            backgroundLayout.SetItemOptions(background, FixedBasis(526));
            return panel;
        }

        private RoundedButton CreateRolePill(Drawable parent)
        {
            var pill = new RoundedButton
            {
                Parent = parent,
                Size = new ScalableVector2(150, 25),
                WidthMode = ButtonSizeMode.Auto,
                AutoSizePadding = new Vector2(22, 0),
                CornerRadius = 12.5f,
                Tint = RolePillColor,
                IsInteractionEnabled = false,
                UsePreviousSpriteBatchOptions = true
            };
            pill.SetIcon(CropIcon(40, 360), new Vector2(16, 16));
            pill.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), string.Empty, 14, Color.White);
            return pill;
        }

        private RoundedButton CreateActionButton(Drawable parent, Texture2D icon, Action action)
        {
            var button = new RoundedButton((sender, args) => action())
            {
                Parent = parent,
                Size = new ScalableVector2(30, 30),
                CornerRadius = 5,
                Tint = ActionButtonColor,
                UsePreviousSpriteBatchOptions = true
            };
            button.SetIcon(icon, new Vector2(20, 20));
            return button;
        }

        private RoundedButton CreateStatPill(FlexContainer parent, float width, Texture2D icon)
        {
            var pill = new RoundedButton
            {
                Parent = parent,
                Size = new ScalableVector2(width, 30),
                CornerRadius = 15,
                Tint = StatPillColor,
                IsInteractionEnabled = false,
                UsePreviousSpriteBatchOptions = true
            };
            pill.SetIcon(icon, new Vector2(22, 22));
            pill.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), "--", 15, Color.White);
            parent.SetItemOptions(pill, FixedBasis(width));
            return pill;
        }

        private static RoundedAvatar CreateAvatar(Drawable parent, float size, Texture2D image) =>
            new RoundedAvatar(size, 6, image)
        {
            Parent = parent,
            UsePreviousSpriteBatchOptions = true
        };

        private static FlexContainer CreateFlex(Drawable parent, ScalableVector2 size, FlexDirection direction,
            FlexJustifyContent justifyContent, FlexAlignItems alignItems, float gap = 0) => new FlexContainer
        {
            Parent = parent,
            Size = size,
            Direction = direction,
            JustifyContent = justifyContent,
            AlignItems = alignItems,
            Gap = gap,
            UsePreviousSpriteBatchOptions = true
        };

        private static Container CreateSpacer(Drawable parent, float width, float height) => new Container
        {
            Parent = parent,
            Size = new ScalableVector2(width, height),
            UsePreviousSpriteBatchOptions = true
        };

        private static FlexItemOptions FixedBasis(float basis) => new FlexItemOptions
        {
            Basis = basis,
            Grow = 0,
            Shrink = 0
        };

        private static Sprite CreateRoundedSprite(float width, float height, float radius, Color tint) => new Sprite
        {
            Size = new ScalableVector2(width, height),
            Image = RoundedRectTextureCache.Get(width, height, radius),
            Tint = tint,
            UsePreviousSpriteBatchOptions = true
        };

        private void RefreshLayouts()
        {
            ConnectedLayout.RefreshLayout();
            UpperLayout.RefreshLayout();
            ProfileLayout.RefreshLayout();
            InfoLayout.RefreshLayout();
            IdentityLayout.RefreshLayout();
            ActionsLayout.RefreshLayout();
            ActionButtonsLayout.RefreshLayout();
            StatsLayout.RefreshLayout();
            OfflineLayout.RefreshLayout();
            OfflineInfoLayout.RefreshLayout();
            OfflineActionLayout.RefreshLayout();
        }

        private Texture2D CropIcon(int x, int y)
        {
            var source = UserInterface.MainMenuScreenIcons2;
            var sourceRectangle = new Rectangle(x, y, IconCellSize, IconCellSize);
            var pixels = new Color[IconCellSize * IconCellSize];
            source.GetData(0, sourceRectangle, pixels, 0, pixels.Length);

            var texture = new Texture2D(GameBase.Game.GraphicsDevice, IconCellSize, IconCellSize);
            texture.SetData(pixels);
            OwnedIcons.Add(texture);
            return texture;
        }

        private void RefreshState()
        {
            StateDirty = false;
            User = OnlineManager.Self;
            var connected = OnlineManager.Connected && User?.OnlineUser != null;

            ConnectedPanel.Visible = connected;
            OfflinePanel.Visible = !connected;
            ProfileButton.Visible = connected;
            ProfileButton.IsClickable = connected;
            LogoutButton.Visible = connected;
            LogoutButton.IsClickable = connected;
            ModeButton.Visible = connected;
            ModeButton.IsClickable = connected;
            var previousHeight = ActiveContentHeight;
            ActiveContentHeight = connected ? ConnectedHeight : OfflineHeight;

            if (IsOpen && Math.Abs(previousHeight - ActiveContentHeight) > 0.001f)
            {
                ClearAnimations();
                ChangeHeightTo((int) ActiveContentHeight, Easing.OutQuint, 250);
            }

            if (connected)
                RefreshConnectedState();
            else
                RefreshOfflineState();
        }

        private void RefreshConnectedState()
        {
            LoginButton.Visible = false;
            LoginButton.IsClickable = false;
            LoginWheel.Visible = false;
            Avatar.SetSource(GetAvatar());
            Flag.Image = Flags.Get(User.OnlineUser.CountryFlag ?? "XX");
            Clan.UpdateFromUser(User.OnlineUser, Color.White);
            ClanUsernameSpacer.Width = Clan.Visible ? 7 : 0;

            Username.Text = User.OnlineUser.Username ?? ConfigManager.Username.Value ?? "Player";

            Status.Text = GetStatusText();

            var role = GetRole(User.OnlineUser.UserGroups);
            RolePill.Visible = role != null;
            if (role != null)
            {
                RolePill.Label.Text = role;
                RolePill.RecalculateAutoSize();
            }

            RankPill.Label.Text = "--";
            RatingPill.Label.Text = "--";
            AccuracyPill.Label.Text = "--";

            if (User.Stats.TryGetValue(ActiveMode, out var stats))
            {
                RankPill.Label.Text = $"#{stats.Rank:n0}";
                RatingPill.Label.Text = StringHelper.RatingToString(stats.OverallPerformanceRating);
                AccuracyPill.Label.Text = StringHelper.AccuracyToString((float) stats.OverallAccuracy);
            }

            var is4K = ActiveMode == GameMode.Keys4;
            ModeSelection.Alignment = is4K ? Alignment.MidLeft : Alignment.MidRight;
            ModeSelection.X = is4K ? 2 : -2;
            ModeLabel.Text = is4K ? "4K" : "7K";

            RefreshLayouts();
            Username.TruncateWithEllipsis((int) Math.Max(40, UsernameHost.Width));
            Status.TruncateWithEllipsis((int) StatusHost.Width);
        }

        private void RefreshOfflineState()
        {
            OfflineAvatar.SetSource(UserInterface.OfflineAvatar);
            OfflineStatus.Text = GetConnectionStatusText();
            OfflineStatus.TruncateWithEllipsis(385);

            var canLogin = OnlineManager.Status.Value == ConnectionStatus.Disconnected;
            LoginButton.Visible = canLogin;
            LoginButton.IsClickable = canLogin;
            LoginWheel.Visible = !canLogin;
            LoginButtonOptions.Basis = canLogin ? 40 : 0;
            LoginWheelOptions.Basis = canLogin ? 0 : 24;

            RefreshLayouts();
            OfflineStatus.TruncateWithEllipsis((int) OfflineInfoLayout.Width);
        }

        private void ToggleMode()
        {
            ActiveMode = ActiveMode == GameMode.Keys4 ? GameMode.Keys7 : GameMode.Keys4;

            if (ConfigManager.SelectedGameMode != null)
                ConfigManager.SelectedGameMode.Value = ActiveMode;

            StateDirty = true;
        }

        private void OpenProfile()
        {
            if (User?.OnlineUser != null)
                BrowserHelper.OpenURL($"https://quavergame.com/profile/{User.OnlineUser.Id}");
        }

        private static void Logout() => ThreadScheduler.Run(() => OnlineManager.Client?.Disconnect());

        private Texture2D GetAvatar()
        {
            var image = UserInterface.UnknownAvatar;
            if (User?.OnlineUser != null && SteamManager.UserAvatars != null &&
                SteamManager.UserAvatars.TryGetValue((ulong) User.OnlineUser.SteamId, out var avatar))
                image = avatar;

            return image;
        }

        private string GetStatusText()
        {
            switch (User?.CurrentStatus?.Status)
            {
                case ClientStatus.Selecting:
                    return "Selecting a song";
                case ClientStatus.Playing:
                case ClientStatus.Paused:
                    return "Playing";
                case ClientStatus.Watching:
                    return "Watching a replay";
                case ClientStatus.Editing:
                    return "Editing";
                case ClientStatus.InLobby:
                    return "Multiplayer Lobby";
                case ClientStatus.Multiplayer:
                    return "Playing multiplayer";
                case ClientStatus.Listening:
                    return "Listening";
                case ClientStatus.InMenus:
                case null:
                    return "Idle";
                default:
                    return "Idle";
            }
        }

        private static string GetConnectionStatusText()
        {
            switch (OnlineManager.Status.Value)
            {
                case ConnectionStatus.Connecting:
                    return "Connecting to the server...";
                case ConnectionStatus.Reconnecting:
                    return "Reconnecting to the server...";
                case ConnectionStatus.Connected:
                    return "Connected!";
                default:
                    return "Disconnected from the server.";
            }
        }

        private static string GetRole(UserGroups groups)
        {
            if (groups.HasFlag(UserGroups.Swan))
                return "Swan";
            if (groups.HasFlag(UserGroups.Developer))
                return "Developer";
            if (groups.HasFlag(UserGroups.Bot))
                return "Bot";
            if (groups.HasFlag(UserGroups.Admin))
                return "Administrator";
            if (groups.HasFlag(UserGroups.Moderator))
                return "Moderator";
            if (groups.HasFlag(UserGroups.RankingSupervisor))
                return "Ranking Supervisor";
            if (groups.HasFlag(UserGroups.Contributor))
                return "Contributor";
            if (groups.HasFlag(UserGroups.Donator))
                return "Donator";
            return null;
        }

        private void SubscribeToCurrentClient()
        {
            if (ReferenceEquals(SubscribedClient, OnlineManager.Client))
                return;

            UnsubscribeFromClient();
            SubscribedClient = OnlineManager.Client;
            if (SubscribedClient == null)
                return;

            SubscribedClient.OnLoginSuccess += OnLoginSuccess;
            SubscribedClient.OnUserStatusReceived += OnUserStatusReceived;
        }

        private void UnsubscribeFromClient()
        {
            if (SubscribedClient == null)
                return;

            SubscribedClient.OnLoginSuccess -= OnLoginSuccess;
            SubscribedClient.OnUserStatusReceived -= OnUserStatusReceived;
            SubscribedClient = null;
        }

        private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> args) =>
            StateDirty = true;

        private void OnSelectedModeChanged(object sender, BindableValueChangedEventArgs<GameMode> args)
        {
            ActiveMode = args.Value;
            StateDirty = true;
        }

        private void OnSteamAvatarLoaded(object sender, SteamAvatarLoadedEventArgs args)
        {
            if (User?.OnlineUser != null && (ulong) User.OnlineUser.SteamId == args.SteamId)
                StateDirty = true;
        }

        private void OnLoginSuccess(object sender, LoginReplyEventArgs args) => StateDirty = true;

        private void OnUserStatusReceived(object sender, UserStatusEventArgs args) => StateDirty = true;

        /// <summary>
        ///     Uses a pre-masked texture instead of SpriteMaskContainer so the avatar cannot replace the
        ///     ScrollContainer's scissor batch or leak out while the dropdown height is animated to zero.
        /// </summary>
        private sealed class RoundedAvatar : Sprite
        {
            private float CornerRadius { get; }

            private Texture2D Source { get; set; }

            private Texture2D RoundedTexture { get; set; }

            public RoundedAvatar(float size, float cornerRadius, Texture2D image)
            {
                Size = new ScalableVector2(size, size);
                CornerRadius = cornerRadius;
                SetSource(image);
            }

            public void SetSource(Texture2D source)
            {
                if (source == null || source.IsDisposed || ReferenceEquals(Source, source))
                    return;

                Source = source;
                RoundedTexture?.Dispose();
                RoundedTexture = CreateRoundedTexture(source, Math.Max(1, (int) Math.Ceiling(Width)),
                    CornerRadius);
                Image = RoundedTexture ?? source;
            }

            public override void Destroy()
            {
                RoundedTexture?.Dispose();
                RoundedTexture = null;
                Source = null;
                base.Destroy();
            }

            private static Texture2D CreateRoundedTexture(Texture2D source, int size, float radius)
            {
                try
                {
                    var sourcePixels = new Color[source.Width * source.Height];
                    source.GetData(sourcePixels);
                    var pixels = new Color[size * size];
                    var halfSize = size / 2f;

                    for (var y = 0; y < size; y++)
                    {
                        var sourceY = Math.Min(source.Height - 1, y * source.Height / size);

                        for (var x = 0; x < size; x++)
                        {
                            var sourceX = Math.Min(source.Width - 1, x * source.Width / size);
                            var color = sourcePixels[sourceY * source.Width + sourceX];
                            var qx = Math.Abs(x + 0.5f - halfSize) - (halfSize - radius);
                            var qy = Math.Abs(y + 0.5f - halfSize) - (halfSize - radius);
                            var outsideDistance = Math.Sqrt(Math.Max(qx, 0) * Math.Max(qx, 0) +
                                                            Math.Max(qy, 0) * Math.Max(qy, 0));
                            var distance = outsideDistance + Math.Min(Math.Max(qx, qy), 0) - radius;
                            var coverage = 1 - MathHelper.SmoothStep(-1, 0, (float) distance);
                            pixels[y * size + x] = new Color(color.R, color.G, color.B,
                                (byte) (color.A * coverage));
                        }
                    }

                    var texture = new Texture2D(GameBase.Game.GraphicsDevice, size, size);
                    texture.SetData(pixels);
                    return texture;
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }
        }
    }
}
