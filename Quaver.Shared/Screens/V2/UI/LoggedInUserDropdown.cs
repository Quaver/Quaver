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
using Quaver.Shared.Skinning;
using Quaver.Shared.Skinning.V2;
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
        public static ScalableVector2 ContainerSize
        {
            get
            {
                var config = SkinManager.SkinV2?.Config.Shared.Navigation.AccountDropdown ??
                             new SkinV2AccountDropdownConfig();
                return new ScalableVector2(config.Width, config.ConnectedHeight);
            }
        }

        private SkinStoreV2Lease Skin { get; }

        private SkinV2AccountDropdownConfig Config { get; }

        private Texture2D OfflineAvatarTexture { get; }

        private Texture2D UnknownAvatarTexture { get; }

        public override ImageButton Button { get; }

        private Sprite ScreenDarkness { get; }

        private Container ConnectedPanel { get; }

        private Container OfflinePanel { get; }

        private RoundedProfileBackground ProfileBackground { get; set; }

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

        private float ActiveContentHeight { get; set; }

        private float CloseAnimationRemaining { get; set; }

        private int? RequestedProfileCoverUserId { get; set; }

        public LoggedInUserDropdown()
            : base(new ScalableVector2(ContainerSize.X.Value, 0), ContainerSize)
        {
            Skin = SkinManager.AcquireV2();
            Config = Skin.Config.Shared.Navigation.AccountDropdown;
            OfflineAvatarTexture = UserInterface.OfflineAvatar;
            UnknownAvatarTexture = UserInterface.UnknownAvatar;
            ActiveContentHeight = Config.OfflineHeight;
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
                FlexJustifyContent.FlexStart, FlexAlignItems.Stretch, Config.PanelGap);
            UpperLayout = CreateFlex(ConnectedLayout, new ScalableVector2(Config.Width, Config.UpperHeight), FlexDirection.Row,
                FlexJustifyContent.FlexStart, FlexAlignItems.Stretch);
            ConnectedLayout.SetItemOptions(UpperLayout, FixedBasis(Config.UpperHeight));
            ProfileLayout = CreateFlex(UpperLayout, new ScalableVector2(Config.ProfileWidth, Config.UpperHeight), FlexDirection.Row,
                FlexJustifyContent.FlexStart, FlexAlignItems.Center);
            UpperLayout.SetItemOptions(ProfileLayout, FixedBasis(Config.ProfileWidth));
            ActionsLayout = CreateFlex(UpperLayout, new ScalableVector2(Config.ActionsWidth, Config.UpperHeight), FlexDirection.Column,
                FlexJustifyContent.FlexStart, FlexAlignItems.Stretch);
            UpperLayout.SetItemOptions(ActionsLayout, FixedBasis(Config.ActionsWidth));

            CreateSpacer(ProfileLayout, Config.ContentPadding, Config.UpperHeight);
            Avatar = CreateAvatar(ProfileLayout, Config.AvatarSize, GetAvatar());
            CreateSpacer(ProfileLayout, Config.ContentPadding, Config.UpperHeight);
            InfoLayout = CreateFlex(ProfileLayout, new ScalableVector2(Config.InfoWidth, Config.AvatarSize), FlexDirection.Column,
                FlexJustifyContent.Center, FlexAlignItems.FlexStart, Config.InfoGap);
            ProfileLayout.SetItemOptions(InfoLayout, FixedBasis(Config.InfoWidth));
            CreateSpacer(ProfileLayout, Config.ContentPadding, Config.UpperHeight);

            IdentityLayout = CreateFlex(InfoLayout, new ScalableVector2(Config.InfoWidth, Config.IdentityHeight), FlexDirection.Row,
                FlexJustifyContent.FlexStart, FlexAlignItems.Center);
            InfoLayout.SetItemOptions(IdentityLayout, FixedBasis(Config.IdentityHeight));
            Flag = new Sprite
            {
                Parent = IdentityLayout,
                Size = new ScalableVector2(Config.FlagSize, Config.FlagSize),
                Image = Flags.Get("XX"),
                UsePreviousSpriteBatchOptions = true
            };
            CreateSpacer(IdentityLayout, Config.IdentitySpacing, Config.IdentityHeight);
            Clan = new ClanTag(Config.UsernameFontSize)
            {
                Parent = IdentityLayout,
                UsePreviousSpriteBatchOptions = true
            };
            ClanUsernameSpacer = CreateSpacer(IdentityLayout, 0, Config.IdentityHeight);
            UsernameHost = new Container
            {
                Parent = IdentityLayout,
                Size = new ScalableVector2(40, Config.IdentityHeight)
            };
            IdentityLayout.SetItemOptions(UsernameHost, new FlexItemOptions
            {
                Basis = 40,
                Grow = 1,
                Shrink = 1
            });
            Username = new SpriteTextPlus(FontManager.GetWobbleFont(Config.PrimaryFont), string.Empty,
                Config.UsernameFontSize)
            {
                Parent = UsernameHost,
                Tint = SkinV2Color.Parse(Config.TextColor),
                UsePreviousSpriteBatchOptions = true
            };

            StatusHost = new Container
            {
                Parent = InfoLayout,
                Size = new ScalableVector2(Config.InfoWidth, Config.StatusHeight)
            };
            InfoLayout.SetItemOptions(StatusHost, FixedBasis(Config.StatusHeight));
            Status = new SpriteTextPlus(FontManager.GetWobbleFont(Config.PrimaryFont), string.Empty,
                Config.StatusFontSize)
            {
                Parent = StatusHost,
                Tint = SkinV2Color.Parse(Config.TextColor),
                UsePreviousSpriteBatchOptions = true
            };

            RolePill = CreateRolePill(InfoLayout);
            InfoLayout.SetItemOptions(RolePill, FixedBasis(Config.RoleHeight));

            CreateSpacer(ActionsLayout, Config.ActionsWidth, Config.ActionTopSpacer);
            ActionButtonsLayout = CreateFlex(ActionsLayout,
                new ScalableVector2(Config.ActionsWidth, Config.ActionButtonSize), FlexDirection.Row,
                FlexJustifyContent.FlexStart, FlexAlignItems.Center);
            ActionsLayout.SetItemOptions(ActionButtonsLayout, FixedBasis(Config.ActionButtonSize));
            CreateSpacer(ActionButtonsLayout, Config.ActionLeftSpacer, Config.ActionButtonSize);
            ProfileButton = CreateActionButton(ActionButtonsLayout,
                LoadIcon(0, 280), OpenProfile);
            CreateSpacer(ActionButtonsLayout, Config.ActionSpacing, Config.ActionButtonSize);
            LogoutButton = CreateActionButton(ActionButtonsLayout,
                LoadIcon(40, 280), Logout);
            CreateSpacer(ActionButtonsLayout, Config.ActionSpacing, Config.ActionButtonSize);
            CreateSpacer(ActionsLayout, Config.ActionsWidth, Config.ContentPadding);

            StatsLayout = CreateFlex(ConnectedLayout, new ScalableVector2(Config.Width, Config.StatsHeight), FlexDirection.Row,
                FlexJustifyContent.FlexStart, FlexAlignItems.Center);
            ConnectedLayout.SetItemOptions(StatsLayout, FixedBasis(Config.StatsHeight));
            CreateSpacer(StatsLayout, Config.ContentPadding, Config.StatsHeight);
            RankPill = CreateStatPill(StatsLayout, Config.RankWidth, LoadIcon(0, 0));
            CreateSpacer(StatsLayout, Config.ContentPadding, Config.StatsHeight);
            RatingPill = CreateStatPill(StatsLayout, Config.RatingWidth, LoadIcon(0, 120));
            CreateSpacer(StatsLayout, Config.ContentPadding, Config.StatsHeight);
            AccuracyPill = CreateStatPill(StatsLayout, Config.AccuracyWidth,
                LoadIcon(40, 240));
            var statsSpacer = CreateSpacer(StatsLayout, 1, Config.StatsHeight);
            StatsLayout.SetItemOptions(statsSpacer, new FlexItemOptions { Basis = 1, Grow = 1, Shrink = 1 });

            ModeButton = new RoundedButton((sender, args) => ToggleMode())
            {
                Parent = StatsLayout,
                Size = new ScalableVector2(Config.ModeWidth, Config.ModeHeight),
                CornerRadius = Config.ModeHeight / 2,
                Tint = SkinV2Color.Parse(Config.ModeBackgroundColor),
                UsePreviousSpriteBatchOptions = true
            };
            StatsLayout.SetItemOptions(ModeButton, FixedBasis(Config.ModeWidth));
            ModeSelection = CreateRoundedSprite(Config.ModeSelectionWidth, Config.ModeSelectionHeight,
                Config.ModeSelectionHeight / 2, SkinV2Color.Parse(Config.TextColor));
            ModeSelection.Parent = ModeButton;
            ModeSelection.Alignment = Alignment.MidLeft;
            ModeSelection.X = Config.ModeSelectionInset;
            ModeLabel = new SpriteTextPlus(FontManager.GetWobbleFont(Config.PrimaryFont), "4K",
                Config.ModeFontSize)
            {
                Parent = ModeSelection,
                Alignment = Alignment.MidCenter,
                Tint = SkinV2Color.Parse(Config.ActionPanelColor),
                UsePreviousSpriteBatchOptions = true
            };
            CreateSpacer(StatsLayout, Config.ContentPadding, Config.StatsHeight);

            OfflineLayout = CreateFlex(OfflinePanel, new ScalableVector2(Config.Width, Config.OfflineHeight), FlexDirection.Row,
                FlexJustifyContent.FlexStart, FlexAlignItems.Center);
            CreateSpacer(OfflineLayout, Config.ContentPadding, Config.OfflineHeight);
            OfflineAvatar = CreateAvatar(OfflineLayout, Config.OfflineAvatarSize, OfflineAvatarTexture);
            CreateSpacer(OfflineLayout, Config.OfflineAvatarSpacing, Config.OfflineHeight);
            OfflineInfoLayout = CreateFlex(OfflineLayout, new ScalableVector2(100, Config.OfflineInfoHeight), FlexDirection.Column,
                FlexJustifyContent.Center, FlexAlignItems.FlexStart, Config.InfoGap);
            OfflineLayout.SetItemOptions(OfflineInfoLayout,
                new FlexItemOptions { Basis = 100, Grow = 1, Shrink = 1 });
            OfflineTitle = new SpriteTextPlus(FontManager.GetWobbleFont(Config.PrimaryFont), "Login",
                Config.OfflineTitleFontSize)
            {
                Parent = OfflineInfoLayout,
                Tint = SkinV2Color.Parse(Config.TextColor),
                UsePreviousSpriteBatchOptions = true
            };
            OfflineStatus = new SpriteTextPlus(FontManager.GetWobbleFont(Config.SecondaryFont), string.Empty,
                Config.OfflineStatusFontSize)
            {
                Parent = OfflineInfoLayout,
                Tint = SkinV2Color.Parse(Config.TextColor),
                UsePreviousSpriteBatchOptions = true
            };
            CreateSpacer(OfflineLayout, Config.ContentPadding, Config.OfflineHeight);
            OfflineActionLayout = CreateFlex(OfflineLayout,
                new ScalableVector2(Config.LoginButtonSize, Config.LoginButtonSize), FlexDirection.Row,
                FlexJustifyContent.Center, FlexAlignItems.Center);
            OfflineLayout.SetItemOptions(OfflineActionLayout, FixedBasis(Config.LoginButtonSize));
            LoginButton = new RoundedButton((sender, args) => OnlineManager.Login())
            {
                Parent = OfflineActionLayout,
                Size = new ScalableVector2(Config.LoginButtonSize, Config.LoginButtonSize),
                CornerRadius = Config.CornerRadius,
                Tint = SkinV2Color.Parse(Config.ActionButtonColor),
                UsePreviousSpriteBatchOptions = true
            };
            LoginButton.SetIcon(LoadIcon(0, 320),
                new Vector2(Config.LoginIconSize, Config.LoginIconSize));
            LoginButtonOptions = FixedBasis(Config.LoginButtonSize);
            OfflineActionLayout.SetItemOptions(LoginButton, LoginButtonOptions);
            LoginWheel = new LoadingWheel
            {
                Parent = OfflineActionLayout,
                Size = new ScalableVector2(Config.LoginIconSize, Config.LoginIconSize),
                Visible = false,
                UsePreviousSpriteBatchOptions = true
            };
            LoginWheelOptions = FixedBasis(0);
            OfflineActionLayout.SetItemOptions(LoginWheel, LoginWheelOptions);
            CreateSpacer(OfflineLayout, Config.ContentPadding, Config.OfflineHeight);

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
            if (!IsOpen && CloseAnimationRemaining > 0)
            {
                CloseAnimationRemaining -= (float) gameTime.ElapsedGameTime.TotalMilliseconds;
                if (CloseAnimationRemaining <= 0)
                    Visible = false;
            }

            if (Math.Abs(ScreenDarkness.Width - WindowManager.Width) > 0.001f ||
                Math.Abs(ScreenDarkness.Height - WindowManager.Height) > 0.001f)
                ScreenDarkness.Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);

            SubscribeToCurrentClient();

            if (StateDirty || !ReferenceEquals(User, OnlineManager.Self))
                RefreshState();

            var childHovered = ConnectedPanel.Visible &&
                               (ProfileButton.IsHovered || LogoutButton.IsHovered || ModeButton.IsHovered) ||
                               OfflinePanel.Visible && LoginButton.Visible && LoginButton.IsHovered;
            var targetDepth = childHovered ? 1 : 0;
            if (Button.Depth != targetDepth)
                Button.Depth = targetDepth;

            if (Math.Abs(Button.Width - ContainerSize.X.Value) > 0.001f ||
                Math.Abs(Button.Height - ActiveContentHeight) > 0.001f)
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
            Skin.Dispose();
        }

        public override void Open()
        {
            IsOpen = true;
            CloseAnimationRemaining = 0;
            Visible = true;
            RefreshState();
            ClearAnimations();
            ChangeHeightTo((int) ActiveContentHeight, Easing.OutQuint, 450);

            ScreenDarkness.ClearAnimations();
            ScreenDarkness.FadeTo(Config.DarknessOpacity, Easing.Linear, 200);
        }

        public override void Close()
        {
            IsOpen = false;
            CloseAnimationRemaining = 550;
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
                FlexJustifyContent.FlexStart, FlexAlignItems.Stretch, Config.PanelGap);
            var upperBackgroundLayout = CreateFlex(backgroundLayout,
                new ScalableVector2(Config.Width, Config.UpperHeight),
                FlexDirection.Row, FlexJustifyContent.FlexStart, FlexAlignItems.Stretch);
            backgroundLayout.SetItemOptions(upperBackgroundLayout, FixedBasis(Config.UpperHeight));

            ProfileBackground = new RoundedProfileBackground(Config.Width, Config.UpperHeight,
                Config.CornerRadius, SkinV2Color.Parse(Config.UpperPanelColor), Config.ProfileCoverBrightness)
            {
                Parent = upperBackgroundLayout
            };
            upperBackgroundLayout.SetItemOptions(ProfileBackground, FixedBasis(Config.Width));

            var statsBackground = CreateRoundedSprite(Config.Width, Config.StatsHeight,
                Config.CornerRadius, SkinV2Color.Parse(Config.StatsPanelColor));
            statsBackground.Parent = backgroundLayout;
            backgroundLayout.SetItemOptions(statsBackground, FixedBasis(Config.StatsHeight));
            return panel;
        }

        private Container CreateOfflinePanel()
        {
            var panel = new Container
            {
                Size = new ScalableVector2(Config.Width, Config.OfflineHeight)
            };

            var backgroundLayout = CreateFlex(panel, new ScalableVector2(Config.Width, Config.OfflineHeight), FlexDirection.Row,
                FlexJustifyContent.FlexStart, FlexAlignItems.Stretch);
            var background = CreateRoundedSprite(Config.Width, Config.OfflineHeight, Config.CornerRadius,
                SkinV2Color.Parse(Config.UpperPanelColor));
            background.Parent = backgroundLayout;
            backgroundLayout.SetItemOptions(background, FixedBasis(Config.Width));
            return panel;
        }

        private RoundedButton CreateRolePill(Drawable parent)
        {
            var pill = new RoundedButton
            {
                Parent = parent,
                Size = new ScalableVector2(Config.RoleDefaultWidth, Config.RoleHeight),
                WidthMode = ButtonSizeMode.Auto,
                AutoSizePadding = new Vector2(Config.RolePadding, 0),
                CornerRadius = Config.RoleHeight / 2,
                Tint = SkinV2Color.Parse(Config.RolePillColor),
                IsInteractionEnabled = false,
                UsePreviousSpriteBatchOptions = true
            };
            pill.SetIcon(LoadIcon(40, 360),
                new Vector2(Config.RoleIconSize, Config.RoleIconSize));
            pill.SetLabel(FontManager.GetWobbleFont(Config.PrimaryFont), string.Empty, Config.RoleFontSize,
                SkinV2Color.Parse(Config.TextColor));
            return pill;
        }

        private RoundedButton CreateActionButton(Drawable parent, Texture2D icon, Action action)
        {
            var button = new RoundedButton((sender, args) => action())
            {
                Parent = parent,
                Size = new ScalableVector2(Config.ActionButtonSize, Config.ActionButtonSize),
                CornerRadius = Config.CornerRadius,
                Tint = SkinV2Color.Parse(Config.ActionButtonColor),
                UsePreviousSpriteBatchOptions = true
            };
            button.SetIcon(icon, new Vector2(Config.ActionIconSize, Config.ActionIconSize));
            return button;
        }

        private RoundedButton CreateStatPill(FlexContainer parent, float width, Texture2D icon)
        {
            var pill = new RoundedButton
            {
                Parent = parent,
                Size = new ScalableVector2(width, Config.StatHeight),
                CornerRadius = Config.StatHeight / 2,
                Tint = SkinV2Color.Parse(Config.StatPillColor),
                IsInteractionEnabled = false,
                UsePreviousSpriteBatchOptions = true
            };
            pill.SetIcon(icon, new Vector2(Config.StatIconSize, Config.StatIconSize));
            pill.SetLabel(FontManager.GetWobbleFont(Config.PrimaryFont), "--", Config.StatFontSize,
                SkinV2Color.Parse(Config.TextColor));
            parent.SetItemOptions(pill, FixedBasis(width));
            return pill;
        }

        private RoundedAvatar CreateAvatar(Drawable parent, float size, Texture2D image) =>
            new RoundedAvatar(size, Config.CornerRadius, image)
        {
            Parent = parent
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

        private Texture2D LoadIcon(int x, int y)
        {
            var source = TextureManager.Load("Quaver.Resources/Textures/UI/Screens/Main/Icons2.png");
            var sourceRectangle = new Rectangle(x, y, Config.IconCellSize, Config.IconCellSize);
            var pixels = new Color[Config.IconCellSize * Config.IconCellSize];
            source.GetData(0, sourceRectangle, pixels, 0, pixels.Length);

            var texture = new Texture2D(GameBase.Game.GraphicsDevice, Config.IconCellSize, Config.IconCellSize);
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
            ActiveContentHeight = connected ? Config.ConnectedHeight : Config.OfflineHeight;

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
            RefreshProfileCover();
            Flag.Image = Flags.Get(User.OnlineUser.CountryFlag ?? "XX");
            Clan.UpdateFromUser(User.OnlineUser, SkinV2Color.Parse(Config.TextColor));
            ClanUsernameSpacer.Width = Clan.Visible ? Config.IdentitySpacing + 1 : 0;

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
            ModeSelection.X = is4K ? Config.ModeSelectionInset : -Config.ModeSelectionInset;
            ModeLabel.Text = is4K ? "4K" : "7K";

            RefreshLayouts();
            Username.TruncateWithEllipsis((int) Math.Max(40, UsernameHost.Width));
            Status.TruncateWithEllipsis((int) StatusHost.Width);
        }

        private void RefreshOfflineState()
        {
            RequestedProfileCoverUserId = null;
            ProfileBackground.ClearSource();
            OfflineAvatar.SetSource(OfflineAvatarTexture);
            OfflineStatus.Text = GetConnectionStatusText();
            OfflineStatus.TruncateWithEllipsis(385);

            var canLogin = OnlineManager.Status.Value == ConnectionStatus.Disconnected;
            LoginButton.Visible = canLogin;
            LoginButton.IsClickable = canLogin;
            LoginWheel.Visible = !canLogin;
            LoginButtonOptions.Basis = canLogin ? Config.LoginButtonSize : 0;
            LoginWheelOptions.Basis = canLogin ? 0 : Config.LoginIconSize;

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

        private void RefreshProfileCover()
        {
            var onlineUser = User?.OnlineUser;
            if (onlineUser == null || !onlineUser.UserGroups.HasFlag(UserGroups.Donator))
            {
                RequestedProfileCoverUserId = null;
                ProfileBackground.ClearSource();
                return;
            }

            var userId = onlineUser.Id;
            if (RequestedProfileCoverUserId == userId)
                return;

            RequestedProfileCoverUserId = userId;
            ProfileBackground.ClearSource();

            ImageDownloader.DownloadProfileCover(userId).ContinueWith(task =>
            {
                var cover = task.Status == System.Threading.Tasks.TaskStatus.RanToCompletion
                    ? task.Result
                    : null;

                AddScheduledUpdate(() =>
                {
                    if (IsDisposed || RequestedProfileCoverUserId != userId ||
                        User?.OnlineUser?.Id != userId)
                        return;

                    if (cover != null)
                        ProfileBackground.SetSource(cover);
                });
            });
        }

        private Texture2D GetAvatar()
        {
            var image = UnknownAvatarTexture;
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
        ///     Draws a texture through the shared rounded-rectangle shader without breaking an ancestor
        ///     ScrollContainer's scissor state.
        /// </summary>
        private abstract class ShaderRoundedSprite : Sprite
        {
            private SpriteBatchOptions PlainScissorOptions { get; } =
                RoundedRectShader.CreateScissorSafeOptions();

            protected ShaderRoundedSprite(float width, float height, float cornerRadius)
            {
                Size = new ScalableVector2(width, height);
                var shader = RoundedRectShader.Create(cornerRadius);
                RoundedRectShader.UpdateSize(shader, new Vector2(width, height));
                SpriteBatchOptions = new SpriteBatchOptions
                {
                    RasterizerState = RoundedRectShader.ScissorSafeRasterizerState,
                    Shader = shader
                };
            }

            public override void Draw(GameTime gameTime)
            {
                base.Draw(gameTime);

                // The following labels and icons share their parent's batch, so restore a plain,
                // scissor-safe batch instead of allowing the rounded shader to leak into them.
                PlainScissorOptions.Begin();
            }
        }

        /// <summary>
        ///     Applies the shared rounded-rectangle shader directly to the avatar texture.
        /// </summary>
        private sealed class RoundedAvatar : ShaderRoundedSprite
        {
            public RoundedAvatar(float size, float cornerRadius, Texture2D image)
                : base(size, size, cornerRadius) => SetSource(image);

            public void SetSource(Texture2D source)
            {
                if (source != null && !source.IsDisposed)
                    Image = source;
            }
        }

        /// <summary>
        ///     Applies the shared rounded-rectangle shader to the full-width profile cover.
        /// </summary>
        private sealed class RoundedProfileBackground : ShaderRoundedSprite
        {
            private Color FallbackColor { get; }

            private Color CoverTint { get; }

            private Texture2D CroppedTexture { get; set; }

            public RoundedProfileBackground(float width, float height, float cornerRadius, Color fallbackColor,
                float coverBrightness)
                : base(width, height, cornerRadius)
            {
                FallbackColor = fallbackColor;
                CoverTint = new Color(coverBrightness, coverBrightness, coverBrightness, 1);
                ClearSource();
            }

            public void SetSource(Texture2D source)
            {
                if (source == null || source.IsDisposed)
                    return;

                CroppedTexture?.Dispose();
                CroppedTexture = CreateCoverTexture(source, Math.Max(1, (int) Math.Ceiling(Width)),
                    Math.Max(1, (int) Math.Ceiling(Height)));
                Image = CroppedTexture ?? source;
                Tint = CoverTint;
            }

            public void ClearSource()
            {
                CroppedTexture?.Dispose();
                CroppedTexture = null;
                Image = UserInterface.BlankBox;
                Tint = FallbackColor;
            }

            public override void Destroy()
            {
                CroppedTexture?.Dispose();
                CroppedTexture = null;
                base.Destroy();
            }

            private static Texture2D CreateCoverTexture(Texture2D source, int width, int height)
            {
                try
                {
                    var sourcePixels = new Color[source.Width * source.Height];
                    source.GetData(sourcePixels);
                    var pixels = new Color[width * height];
                    var targetAspect = width / (float) height;
                    var sourceAspect = source.Width / (float) source.Height;
                    var cropWidth = sourceAspect > targetAspect
                        ? source.Height * targetAspect
                        : source.Width;
                    var cropHeight = sourceAspect > targetAspect
                        ? source.Height
                        : source.Width / targetAspect;
                    var cropX = (source.Width - cropWidth) / 2f;
                    var cropY = (source.Height - cropHeight) / 2f;

                    for (var y = 0; y < height; y++)
                    {
                        var sourceY = Math.Min(source.Height - 1,
                            (int) (cropY + y * cropHeight / height));

                        for (var x = 0; x < width; x++)
                        {
                            var sourceX = Math.Min(source.Width - 1,
                                (int) (cropX + x * cropWidth / width));
                            pixels[y * width + x] = sourcePixels[sourceY * source.Width + sourceX];
                        }
                    }

                    var texture = new Texture2D(GameBase.Game.GraphicsDevice, width, height);
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
