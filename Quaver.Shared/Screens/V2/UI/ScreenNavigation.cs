using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Client;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Menu.Border.Components.Users;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Main.UI;
using Quaver.Shared.Screens.Options;
using Quaver.Shared.Screens.V2.SkinEditor;
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
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Navigation;
using Wobble.Graphics.UI.Tooltips;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     Shared top and bottom chrome for replacement screens.
    /// </summary>
    internal sealed class ScreenNavigation : Container
    {
        public const string ElementKey = "quaver-screen-navigation";

        private const int LogoEnterAnimationDuration = 220;

        private const int LogoExitAnimationDuration = 180;

        private const int ButtonEnterAnimationDuration = 100;

        private static readonly Rectangle BundledLogoSourceRectangle = new Rectangle(0, 0, 153, 132);

        private const int BundledLogoTextureWidth = 53;

        private const int BundledLogoTextureHeight = 46;

        private SkinStoreV2Lease Skin { get; }

        private SkinV2NavigationConfig Config { get; }

        private SkinV2BrandConfig BrandConfig { get; }

        private NavigationBar TopBar { get; }

        private NavigationBar BottomBar { get; }

        private ProfileControl ProfileButton { get; }

        private RoundedButton DonateButton { get; }

        private RoundedButton HubButton { get; }

        private TextureRegion HubListIcon { get; }

        private TextureRegion HubMenuIcon { get; }

        private OnlineHub SubscribedOnlineHub { get; set; }

        private List<Drawable> NavigationButtons { get; } = new List<Drawable>();

        private List<Drawable> FooterButtons { get; } = new List<Drawable>();

        private List<Drawable> TopLayoutButtons { get; } = new List<Drawable>();

        private List<DelayedButtonReveal> DelayedButtonReveals { get; } =
            new List<DelayedButtonReveal>();

        private Dictionary<string, Texture2D> BundledLogoTextures { get; } =
            new Dictionary<string, Texture2D>();

        private Sprite ApplicationLogo { get; set; }

        private Container ApplicationLogoSlot { get; set; }

        private bool ApplicationLogoUsesBundledAsset { get; set; }

        private Container OutgoingApplicationLogoSlot { get; set; }

        private double OutgoingApplicationLogoTimeRemaining { get; set; }

        private FooterLayout? CurrentFooterLayout { get; set; }

        private TopLayout? CurrentTopLayout { get; set; }

        private QuaverScreenType CurrentActiveScreen { get; set; } = QuaverScreenType.None;

        private ScreenNavigation(SkinV2Config previewConfig = null)
        {
            Skin = SkinManager.AcquireV2();
            var rootConfig = previewConfig ?? Skin.Config;
            Config = rootConfig.Shared.Navigation;
            BrandConfig = rootConfig.Shared.Brand;
            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);

            TopBar = CreateBar(Alignment.TopLeft, Config.Bar);
            BottomBar = CreateBar(Alignment.BotLeft, Config.Footer);

            ProfileButton = new ProfileControl(Config.Profile,
                SkinV2Color.Parse(Config.Button.BackgroundColor),
                UserInterface.OfflineAvatar,
                Config.Button.Size);
            HubListIcon = GlobalIcons.Get(GlobalIcon.BurgerRedDot);
            HubMenuIcon = GlobalIcons.Get(GlobalIcon.Burger);
            DonateButton = AddIconButton(TopBar, NavigationBarRegion.Right,
                GlobalIcons.Get(GlobalIcon.Heart),
                LocalizationManager.Get("Screen_Main_Menu_Donate"), ShowDonateMessage,
                TooltipAnchor.BottomCenter);
            HubButton = AddIconButton(TopBar, NavigationBarRegion.Right, HubMenuIcon,
                "Online Hub", ToggleOnlineHub, TooltipAnchor.BottomCenter);

            ShowMainTopBar();
            ShowDefaultFooter();
        }

        public void ShowMainTopBar()
        {
            if (CurrentTopLayout == TopLayout.Main)
                return;

            var animateButtonsEntrance = CurrentTopLayout == TopLayout.Application;
            if (animateButtonsEntrance)
                BeginApplicationLogoExit();

            var buttons = new List<RoundedButton>();
            SetTopBar(layout =>
            {
                buttons.Add(layout.AddIconButton(NavigationBarRegion.Left,
                    GlobalIcons.Get(GlobalIcon.Jukebox),
                    LocalizationManager.Get("Screen_Main_Menu_Jukebox"), OpenMusicPlayer));
                buttons.Add(layout.AddIconButton(NavigationBarRegion.Left, GlobalIcons.Get(GlobalIcon.Chat),
                    LocalizationManager.Get("Screen_Options_ToggleChatOverlay"), ToggleChat));
                layout.AddSharedRightControls();
            });

            AttachOutgoingApplicationLogo();
            if (animateButtonsEntrance)
                AnimateNavigationButtonsEntrance(buttons, LogoExitAnimationDuration);

            CurrentTopLayout = TopLayout.Main;
            CurrentActiveScreen = QuaverScreenType.Menu;
        }

        public void ShowApplicationTopBar(QuaverScreenType activeScreen)
        {
            if (CurrentTopLayout == TopLayout.Application && CurrentActiveScreen == activeScreen)
                return;

            var animateLogoEntrance = CurrentTopLayout != TopLayout.Application;
            DestroyOutgoingApplicationLogo();

            var buttons = new List<RoundedButton>();
            SetTopBar(layout =>
            {
                layout.AddApplicationLogo();
                buttons.Add(layout.AddApplicationButton(GlobalIcon.Home, "Screen_Main_Menu_Home", NavigateHome,
                    activeScreen == QuaverScreenType.Menu));
                buttons.Add(layout.AddApplicationButton(GlobalIcon.SinglePlayer, "Screen_Main_SinglePlayer",
                    NavigateSinglePlayer, activeScreen == QuaverScreenType.Select));
                buttons.Add(layout.AddApplicationButton(GlobalIcon.Multiplayer, "Screen_Main_Multiplayer",
                    NavigateMultiplayer, activeScreen == QuaverScreenType.Lobby ||
                                         activeScreen == QuaverScreenType.Multiplayer));
                buttons.Add(layout.AddApplicationButton(GlobalIcon.Download, "Screen_Download_Download",
                    NavigateDownload, activeScreen == QuaverScreenType.Download));
                buttons.Add(layout.AddApplicationButton(GlobalIcon.Chat, "Screen_Main_Menu_Chat",
                    ToggleChat, false));
                buttons.Add(layout.AddApplicationButton(GlobalIcon.Jukebox,
                    "Screen_Overlay_VolumeControl_Music",
                    OpenMusicPlayer, activeScreen == QuaverScreenType.Music));
                layout.AddSharedRightControls();
            });

            if (animateLogoEntrance)
            {
                AnimateApplicationLogoEntrance();
                AnimateNavigationButtonsEntrance(buttons, LogoEnterAnimationDuration);
            }

            CurrentTopLayout = TopLayout.Application;
            CurrentActiveScreen = activeScreen;
        }

        public void ShowDefaultFooter()
        {
            if (CurrentFooterLayout == FooterLayout.Default)
                return;

            SetFooter(layout =>
            {
                layout.AddIconButton(NavigationBarRegion.Left, GlobalIcons.Get(GlobalIcon.Website),
                    LocalizationManager.Get("Screen_Main_Menu_Website"),
                    () => BrowserHelper.OpenURL("https://quavergame.com"));
                layout.AddIconButton(NavigationBarRegion.Left, GlobalIcons.Get(GlobalIcon.Discord),
                    LocalizationManager.Get("Screen_Main_Menu_Discord"),
                    () => BrowserHelper.OpenURL("https://discord.gg/quaver", true));
                layout.AddIconButton(NavigationBarRegion.Left, GlobalIcons.Get(GlobalIcon.GitHub),
                    LocalizationManager.Get("Screen_Main_Menu_GitHub"),
                    () => BrowserHelper.OpenURL("https://github.com/Quaver"));

                layout.AddIconButton(NavigationBarRegion.Right, GlobalIcons.Get(GlobalIcon.Volume),
                    LocalizationManager.Get("Screen_Options_Volume"), ShowVolume);
                layout.AddIconButton(NavigationBarRegion.Right, GlobalIcons.Get(GlobalIcon.Options),
                    LocalizationManager.Get("Screen_Main_Options"),
                    () => DialogManager.Show(new OptionsDialog()));
                layout.AddIconButton(NavigationBarRegion.Right, GlobalIcons.Get(GlobalIcon.Quit),
                    LocalizationManager.Get("Screen_Main_QuitGame"),
                    () => DialogManager.Show(new QuitDialog()));
            });

            CurrentFooterLayout = FooterLayout.Default;
        }

        public void ShowSelectionFooter()
        {
            if (CurrentFooterLayout == FooterLayout.Selection)
                return;

            SetFooter(layout => layout.AddRoundedButton(NavigationBarRegion.Right,
                new NavigationBarButtonOptions
                {
                    IconRegion = GlobalIcons.Get(GlobalIcon.Play),
                    IconSize = new Vector2(Config.Button.IconSize, Config.Button.IconSize),
                    Text = LocalizationManager.Get("Screen_Selection_Play"),
                    Font = FontManager.GetWobbleFont(Fonts.InterBold),
                    FontSize = SkinV2FontSizesConfig.TextBase,
                    WidthMode = ButtonSizeMode.Auto,
                    Height = Config.Button.Size,
                    AutoSizePadding = new Vector2(Config.EdgePadding, 0),
                    CornerRadius = Config.Button.CornerRadius,
                    BackgroundColor = SkinV2Color.Parse(Config.Button.BackgroundColor),
                    ForegroundColor = SkinV2Color.Parse(Config.Button.ForegroundColor),
                    AlwaysShowLabel = true,
                    ClickAction = (sender, args) => ShowSelectionPlayUnavailable()
                }));

            CurrentFooterLayout = FooterLayout.Selection;
        }

        /// <summary>
        ///     Replaces the top bar contents. Screens may call this again whenever their state changes.
        /// </summary>
        public void SetTopBar(Action<ScreenNavigationLayout> configure)
        {
            ClearTopLayout();
            configure?.Invoke(new ScreenNavigationLayout(this, TopBar, TopLayoutButtons,
                TooltipAnchor.BottomCenter));
            CurrentTopLayout = TopLayout.Custom;
            CurrentActiveScreen = QuaverScreenType.None;
        }

        /// <summary>
        ///     Replaces the footer contents. Screens may call this again whenever their state changes.
        /// </summary>
        public void SetFooter(Action<ScreenNavigationLayout> configure)
        {
            ClearFooter();
            configure?.Invoke(new ScreenNavigationLayout(this, BottomBar, FooterButtons,
                TooltipAnchor.TopCenter));
            CurrentFooterLayout = FooterLayout.Custom;
        }

        public static ScreenNavigation EnsureAttached(Container parent, SkinV2Config previewConfig = null)
        {
            if (ScreenManager.TryGetElement<ScreenNavigation>(ElementKey, out var navigation))
            {
                if (previewConfig == null &&
                    navigation.Skin.Generation == SkinManager.SkinV2?.Generation)
                {
                    if (navigation.Parent != parent)
                        navigation.Parent = parent;
                    navigation.ResetTransientState();
                    navigation.ResizeToWindow();
                    return navigation;
                }

                ScreenManager.RemoveElement(ElementKey);
            }

            navigation = new ScreenNavigation(previewConfig) { Parent = parent };
            ScreenManager.RegisterElement(ElementKey, navigation);
            return navigation;
        }

        public static ScreenNavigation ReplaceAttached(Container parent, SkinV2Config previewConfig)
        {
            ScreenManager.RemoveElement(ElementKey);
            return EnsureAttached(parent, previewConfig);
        }

        public IReadOnlyList<SkinEditorTarget> GetSkinEditorTargets()
        {
            var targets = new List<SkinEditorTarget>
            {
                new SkinEditorTarget("navigation-top",
                LocalizationManager.Get("SkinEditor_Component_TopNavigation"),
                "Shared.Navigation.Bar", TopBar),
                new SkinEditorTarget("navigation-bottom",
                LocalizationManager.Get("SkinEditor_Component_BottomNavigation"),
                "Shared.Navigation.Footer", BottomBar),
                new SkinEditorTarget("navigation-buttons",
                LocalizationManager.Get("SkinEditor_Component_NavigationButtons"),
                "Shared.Navigation.Button", NavigationButtons.ToArray()),
                new SkinEditorTarget("navigation-profile",
                LocalizationManager.Get("SkinEditor_Component_Profile"),
                "Shared.Navigation.Profile", ProfileButton)
            };

            if (ApplicationLogoUsesBundledAsset && ApplicationLogo != null)
            {
                targets.Add(new SkinEditorTarget("brand-logo-accent",
                    LocalizationManager.Get("SkinEditor_Component_LogoAccent"),
                    "Shared.Brand", ApplicationLogo));
            }

            return targets;
        }

        public override void Update(GameTime gameTime)
        {
            ResizeToWindow();
            EnsureOnlineHubSubscription();
            base.Update(gameTime);
            UpdateOutgoingApplicationLogo(gameTime);
            UpdateDelayedButtonReveals(gameTime);
        }

        public override void Destroy()
        {
            if (SubscribedOnlineHub != null)
                SubscribedOnlineHub.UnreadStateChanged -= OnHubUnreadStateChanged;

            base.Destroy();

            foreach (var texture in BundledLogoTextures.Values)
                texture.Dispose();
            BundledLogoTextures.Clear();
            DelayedButtonReveals.Clear();
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
            TextureRegion icon, string tooltip, Action action, TooltipAnchor tooltipAnchor,
            List<Drawable> layoutItems = null)
        {
            var button = bar.AddRoundedButton(region, new NavigationBarButtonOptions
            {
                IconRegion = icon,
                IconSize = new Vector2(Config.Button.IconSize, Config.Button.IconSize),
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

            NavigationButtons.Add(button);
            layoutItems?.Add(button);
            return button;
        }

        private void AddApplicationLogo(List<Drawable> layoutItems)
        {
            ApplicationLogoUsesBundledAsset = string.IsNullOrWhiteSpace(Config.Logo.Image);
            if (ApplicationLogoUsesBundledAsset)
            {
                var accentColor = SkinV2Color.Parse(BrandConfig.AccentColor);
                ApplicationLogo = accentColor == SkinV2Color.Parse(SkinV2BrandConfig.DefaultAccentColor)
                    ? new Sprite
                    {
                        Image = LoadBundledLogoTexture(
                            "Quaver.Resources/Textures/UI/Screens/Main/Logos/logo-colored.png")
                    }
                    : new TintableLogo(
                        LoadBundledLogoTexture(
                            "Quaver.Resources/Textures/UI/Screens/Main/Logos/logo.png"),
                        LoadBundledLogoTexture(
                            "Quaver.Resources/Textures/UI/Screens/Main/Logos/logo-tail.png"),
                        Color.Lerp(Color.White, accentColor, 0.5f),
                        LoadBundledLogoTexture(
                            "Quaver.Resources/Textures/UI/Screens/Main/Logos/logo-accent.png"),
                        accentColor);
            }
            else
            {
                var fallback = TextureManager.Load(
                    "Quaver.Resources/Textures/UI/Screens/Main/Logos/logo-colored.png");
                var texture = Skin.LoadTexture(Config.Logo.Image, fallback);
                ApplicationLogo = texture == fallback
                    ? new Sprite
                    {
                        Image = LoadBundledLogoTexture(
                            "Quaver.Resources/Textures/UI/Screens/Main/Logos/logo-colored.png")
                    }
                    : new Sprite { Image = texture };
            }

            var aspectRatio = (float) ApplicationLogo.ImageWidth / ApplicationLogo.ImageHeight;
            var width = Config.Logo.Height * aspectRatio;
            ApplicationLogo.Size = new ScalableVector2(width, Config.Logo.Height);
            ApplicationLogo.Alignment = Alignment.MidLeft;

            ApplicationLogoSlot = new Container
            {
                Size = new ScalableVector2(width, Config.Logo.Height)
            };
            ApplicationLogo.Parent = ApplicationLogoSlot;

            TopBar.Add(NavigationBarRegion.Left, ApplicationLogoSlot);
            layoutItems.Add(ApplicationLogoSlot);
        }

        private Texture2D LoadBundledLogoTexture(string resource)
        {
            if (BundledLogoTextures.TryGetValue(resource, out var texture) && !texture.IsDisposed)
                return texture;

            texture = TextureRegionResizer.Create(TextureManager.Load(resource),
                BundledLogoSourceRectangle, BundledLogoTextureWidth, BundledLogoTextureHeight);
            BundledLogoTextures[resource] = texture;
            return texture;
        }

        private void AnimateApplicationLogoEntrance()
        {
            if (ApplicationLogo == null)
                return;

            ApplicationLogo.ClearAnimations();
            ApplicationLogo.X = -Config.EdgePadding - ApplicationLogo.Width;
            ApplicationLogo.MoveToX(0, Easing.OutCubic, LogoEnterAnimationDuration);
        }

        private void BeginApplicationLogoExit()
        {
            DestroyOutgoingApplicationLogo();

            if (ApplicationLogoSlot == null || ApplicationLogo == null)
                return;

            TopBar.Remove(ApplicationLogoSlot, destroy: false);
            TopLayoutButtons.Remove(ApplicationLogoSlot);

            ApplicationLogo.ClearAnimations();
            ApplicationLogo.MoveToX(-Config.EdgePadding - ApplicationLogo.Width, Easing.InCubic,
                LogoExitAnimationDuration);

            OutgoingApplicationLogoSlot = ApplicationLogoSlot;
            OutgoingApplicationLogoTimeRemaining = LogoExitAnimationDuration;
            ApplicationLogoSlot = null;
            ApplicationLogo = null;
            ApplicationLogoUsesBundledAsset = false;
        }

        private void AttachOutgoingApplicationLogo()
        {
            if (OutgoingApplicationLogoSlot != null)
                OutgoingApplicationLogoSlot.Parent = TopBar;
        }

        private void UpdateOutgoingApplicationLogo(GameTime gameTime)
        {
            if (OutgoingApplicationLogoSlot == null)
                return;

            OutgoingApplicationLogoTimeRemaining -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (OutgoingApplicationLogoTimeRemaining <= 0)
                DestroyOutgoingApplicationLogo();
        }

        private void DestroyOutgoingApplicationLogo()
        {
            if (OutgoingApplicationLogoSlot != null && !OutgoingApplicationLogoSlot.IsDisposed)
                OutgoingApplicationLogoSlot.Destroy();

            OutgoingApplicationLogoSlot = null;
            OutgoingApplicationLogoTimeRemaining = 0;
        }

        private void AnimateNavigationButtonsEntrance(IEnumerable<RoundedButton> buttons, int delay)
        {
            foreach (var button in buttons)
            {
                button.ClearAnimations();
                button.Alpha = 0;
                button.Visible = false;
                button.IsInteractionEnabled = false;
                DelayedButtonReveals.Add(new DelayedButtonReveal
                {
                    Button = button,
                    TimeRemaining = delay
                });
            }
        }

        private void UpdateDelayedButtonReveals(GameTime gameTime)
        {
            for (var i = DelayedButtonReveals.Count - 1; i >= 0; i--)
            {
                var delayed = DelayedButtonReveals[i];
                if (delayed.Button.IsDisposed)
                {
                    DelayedButtonReveals.RemoveAt(i);
                    continue;
                }

                delayed.TimeRemaining -= gameTime.ElapsedGameTime.TotalMilliseconds;
                if (delayed.TimeRemaining > 0)
                    continue;

                if (!delayed.RevealStarted)
                {
                    delayed.Button.Visible = true;
                    delayed.Button.Alpha = 0;
                    delayed.Button.FadeTo(1, Easing.OutCubic, ButtonEnterAnimationDuration);
                    delayed.RevealStarted = true;
                    delayed.TimeRemaining = ButtonEnterAnimationDuration;
                    continue;
                }

                delayed.Button.IsInteractionEnabled = true;
                DelayedButtonReveals.RemoveAt(i);
            }
        }

        private RoundedButton AddApplicationButton(GlobalIcon icon, string localizationKey, Action action,
            bool active, List<Drawable> layoutItems)
        {
            var button = TopBar.AddRoundedButton(NavigationBarRegion.Left,
                new NavigationBarButtonOptions
                {
                    IconRegion = GlobalIcons.Get(icon),
                    IconSize = new Vector2(Config.Button.IconSize, Config.Button.IconSize),
                    Text = LocalizationManager.Get(localizationKey),
                    Font = FontManager.GetWobbleFont(Fonts.InterBold),
                    FontSize = SkinV2FontSizesConfig.TextBase,
                    Width = Config.Button.Size,
                    Height = Config.Button.Size,
                    AutoSizePadding = new Vector2(Config.EdgePadding, 0),
                    CornerRadius = Config.Button.CornerRadius,
                    BackgroundColor = SkinV2Color.Parse(Config.Button.BackgroundColor),
                    ForegroundColor = SkinV2Color.Parse(Config.Button.ForegroundColor),
                    ExpandLabelOnHover = true,
                    AlwaysShowLabel = active,
                    ExpandedLabelRightPadding = Config.Button.ExpandedLabelRightPadding,
                    ClickAction = (sender, args) => action()
                });

            NavigationButtons.Add(button);
            layoutItems.Add(button);
            return button;
        }

        private void AddSharedRightControls()
        {
            TopBar.Add(NavigationBarRegion.Right, ProfileButton);
            TopBar.Add(NavigationBarRegion.Right, DonateButton);
            TopBar.Add(NavigationBarRegion.Right, HubButton);
        }

        private void ClearTopLayout()
        {
            TopBar.Clear(destroy: false);

            foreach (var button in TopLayoutButtons)
            {
                NavigationButtons.Remove(button);
                button.Destroy();
            }

            TopLayoutButtons.Clear();
            ApplicationLogo = null;
            ApplicationLogoSlot = null;
            ApplicationLogoUsesBundledAsset = false;
        }

        private void ClearFooter()
        {
            foreach (var button in FooterButtons)
                NavigationButtons.Remove(button);

            FooterButtons.Clear();
            BottomBar.Clear(destroy: true);
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
            var current = HubButton.Icon.Region;
            if (!current.HasValue || !ReferenceEquals(current.Value.Texture, icon.Texture) ||
                current.Value.SourceRectangle != icon.SourceRectangle)
                HubButton.Icon.Region = icon;
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

        private static void NavigateHome()
        {
            if (GameBase.Game is QuaverGame game &&
                game.CurrentScreen?.Type != QuaverScreenType.Menu)
                game.CurrentScreen?.Exit(() => QuaverScreenFactory.CreateMainMenu());
        }

        private static void NavigateSinglePlayer()
        {
            if (GameBase.Game is QuaverGame game &&
                game.CurrentScreen?.Type != QuaverScreenType.Select)
                game.CurrentScreen?.Exit(() => QuaverScreenFactory.CreateSelection());
        }

        private static void NavigateMultiplayer()
        {
            if (!(GameBase.Game is QuaverGame game) ||
                game.CurrentScreen?.Type == QuaverScreenType.Lobby ||
                game.CurrentScreen?.Type == QuaverScreenType.Multiplayer)
                return;

            if (!OnlineManager.Connected)
            {
                NotificationManager.Show(NotificationLevel.Error,
                    LocalizationManager.Get("Screen_Main_MultiplayerLoginRequired"));
                return;
            }

            if (MapManager.Mapsets.Count == 0)
            {
                if (OnlineManager.Status.Value == ConnectionStatus.Connected)
                {
                    game.CurrentScreen?.Exit(() => QuaverScreenFactory.CreateDownloading());
                    return;
                }

                NotificationManager.Show(NotificationLevel.Error,
                    LocalizationManager.Get("Screen_Main_NoMapsLoaded"));
                return;
            }

            game.CurrentScreen?.Exit(() => QuaverScreenFactory.CreateMultiplayerLobby());
        }

        private static void NavigateDownload()
        {
            if (!(GameBase.Game is QuaverGame game) ||
                game.CurrentScreen?.Type == QuaverScreenType.Download)
                return;

            var previousScreen = game.CurrentScreen?.Type ?? QuaverScreenType.Menu;
            game.CurrentScreen?.Exit(() => QuaverScreenFactory.CreateDownloading(previousScreen));
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

        private static void ShowSelectionPlayUnavailable() =>
            NotificationManager.Show(NotificationLevel.Warning,
                LocalizationManager.Get("Screen_Main_NotImplemented"));

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

        private enum FooterLayout
        {
            Default,
            Selection,
            Custom
        }

        private enum TopLayout
        {
            Main,
            Application,
            Custom
        }

        private sealed class DelayedButtonReveal
        {
            public RoundedButton Button { get; set; }

            public double TimeRemaining { get; set; }

            public bool RevealStarted { get; set; }
        }

        public sealed class ScreenNavigationLayout
        {
            private ScreenNavigation Navigation { get; }

            private NavigationBar Bar { get; }

            private List<Drawable> Items { get; }

            private TooltipAnchor TooltipAnchor { get; }

            public ScreenNavigationLayout(ScreenNavigation navigation, NavigationBar bar,
                List<Drawable> items, TooltipAnchor tooltipAnchor)
            {
                Navigation = navigation;
                Bar = bar;
                Items = items;
                TooltipAnchor = tooltipAnchor;
            }

            public void Add(Drawable drawable, NavigationBarRegion region)
            {
                Bar.Add(region, drawable);
                Items.Add(drawable);
            }

            public RoundedButton AddRoundedButton(NavigationBarRegion region,
                NavigationBarButtonOptions options)
            {
                var button = Bar.AddRoundedButton(region, options);
                Navigation.NavigationButtons.Add(button);
                Items.Add(button);
                return button;
            }

            public RoundedButton AddIconButton(NavigationBarRegion region, TextureRegion icon,
                string tooltip, Action action) =>
                Navigation.AddIconButton(Bar, region, icon, tooltip, action, TooltipAnchor, Items);

            public void AddApplicationLogo()
            {
                if (Bar != Navigation.TopBar)
                    throw new InvalidOperationException("The application logo can only be added to the top bar.");

                Navigation.AddApplicationLogo(Items);
            }

            public RoundedButton AddApplicationButton(GlobalIcon icon, string localizationKey, Action action,
                bool active)
            {
                if (Bar != Navigation.TopBar)
                    throw new InvalidOperationException("Application buttons can only be added to the top bar.");

                return Navigation.AddApplicationButton(icon, localizationKey, action, active, Items);
            }

            public void AddSharedRightControls()
            {
                if (Bar != Navigation.TopBar)
                    throw new InvalidOperationException("Shared controls can only be added to the top bar.");

                Navigation.AddSharedRightControls();
            }
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
