using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Overlays.V2Hub.Users;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Online.API.User;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Tooltips;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Profile
{
    internal sealed class ProfileViewerPanel : Container
    {
        private User User { get; }

        private SkinStoreV2Lease Skin { get; }

        private ProfileViewerStyle Config { get; }

        private Action CloseDialog { get; }

        private Container OverlayHost { get; }

        private FlexContainer RootLayout { get; }

        private Container IdentityPanel { get; }

        private Container StatsPanel { get; }

        private Sprite StatsBackground { get; }

        private FlexContainer IdentityLayout { get; }

        private FlexContainer InfoLayout { get; }

        private FlexContainer NameLayout { get; }

        private FlexContainer ActionsLayout { get; }

        private FlexContainer StatsLayout { get; }

        private RoundedCoverSprite Cover { get; }

        private RoundedTextureSprite Avatar { get; }

        private Sprite Flag { get; }

        private ClanTag Clan { get; }

        private Container ClanUsernameSpacer { get; }

        private Container UsernameHost { get; }

        private Container StatusHost { get; }

        private SpriteTextPlus Username { get; }

        private MarqueeSpriteText Status { get; }

        private Sprite RoleBadge { get; }

        private RoundedButton FriendButton { get; }

        private RoundedButton ChatButton { get; }

        private RoundedButton SteamButton { get; }

        private RoundedButton SpectateButton { get; }

        private RoundedButton MoreButton { get; }

        private ProfileStatPill RankPill { get; }

        private ProfileStatPill RatingPill { get; }

        private ProfileStatPill AccuracyPill { get; }

        private RoundedButton ModeButton { get; }

        private Sprite ModeSelection { get; }

        private SpriteTextPlus ModeLabel { get; }

        private UserRightClickOptions MoreMenu { get; set; }

        private List<IDisposable> TooltipRegistrations { get; } = new List<IDisposable>();

        private TooltipOptions FriendTooltip { get; }

        private TooltipOptions ChatTooltip { get; }

        private TooltipOptions SteamTooltip { get; }

        private TooltipOptions SpectateTooltip { get; }

        private APIResponseUsersFull LoadedStats { get; set; }

        private ProfileAccentPalette AccentPalette { get; set; }

        private GameMode ActiveMode { get; set; } = GameMode.Keys4;

        private bool? LastFriendState { get; set; }

        private bool? LastSpectatingState { get; set; }

        private bool? LastSelfActionState { get; set; }

        private string LastStatusText { get; set; }

        private bool IsSelf => User.OnlineUser.Id == OnlineManager.Self?.OnlineUser?.Id;

        private bool IsOnline => OnlineManager.OnlineUsers?.ContainsKey(User.OnlineUser.Id) == true || IsSelf && OnlineManager.Connected;

        public ProfileViewerPanel(User user, ProfileViewerData data, SkinStoreV2Lease skin, Action closeDialog, Container overlayHost)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            LoadedStats = data?.Response;
            AccentPalette = data?.Accent;
            Skin = skin ?? throw new ArgumentNullException(nameof(skin));
            CloseDialog = closeDialog ?? throw new ArgumentNullException(nameof(closeDialog));
            OverlayHost = overlayHost ?? throw new ArgumentNullException(nameof(overlayHost));
            Config = ProfileViewerStyle.Default;
            Size = new ScalableVector2(Config.Width, Config.Height);

            if (ConfigManager.SelectedGameMode != null)
                ActiveMode = ConfigManager.SelectedGameMode.Value;

            RootLayout = CreateFlex(this, Size, FlexDirection.Column, FlexJustifyContent.FlexStart, FlexAlignItems.Stretch, Config.PanelGap);

            IdentityPanel = new Container
            {
                Parent = RootLayout,
                Size = new ScalableVector2(Config.Width, Config.IdentityHeight)
            };
            RootLayout.SetItemOptions(IdentityPanel, FixedBasis(Config.IdentityHeight));

            StatsPanel = new Container
            {
                Parent = RootLayout,
                Size = new ScalableVector2(Config.Width, Config.StatsHeight)
            };
            RootLayout.SetItemOptions(StatsPanel, FixedBasis(Config.StatsHeight));

            Cover = new RoundedCoverSprite(Config.Width, Config.IdentityHeight, Config.CornerRadius,
                SkinV2Color.Parse(Config.IdentityBackgroundColor), Config.ProfileCoverBrightness,
                UserInterface.DefaultBanner)
            {
                Parent = IdentityPanel
            };
            if (data?.Cover != null)
                Cover.SetSource(data.Cover);

            StatsBackground = CreateRoundedSprite(Config.Width, Config.StatsHeight, Config.CornerRadius, SkinV2Color.Parse(Config.StatsBackgroundColor));
            StatsBackground.Parent = StatsPanel;

            IdentityLayout = CreateFlex(IdentityPanel, IdentityPanel.Size, FlexDirection.Row, FlexJustifyContent.FlexStart, FlexAlignItems.Center);
            CreateSpacer(IdentityLayout, Config.ContentPadding, Config.IdentityHeight);

            Avatar = new RoundedTextureSprite(Config.AvatarSize, Config.AvatarSize, Config.CornerRadius,
                GetAvatar())
            {
                Parent = IdentityLayout
            };
            IdentityLayout.SetItemOptions(Avatar, FixedBasis(Config.AvatarSize));

            CreateSpacer(IdentityLayout, Config.ContentPadding, Config.IdentityHeight);

            InfoLayout = CreateFlex(IdentityLayout, new ScalableVector2(1, Config.AvatarSize), FlexDirection.Column, FlexJustifyContent.FlexStart, FlexAlignItems.Stretch, Config.InfoGap);
            IdentityLayout.SetItemOptions(InfoLayout, new FlexItemOptions { Basis = 1, Grow = 1, Shrink = 1 });

            NameLayout = CreateFlex(InfoLayout, new ScalableVector2(1, Config.IdentityRowHeight), FlexDirection.Row, FlexJustifyContent.FlexStart, FlexAlignItems.Center);
            InfoLayout.SetItemOptions(NameLayout, FixedBasis(Config.IdentityRowHeight));

            Flag = new Sprite
            {
                Parent = NameLayout,
                Size = new ScalableVector2(Config.FlagSize, Config.FlagSize),
                Region = Flags.GetRegion(User.OnlineUser.CountryFlag ?? "XX"),
                UsePreviousSpriteBatchOptions = true
            };
            CreateSpacer(NameLayout, Config.IdentitySpacing, Config.IdentityRowHeight);

            Clan = new ClanTag(Config.UsernameFontSize)
            {
                Parent = NameLayout,
                UsePreviousSpriteBatchOptions = true
            };
            Clan.UpdateFromUser(User.OnlineUser, SkinV2Color.Parse(Config.TextColor));
            ClanUsernameSpacer = CreateSpacer(NameLayout, Clan.Visible ? Config.IdentitySpacing : 0, Config.IdentityRowHeight);

            UsernameHost = new Container
            {
                Parent = NameLayout,
                Size = new ScalableVector2(40, Config.IdentityRowHeight)
            };
            NameLayout.SetItemOptions(UsernameHost, new FlexItemOptions { Basis = 40, Grow = 1, Shrink = 1 });
            Username = new SpriteTextPlus(FontManager.GetWobbleFont(Config.PrimaryFont), User.OnlineUser.Username ?? $"#{User.OnlineUser.Id}", Config.UsernameFontSize)
            {
                Parent = UsernameHost,
                Alignment = Alignment.MidLeft,
                Tint = Color.White,
                UsePreviousSpriteBatchOptions = true
            };

            StatusHost = new Container
            {
                Parent = InfoLayout,
                Size = new ScalableVector2(1, Config.StatusHeight)
            };
            InfoLayout.SetItemOptions(StatusHost, FixedBasis(Config.StatusHeight));
            Status = new MarqueeSpriteText(FontManager.GetWobbleFont(Config.SecondaryFont), string.Empty, Config.StatusFontSize, 1)
            {
                Parent = StatusHost,
                Size = new ScalableVector2(1, Config.StatusHeight),
                IsActive = true,
                UsePreviousSpriteBatchOptions = true
            };
            Status.TextSprite.Tint = SkinV2Color.Parse(Config.SecondaryTextColor);

            RoleBadge = new Sprite
            {
                Parent = InfoLayout,
                Size = new ScalableVector2(1, Config.RoleHeight),
                Visible = false,
                UsePreviousSpriteBatchOptions = true
            };
            InfoLayout.SetItemOptions(RoleBadge, new FlexItemOptions
            {
                Basis = Config.RoleHeight,
                AlignSelf = FlexAlignSelf.FlexStart
            });

            ActionsLayout = CreateFlex(IdentityPanel, new ScalableVector2(Config.ActionsWidth, Config.AvatarSize), FlexDirection.Row, FlexJustifyContent.FlexEnd, FlexAlignItems.FlexEnd, Config.ActionSpacing);
            ActionsLayout.Alignment = Alignment.MidRight;
            ActionsLayout.X = -Config.ContentPadding;

            FriendButton = CreateActionButton(GlobalIcon.HeartWhite, PerformPrimaryAction, 0);
            ChatButton = CreateActionButton(GlobalIcon.Chat, PerformSecondaryAction, 1);
            SteamButton = CreateActionButton(GlobalIcon.Steam, OpenSteamProfile, 2);
            SpectateButton = CreateActionButton(GlobalIcon.ViewProfile, PerformTertiaryAction, 3);
            MoreButton = CreateActionButton(GlobalIcon.Options, ToggleMoreMenu, 4);

            CreateSpacer(IdentityLayout, Config.ContentPadding, Config.IdentityHeight);

            StatsLayout = CreateFlex(StatsPanel, StatsPanel.Size, FlexDirection.Row, FlexJustifyContent.FlexStart, FlexAlignItems.Center);
            CreateSpacer(StatsLayout, Config.ContentPadding, Config.StatsHeight);
            RankPill = CreateStatPill(CapsuleIcon.GlobalRank);
            CreateSpacer(StatsLayout, Config.ContentPadding, Config.StatsHeight);
            RatingPill = CreateStatPill(CapsuleIcon.LineChart);
            CreateSpacer(StatsLayout, Config.ContentPadding, Config.StatsHeight);
            AccuracyPill = CreateStatPill(CapsuleIcon.Accuracy);

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
            ModeSelection = CreateRoundedSprite(Config.ModeSelectionWidth, Config.ModeSelectionHeight, Config.ModeSelectionHeight / 2, SkinV2Color.Parse(Config.ModeSelectionColor));
            ModeSelection.Parent = ModeButton;
            ModeSelection.Alignment = Alignment.MidLeft;
            ModeSelection.X = Config.ModeSelectionInset;
            ModeLabel = new SpriteTextPlus(FontManager.GetWobbleFont(Config.PrimaryFont), "4K", Config.ModeFontSize)
            {
                Parent = ModeSelection,
                Alignment = Alignment.MidCenter,
                Tint = SkinV2Color.Parse(Config.ModeBackgroundColor),
                UsePreviousSpriteBatchOptions = true
            };
            CreateSpacer(StatsLayout, Config.ContentPadding, Config.StatsHeight);

            FriendTooltip = new TooltipOptions(LocalizationManager.Get("Screen_Hub_AddFriend"));
            ChatTooltip = new TooltipOptions(LocalizationManager.Get("Screen_Main_Menu_Chat"));
            SteamTooltip = new TooltipOptions(LocalizationManager.Get("Screen_Multiplayer_SteamProfile"));
            SpectateTooltip = new TooltipOptions(LocalizationManager.Get("Screen_Hub_Spectate"));

            TooltipRegistrations.Add(FriendButton.AddTooltip(FriendTooltip));
            TooltipRegistrations.Add(ChatButton.AddTooltip(ChatTooltip));
            TooltipRegistrations.Add(SteamButton.AddTooltip(SteamTooltip));
            TooltipRegistrations.Add(SpectateButton.AddTooltip(SpectateTooltip));
            TooltipRegistrations.Add(MoreButton.AddTooltip(LocalizationManager.Get("Screen_Selection_Options")));
            TooltipRegistrations.Add(RankPill.AddTooltip(LocalizationManager.Get("Screen_Selection_GlobalRank")));
            TooltipRegistrations.Add(RatingPill.AddTooltip(LocalizationManager.Get("Screen_Selection_OverallRating")));
            TooltipRegistrations.Add(AccuracyPill.AddTooltip(LocalizationManager.Get("Screen_Selection_OverallAccuracy")));

            SteamManager.SteamUserAvatarLoaded += OnSteamUserAvatarLoaded;

            RefreshIdentity();
            RefreshLayouts();
            RefreshResponsiveText();
            ApplyAccentPalette();
            UpdateActionStates();
            UpdateStats();
            RequestAvatar();
        }

        public override void Update(GameTime gameTime)
        {
            var statusText = GetStatusText();
            if (LastStatusText != statusText)
            {
                LastStatusText = statusText;
                Status.TextSprite.Text = statusText;
                Status.ResetPosition();
            }

            UpdateActionStates();
            base.Update(gameTime);
        }

        public bool IsInteractionHovered() => IsHovered() || MoreMenu?.IsHovered() == true;

        public override void Destroy()
        {
            SteamManager.SteamUserAvatarLoaded -= OnSteamUserAvatarLoaded;
            DestroyMoreMenu();

            foreach (var registration in TooltipRegistrations)
                registration.Dispose();
            TooltipRegistrations.Clear();

            base.Destroy();
        }

        private void RefreshIdentity()
        {
            Flag.Region = Flags.GetRegion(User.OnlineUser.CountryFlag ?? "XX");
            Clan.UpdateFromUser(User.OnlineUser, SkinV2Color.Parse(Config.TextColor));
            ClanUsernameSpacer.Width = Clan.Visible ? Config.IdentitySpacing : 0;
            Username.Text = User.OnlineUser.Username ?? $"#{User.OnlineUser.Id}";
            LastStatusText = GetStatusText();
            Status.TextSprite.Text = LastStatusText;
            Status.ResetPosition();

            var role = GetRoleGroup(User.OnlineUser.UserGroups);
            RoleBadge.Visible = role.HasValue;
            if (role.HasValue)
            {
                var displaySize = UserGroupAssets.GetDisplaySize(role.Value, true);
                var scale = Config.RoleHeight / displaySize.Y;
                RoleBadge.Region = UserGroupAssets.Get(role.Value, true);
                RoleBadge.Size = new ScalableVector2(displaySize.X * scale, Config.RoleHeight);
                RoleBadge.Tint = Color.White;
            }
            else
            {
                RoleBadge.Parent = IdentityPanel;
            }
        }

        private void RefreshLayouts()
        {
            RootLayout.RefreshLayout();
            IdentityLayout.RefreshLayout();
            InfoLayout.RefreshLayout();
            NameLayout.RefreshLayout();
            ActionsLayout.RefreshLayout();
            StatsLayout.RefreshLayout();
        }

        private void RefreshResponsiveText()
        {
            Username.TruncateWithEllipsis((int) Math.Max(40, UsernameHost.Width));
            Status.Width = Math.Max(40, StatusHost.Width);
            Status.Height = Config.StatusHeight;
            Status.ResetPosition();
        }

        private RoundedButton CreateActionButton(GlobalIcon icon, Action action, int order)
        {
            var button = new RoundedButton((sender, args) => action())
            {
                Parent = ActionsLayout,
                Size = new ScalableVector2(Config.ActionButtonSize, Config.ActionButtonSize),
                CornerRadius = Config.CornerRadius,
                Tint = SkinV2Color.Parse(Config.ActionButtonColor),
                PerformHoverFade = true,
                UsePreviousSpriteBatchOptions = true
            };
            button.SetIcon(GlobalIcons.Get(icon), new Vector2(Config.ActionIconSize, Config.ActionIconSize));
            var options = FixedBasis(Config.ActionButtonSize);
            options.Order = order;
            ActionsLayout.SetItemOptions(button, options);
            return button;
        }

        private ProfileStatPill CreateStatPill(CapsuleIcon icon)
        {
            var pill = new ProfileStatPill(Config.StatContentGap, Config.StatHorizontalPadding)
            {
                Parent = StatsLayout,
                Size = new ScalableVector2(1, Config.StatHeight),
                CornerRadius = Config.StatHeight / 2,
                Tint = SkinV2Color.Parse(Config.StatPillColor),
                IsInteractionEnabled = false,
                IsClickable = false,
                PerformHoverFade = false,
                UsePreviousSpriteBatchOptions = true
            };
            pill.SetIcon(CapsuleIcons.Get(icon), CapsuleIcons.GetDisplaySize(icon, Config.StatIconHeight));
            pill.SetLabel(FontManager.GetWobbleFont(Config.PrimaryFont), "--", Config.StatFontSize, SkinV2Color.Parse(Config.TextColor));
            var width = pill.ResizeToContent();
            StatsLayout.SetItemOptions(pill, FixedBasis(width));
            return pill;
        }

        private void RefreshStatPillLayout()
        {
            var rankWidth = RankPill.ResizeToContent();
            var ratingWidth = RatingPill.ResizeToContent();
            var accuracyWidth = AccuracyPill.ResizeToContent();

            StatsLayout.SetItemOptions(RankPill, FixedBasis(rankWidth));
            StatsLayout.SetItemOptions(RatingPill, FixedBasis(ratingWidth));
            StatsLayout.SetItemOptions(AccuracyPill, FixedBasis(accuracyWidth));
            StatsLayout.RefreshLayout();
        }

        private void UpdateActionStates()
        {
            if (LastSelfActionState != IsSelf)
            {
                LastSelfActionState = IsSelf;
                ConfigureActionButtons();
            }

            UpdateActionButtonVisibility(ChatButton, !IsSelf, 1);
            UpdateActionButtonVisibility(SpectateButton, IsSelf || IsOnline, 3);

            if (IsSelf)
            {
                SetActionEnabled(FriendButton, User.HasUserInfo, Config.ActionButtonColor);
                SetActionEnabled(ChatButton, false, Config.ActionButtonColor);
                SetActionEnabled(SteamButton, User.HasUserInfo && User.OnlineUser.SteamId > 0, Config.ActionButtonColor);
                SetActionEnabled(SpectateButton, User.HasUserInfo, Config.ActionButtonColor);
                ApplyClickableAccent(MoreButton);
                return;
            }

            var isFriend = OnlineManager.FriendsList?.Contains(User.OnlineUser.Id) == true;
            if (LastFriendState != isFriend)
            {
                LastFriendState = isFriend;
                FriendTooltip.Text = LocalizationManager.Get(isFriend ? "Screen_Hub_RemoveFriend" : "Screen_Hub_AddFriend");
            }

            var spectating = OnlineManager.SpectatorClients?.ContainsKey(User.OnlineUser.Id) == true;
            if (LastSpectatingState != spectating)
            {
                LastSpectatingState = spectating;
                SpectateTooltip.Text = LocalizationManager.Get(spectating ? "Screen_Hub_StopSpectating" : "Screen_Hub_Spectate");
            }

            SetActionEnabled(FriendButton, !IsSelf && User.HasUserInfo, isFriend ? Config.FriendedActionButtonColor : Config.ActionButtonColor, !isFriend);
            SetActionEnabled(ChatButton, !IsSelf && User.HasUserInfo, Config.ActionButtonColor);
            SetActionEnabled(SteamButton, User.HasUserInfo && User.OnlineUser.SteamId > 0, Config.ActionButtonColor);
            SetActionEnabled(SpectateButton, IsOnline && User.HasUserInfo && OnlineManager.SpectatorClients != null, spectating ? Config.SelectedActionButtonColor : Config.ActionButtonColor);
            ApplyClickableAccent(MoreButton);
        }

        private void UpdateActionButtonVisibility(RoundedButton button, bool visible, int order)
        {
            if ((button.Parent == ActionsLayout) == visible)
                return;

            button.Visible = visible;

            if (!visible)
            {
                button.IsClickable = false;
                button.Parent = IdentityPanel;
            }
            else
            {
                button.Parent = ActionsLayout;
                var options = FixedBasis(Config.ActionButtonSize);
                options.Order = order;
                ActionsLayout.SetItemOptions(button, options);
            }

            ActionsLayout.RefreshLayout();
        }

        private void ConfigureActionButtons()
        {
            if (IsSelf)
            {
                FriendButton.SetIcon(GlobalIcons.Get(GlobalIcon.ViewProfile), new Vector2(Config.ActionIconSize, Config.ActionIconSize));
                SteamButton.SetIcon(GlobalIcons.Get(GlobalIcon.Steam), new Vector2(Config.ActionIconSize, Config.ActionIconSize));
                SpectateButton.SetIcon(GlobalIcons.Get(GlobalIcon.LogOut), new Vector2(Config.ActionIconSize, Config.ActionIconSize));

                FriendTooltip.Text = LocalizationManager.Get("Screen_Multiplayer_ViewProfile");
                SteamTooltip.Text = LocalizationManager.Get("Screen_Multiplayer_SteamProfile");
                SpectateTooltip.Text = LocalizationManager.Get("ProfileViewer_Action_Logout");
                return;
            }

            FriendButton.SetIcon(GlobalIcons.Get(GlobalIcon.HeartWhite), new Vector2(Config.ActionIconSize, Config.ActionIconSize));
            ChatButton.SetIcon(GlobalIcons.Get(GlobalIcon.Chat), new Vector2(Config.ActionIconSize, Config.ActionIconSize));
            SteamButton.SetIcon(GlobalIcons.Get(GlobalIcon.Steam), new Vector2(Config.ActionIconSize, Config.ActionIconSize));
            SpectateButton.SetIcon(GlobalIcons.Get(GlobalIcon.ViewProfile), new Vector2(Config.ActionIconSize, Config.ActionIconSize));
            ChatTooltip.Text = LocalizationManager.Get("Screen_Main_Menu_Chat");
            SteamTooltip.Text = LocalizationManager.Get("Screen_Multiplayer_SteamProfile");
        }

        private void SetActionEnabled(RoundedButton button, bool enabled, string enabledColor, bool applyAccent = true)
        {
            button.IsClickable = enabled;

            if (!enabled)
            {
                button.Alpha = 1;
                button.Tint = SkinV2Color.Parse(Config.DisabledActionButtonColor);
                button.Icon.Tint = SkinV2Color.Parse(Config.TextColor);
                return;
            }

            if (AccentPalette != null && applyAccent)
            {
                ApplyClickableAccent(button);
                return;
            }

            button.Tint = SkinV2Color.Parse(enabledColor);
            button.Icon.Tint = SkinV2Color.Parse(Config.TextColor);
        }

        private void ApplyClickableAccent(RoundedButton button)
        {
            if (AccentPalette == null)
                return;

            var accentSurface = Color.Lerp(SkinV2Color.Parse(Config.StatsBackgroundColor), AccentPalette.Accent, Config.AccentSurfaceOpacity);
            var buttonColor = Color.Lerp(accentSurface, Color.White, Config.AccentButtonLightenAmount);
            var contentColor = ProfileAccentColors.GetContentColor(buttonColor);

            button.Tint = buttonColor;
            button.Icon.Tint = contentColor;
            if (button.Label != null)
                button.Label.Tint = contentColor;
        }

        private void PerformPrimaryAction()
        {
            if (IsSelf)
            {
                UserRightClickOptions.PerformAction(User, UserMenuAction.ViewProfile);
                return;
            }

            ToggleFriend();
        }

        private void PerformSecondaryAction() => OpenChat();

        private void PerformTertiaryAction()
        {
            if (IsSelf)
            {
                CloseDialog();
                ThreadScheduler.Run(() => OnlineManager.Client?.Disconnect());
                return;
            }

            ToggleSpectate();
        }

        private void ToggleFriend()
        {
            var action = OnlineManager.FriendsList?.Contains(User.OnlineUser.Id) == true ? UserMenuAction.RemoveFriend : UserMenuAction.AddFriend;
            UserRightClickOptions.PerformAction(User, action);
            LastFriendState = null;
        }

        private void OpenChat()
        {
            if (UserRightClickOptions.PerformAction(User, UserMenuAction.Chat))
                CloseDialog();
        }

        private void OpenSteamProfile() => UserRightClickOptions.PerformAction(User, UserMenuAction.SteamProfile);

        private void ToggleSpectate()
        {
            var spectating = OnlineManager.SpectatorClients?.ContainsKey(User.OnlineUser.Id) == true;
            UserRightClickOptions.PerformAction(User, spectating ? UserMenuAction.StopSpectating : UserMenuAction.Spectate);
            LastSpectatingState = null;
        }

        private void ToggleMoreMenu()
        {
            if (MoreMenu != null)
            {
                DestroyMoreMenu();
                return;
            }

            const float menuWidth = 220;
            MoreMenu = new UserRightClickOptions(User, menuWidth, Skin.Config.Shared.Dropdown, OverlayHost,
                GetDedicatedActions())
            {
                Parent = OverlayHost,
                Position = new ScalableVector2(
                    MoreButton.ScreenRectangle.Right - OverlayHost.ScreenRectangle.Left - menuWidth,
                    MoreButton.ScreenRectangle.Bottom - OverlayHost.ScreenRectangle.Top + Config.PanelGap)
            };
            MoreMenu.OptionSelected += OnMoreOptionSelected;
            MoreMenu.Open();
        }

        private HashSet<UserMenuAction> GetDedicatedActions()
        {
            var actions = new HashSet<UserMenuAction>
            {
                UserMenuAction.SteamProfile
            };

            if (IsSelf)
            {
                actions.Add(UserMenuAction.ViewProfile);
                return actions;
            }

            actions.Add(UserMenuAction.AddFriend);
            actions.Add(UserMenuAction.RemoveFriend);
            actions.Add(UserMenuAction.Chat);
            actions.Add(UserMenuAction.Spectate);
            actions.Add(UserMenuAction.StopSpectating);
            return actions;
        }

        private void OnMoreOptionSelected(object sender, DropdownOptionEventArgs<UserMenuAction> args) => CloseDialog();

        private void DestroyMoreMenu()
        {
            if (MoreMenu != null)
                MoreMenu.OptionSelected -= OnMoreOptionSelected;
            MoreMenu?.Destroy();
            MoreMenu = null;
        }

        private void ToggleMode()
        {
            ActiveMode = ActiveMode == GameMode.Keys4 ? GameMode.Keys7 : GameMode.Keys4;
            UpdateStats();
        }

        private void UpdateStats()
        {
            var is4K = ActiveMode == GameMode.Keys4;
            ModeSelection.Alignment = is4K ? Alignment.MidLeft : Alignment.MidRight;
            ModeSelection.X = is4K ? Config.ModeSelectionInset : -Config.ModeSelectionInset;
            ModeLabel.Text = is4K ? "4K" : "7K";

            var apiStats = is4K ? LoadedStats?.User?.StatsKeys4 : LoadedStats?.User?.StatsKeys7;
            if (apiStats != null)
            {
                SetStats(apiStats.Ranks?.Global ?? -1, apiStats.OverallPerformanceRating, apiStats.OverallAccuracy);
                return;
            }

            if (User.Stats.TryGetValue(ActiveMode, out var userStats))
            {
                SetStats(userStats.Rank, userStats.OverallPerformanceRating, userStats.OverallAccuracy);
                return;
            }

            SetStats(-1, null, null);
        }

        private void SetStats(int rank, double? rating, double? accuracy)
        {
            RankPill.Label.Text = rank > 0 ? $"#{rank:n0}" : "--";
            RatingPill.Label.Text = rating.HasValue ? StringHelper.RatingToString(rating.Value) : "--";
            AccuracyPill.Label.Text = accuracy.HasValue ? StringHelper.AccuracyToString((float) accuracy.Value) : "--";
            RefreshStatPillLayout();
        }

        private void ApplyAccentPalette()
        {
            if (AccentPalette == null)
                return;

            StatsBackground.Tint = Color.Lerp(SkinV2Color.Parse(Config.StatsBackgroundColor), AccentPalette.Accent, Config.AccentSurfaceOpacity);
            RankPill.Tint = AccentPalette.Accent;
            RatingPill.Tint = AccentPalette.Accent;
            AccuracyPill.Tint = AccentPalette.Accent;

            RankPill.Icon.Tint = AccentPalette.Content;
            RankPill.Label.Tint = AccentPalette.Content;
            RatingPill.Icon.Tint = AccentPalette.Content;
            RatingPill.Label.Tint = AccentPalette.Content;
            AccuracyPill.Icon.Tint = AccentPalette.Content;
            AccuracyPill.Label.Tint = AccentPalette.Content;
            ApplyClickableAccent(MoreButton);
        }

        private void RequestAvatar()
        {
            var steamId = (ulong) Math.Max(0, User.OnlineUser.SteamId);
            if (steamId > 0 && TryGetAvatar(steamId) == null)
                SteamManager.SendAvatarRetrievalRequest(steamId);
        }

        private Texture2D GetAvatar()
        {
            if (User.OnlineUser.UserGroups.HasFlag(UserGroups.Bot))
                return UserInterface.QuaverIcon;

            var steamId = (ulong) Math.Max(0, User.OnlineUser.SteamId);
            return steamId > 0 ? TryGetAvatar(steamId) ?? UserInterface.UnknownAvatar : UserInterface.UnknownAvatar;
        }

        private static Texture2D TryGetAvatar(ulong steamId)
        {
            if (SteamManager.UserAvatarsLarge != null && SteamManager.UserAvatarsLarge.TryGetValue(steamId, out var avatar) && avatar != UserInterface.UnknownAvatar)
                return avatar;

            if (SteamManager.UserAvatars != null && SteamManager.UserAvatars.TryGetValue(steamId, out avatar) && avatar != UserInterface.UnknownAvatar)
                return avatar;

            return null;
        }

        private void OnSteamUserAvatarLoaded(object sender, SteamAvatarLoadedEventArgs args)
        {
            if ((ulong) Math.Max(0, User.OnlineUser.SteamId) != args.SteamId)
                return;

            AddScheduledUpdate(() =>
            {
                if (!IsDisposed)
                    Avatar.SetSource(GetAvatar());
            });
        }

        private string GetStatusText()
        {
            if (!IsOnline)
                return LocalizationManager.Get("ProfileViewer_Status_Offline");

            var status = User.CurrentStatus;
            switch (status?.Status)
            {
                case ClientStatus.Selecting:
                    return FormatActivityStatus("ProfileViewer_Status_Selecting", status.Content);
                case ClientStatus.Playing:
                case ClientStatus.Paused:
                    return FormatActivityStatus("ProfileViewer_Status_Playing", status.Content);
                case ClientStatus.Watching:
                    return FormatActivityStatus("ProfileViewer_Status_WatchingReplay", status.Content);
                case ClientStatus.Editing:
                    return FormatActivityStatus("ProfileViewer_Status_Editing", status.Content);
                case ClientStatus.InLobby:
                    return FormatActivityStatus("ProfileViewer_Status_MultiplayerLobby", status.Content);
                case ClientStatus.Multiplayer:
                    return FormatActivityStatus("ProfileViewer_Status_PlayingMultiplayer", status.Content);
                case ClientStatus.Listening:
                    return FormatActivityStatus("ProfileViewer_Status_Listening", status.Content);
                default:
                    return LocalizationManager.Get("ProfileViewer_Status_Idle");
            }
        }

        private static string FormatActivityStatus(string localizationKey, string activity)
        {
            var status = LocalizationManager.Get(localizationKey);
            return string.IsNullOrWhiteSpace(activity) ? status : $"{status} · {activity}";
        }

        private static GlobalUserGroup? GetRoleGroup(UserGroups groups)
        {
            if (groups.HasFlag(UserGroups.Swan))
                return GlobalUserGroup.Swan;
            if (groups.HasFlag(UserGroups.Developer))
                return GlobalUserGroup.Developer;
            if (groups.HasFlag(UserGroups.GraphicDesigner))
                return GlobalUserGroup.GraphicDesigner;
            if (groups.HasFlag(UserGroups.Bot))
                return GlobalUserGroup.Bot;
            if (groups.HasFlag(UserGroups.Admin))
                return GlobalUserGroup.Administrator;
            if (groups.HasFlag(UserGroups.Moderator))
                return GlobalUserGroup.Moderator;
            if (groups.HasFlag(UserGroups.HeadRankingSupervisor))
                return GlobalUserGroup.HeadRankingSupervisor;
            if (groups.HasFlag(UserGroups.TrialRankingSupervisor))
                return GlobalUserGroup.TrialRankingSupervisor;
            if (groups.HasFlag(UserGroups.RankingSupervisor))
                return GlobalUserGroup.RankingSupervisor;
            if (groups.HasFlag(UserGroups.CommunityManager))
                return GlobalUserGroup.CommunityManager;
            if (groups.HasFlag(UserGroups.Contributor))
                return GlobalUserGroup.Contributor;
            if (groups.HasFlag(UserGroups.Donator))
                return GlobalUserGroup.Donator;
            return null;
        }

        private static FlexContainer CreateFlex(Drawable parent, ScalableVector2 size, FlexDirection direction, FlexJustifyContent justifyContent, FlexAlignItems alignItems, float gap = 0) => new FlexContainer
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

        private class RoundedTextureSprite : Sprite
        {
            private SpriteBatchOptions PlainScissorOptions { get; } = RoundedRectShader.CreateScissorSafeOptions();

            public RoundedTextureSprite(float width, float height, float cornerRadius, Texture2D image)
            {
                Size = new ScalableVector2(width, height);
                var shader = RoundedRectShader.Create(cornerRadius);
                RoundedRectShader.UpdateSize(shader, new Vector2(width, height));
                SpriteBatchOptions = new SpriteBatchOptions
                {
                    RasterizerState = RoundedRectShader.ScissorSafeRasterizerState,
                    Shader = shader
                };
                SetSource(image);
            }

            public void SetSource(Texture2D source)
            {
                if (source != null && !source.IsDisposed)
                    Image = source;
            }

            public override void Draw(GameTime gameTime)
            {
                base.Draw(gameTime);
                PlainScissorOptions.Begin();
            }
        }

        private sealed class RoundedCoverSprite : RoundedTextureSprite
        {
            private Color CoverTint { get; }

            private Texture2D CroppedTexture { get; set; }

            public RoundedCoverSprite(float width, float height, float cornerRadius, Color fallbackColor, float coverBrightness, Texture2D fallbackTexture) : base(width, height, cornerRadius, fallbackTexture)
            {
                CoverTint = new Color(coverBrightness, coverBrightness, coverBrightness, 1);
                SetSource(fallbackTexture);
                Tint = fallbackColor;
            }

            public new void SetSource(Texture2D source)
            {
                if (source == null || source.IsDisposed)
                    return;

                CroppedTexture?.Dispose();
                CroppedTexture = CreateCoverTexture(source, Math.Max(1, (int) Math.Ceiling(Width)), Math.Max(1, (int) Math.Ceiling(Height)));
                base.SetSource(CroppedTexture ?? source);
                Tint = CoverTint;
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
                    var cropWidth = sourceAspect > targetAspect ? source.Height * targetAspect : source.Width;
                    var cropHeight = sourceAspect > targetAspect ? source.Height : source.Width / targetAspect;
                    var cropX = (source.Width - cropWidth) / 2f;
                    var cropY = (source.Height - cropHeight) / 2f;

                    for (var y = 0; y < height; y++)
                    {
                        var sourceY = Math.Min(source.Height - 1, (int) (cropY + y * cropHeight / height));
                        for (var x = 0; x < width; x++)
                        {
                            var sourceX = Math.Min(source.Width - 1, (int) (cropX + x * cropWidth / width));
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

        private sealed class ProfileStatPill : RoundedButton
        {
            private float ContentGap { get; }

            private float HorizontalPadding { get; }

            public ProfileStatPill(float contentGap, float horizontalPadding)
            {
                ContentGap = contentGap;
                HorizontalPadding = horizontalPadding;
            }

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);
                PositionContent();
            }

            public float ResizeToContent()
            {
                if (Icon == null || Label == null)
                    return Width;

                var totalWidth = Icon.Width + ContentGap + Label.Width;
                var desiredWidth = totalWidth + HorizontalPadding * 2;
                if (Math.Abs(Width - desiredWidth) >= 0.01f)
                    Width = desiredWidth;

                PositionContent();
                return desiredWidth;
            }

            private void PositionContent()
            {
                if (Icon == null || Label == null)
                    return;

                var totalWidth = Icon.Width + ContentGap + Label.Width;
                Icon.X = -totalWidth / 2 + Icon.Width / 2;
                Label.X = Icon.X + Icon.Width / 2 + ContentGap + Label.Width / 2;
            }
        }
    }
}
