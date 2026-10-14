using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Helpers;
using Quaver.Server.Client.Objects.Twitch;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Form;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.Hub.SongRequests.Header;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.V2Hub.SongRequests;

public class SongRequestsSection : Container
{
    private SkinV2HubConfig HubConfig { get; }
    private SkinStoreV2Lease Skin { get; }
    private SkinV2HubSectionConfig SectionConfig => HubConfig.Sections;
    private SkinV2HubSongRequestsConfig Config => HubConfig.SongRequests;
    private bool PreviewMode { get; }

    private FlexContainer Layout { get; set; }
    private RoundedButton AlertsButton { get; set; }
    private ToggleV2 AlertsToggle { get; set; }
    private RoundedButton TwitchButton { get; set; }
    private RoundedButton ClearButton { get; set; }
    private SongRequestList Scroll { get; set; }
    private SpriteTextPlus EmptyMessage { get; set; }
    private SongRequestRightClickOptions RequestMenu { get; set; }
    private SkinV2DropdownConfig DropdownStyle { get; set; }
    private OnlineClient SubscribedClient { get; set; }
    private string PreviousTwitchUsername { get; set; }
    private bool TwitchLabelInitialized { get; set; }
    internal Sprite Header { get; private set; }

    public SongRequestsSection(ScalableVector2 size, SkinStoreV2Lease skin, SkinV2Config rootConfig, bool previewMode = false)
    {
        Skin = skin;
        HubConfig = rootConfig.Screens.Hub;
        PreviewMode = previewMode;
        Size = size;
        CreateLayout();
        CreateHeader();
        CreateContent();

        OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
        SubscribeToRequests();
        Layout.RefreshLayout();
    }

    private void CreateLayout()
    {
        var padding = SectionConfig.ContentPadding;
        Layout = new FlexContainer
        {
            Parent = this,
            Position = new ScalableVector2(padding, padding),
            Size = new ScalableVector2(Width - padding, Height - padding * 2),
            Direction = FlexDirection.Column,
            AlignItems = FlexAlignItems.Stretch
        };
    }

    private void CreateHeader()
    {
        var section = SectionConfig;
        Header = new Sprite
        {
            Parent = Layout,
            Size = new ScalableVector2(Layout.Width, section.HeaderHeight),
            Tint = SkinV2Color.Parse(section.BackgroundColor)
        };
        Layout.SetItemOptions(Header,
            new FlexItemOptions { Basis = section.HeaderHeight, Shrink = 0 });

        var headerLayout = new FlexContainer
        {
            Parent = Header,
            Size = new ScalableVector2(Header.Width - section.ContentPadding, Header.Height - section.ContentPadding * 2),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            JustifyContent = FlexJustifyContent.SpaceBetween
        };

        var actionsLayout = new FlexContainer
        {
            Parent = headerLayout,
            Size = new ScalableVector2(Config.AlertsButtonWidth + Config.ActionGap + Config.TwitchButtonWidth, section.ControlHeight),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            ColumnGap = Config.ActionGap
        };
        headerLayout.SetItemOptions(actionsLayout, new FlexItemOptions { Basis = actionsLayout.Width, Shrink = 0 });

        AlertsButton = new RoundedButton((sender, args) => ToggleAlerts())
        {
            Parent = actionsLayout,
            Size = new ScalableVector2(Config.AlertsButtonWidth, section.ControlHeight),
            CornerRadius = section.CornerRadius,
            Tint = SkinV2Color.Parse(section.ControlColor),
            PerformHoverFade = false
        };
        AlertsButton.SetLabel(FontManager.GetWobbleFont(section.Font), LocalizationManager.Get("Screen_Hub_DisplayAlert"), section.FontSize, SkinV2Color.Parse(section.TextColor));
        AlertsButton.Label.Alignment = Alignment.MidLeft;
        AlertsButton.Label.X = section.ContentPadding;

        AlertsToggle = new ToggleV2(ConfigManager.DisplaySongRequestNotifications)
        {
            Parent = AlertsButton,
            Alignment = Alignment.MidRight,
            X = -16,
            Scale = new Vector2(1.2f)
        };

        TwitchButton = new RoundedButton((sender, args) => ToggleTwitch())
        {
            Parent = actionsLayout,
            Size = new ScalableVector2(Config.TwitchButtonWidth, section.ControlHeight),
            CornerRadius = section.CornerRadius,
            Tint = SkinV2Color.Parse(Config.TwitchColor),
            PerformHoverFade = false
        };
        TwitchButton.SetIcon(Skin.LoadTexture(Config.TwitchIcon, UserInterface.TwitchIconV2), new Vector2(Config.TwitchIconWidth, Config.TwitchIconHeight));

        ClearButton = new RoundedButton((sender, args) => ClearRequests())
        {
            Parent = headerLayout,
            Size = new ScalableVector2(section.ClearButtonWidth, section.ControlHeight),
            CornerRadius = section.CornerRadius,
            Tint = SkinV2Color.Parse(section.ControlColor),
            PerformHoverFade = false
        };
        ClearButton.SetLabel(FontManager.GetWobbleFont(section.Font), LocalizationManager.Get("Screen_Hub_NotificationClearAll"), section.FontSize, SkinV2Color.Parse(section.TextColor));
        ClearButton.SetIcon(Skin.LoadTexture(section.ClearIcon, UserInterface.HubNotificationsClearIcon), new Vector2(section.IconSize, section.IconSize));

        UpdateHeaderButtons();

        DropdownStyle = new SkinV2DropdownConfig
        {
            Height = section.ControlHeight,
            ItemHeight = section.ControlHeight,
            Font = section.Font,
            FontSize = section.FontSize,
            TriggerColor = section.ControlColor,
            ItemColor = section.ControlColor,
            HoverColor = section.ControlHoverColor,
            SelectedItemColor = section.SelectedControlColor,
            TextColor = section.AccentTextColor,
            IconColor = section.AccentTextColor,
            CornerRadius = section.CornerRadius
        };
    }

    private void CreateContent()
    {
        var requests = new List<SongRequest>(OnlineManager.SongRequests);
        var availableHeight = Height - SectionConfig.HeaderHeight - SectionConfig.ContentPadding * 2;
        var poolSize = (int)Math.Ceiling(Math.Max(0, availableHeight) / 146) + 3;
        Scroll = new SongRequestList(requests, poolSize, new ScalableVector2(Width, 1))
        {
            Parent = Layout,
            Tint = SkinV2Color.Parse(SectionConfig.BackgroundColor),
            EasingType = Easing.OutQuint,
            TimeToCompleteScroll = 1200,
            ScrollSpeed = 220,
            InputEnabled = true
        };
        V2Scrollbar.Apply(Scroll);
        Layout.SetItemOptions(Scroll, new FlexItemOptions { Basis = 0, Grow = 1 });
        Scroll.OnRowClicked += ShowRequestMenu;

        EmptyMessage = new SpriteTextPlus(FontManager.GetWobbleFont(SectionConfig.Font), "", SectionConfig.EmptyMessageFontSize)
        {
            Parent = Scroll,
            Alignment = Alignment.MidCenter,
            TextAlignment = TextAlignment.Center,
            MaxWidth = Scroll.Width - SectionConfig.ContentPadding * 4,
            Tint = SkinV2Color.Parse(SectionConfig.TextColor),
            UsePreviousSpriteBatchOptions = true
        };
        UpdateEmptyMessage();
    }

    private void UpdateHeaderButtons()
    {
        var username = OnlineManager.TwitchUsername;
        if (!TwitchLabelInitialized || PreviousTwitchUsername != username)
        {
            TwitchLabelInitialized = true;
            PreviousTwitchUsername = username;
            TwitchButton.SetLabel(FontManager.GetWobbleFont(SectionConfig.Font), LocalizationManager.Get(string.IsNullOrEmpty(username) ? "Screen_Hub_ConnectTwitch" : "Screen_Hub_UnlinkTwitch"), 
                SectionConfig.FontSize,  SkinV2Color.Parse(SectionConfig.TextColor));
        }
    }

    private void UpdateEmptyMessage()
    {
        EmptyMessage.Visible = Scroll.AvailableItems.Count == 0;
        EmptyMessage.Text = LocalizationManager.Get(OnlineManager.Connected ? "Screen_Hub_NoRequests" : "Screen_Hub_RequestsLoginRequired");
    }

    private void ToggleAlerts()
    {
        if (PreviewMode)
            return;

        ConfigManager.DisplaySongRequestNotifications.Value = !ConfigManager.DisplaySongRequestNotifications.Value;
    }

    private void ToggleTwitch()
    {
        if (PreviewMode)
            return;

        if (string.IsNullOrEmpty(OnlineManager.TwitchUsername))
        {
            BrowserHelper.OpenURL(OnlineClient.CONNECT_TWITCH_URL, true);
            return;
        }

        if (!OnlineManager.Connected)
        {
            NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Hub_UnlinkTwitchLoginRequired"));
            return;
        }

        DialogManager.Show(new UnlinkTwitchConfirmationDialog());
    }

    private void ShowRequestMenu(SongRequest request)
    {
        if (PreviewMode)
            return;

        DismissRequestMenu();

        RequestMenu = new SongRequestRightClickOptions(request, 200, DropdownStyle, this, PlayRequest, DeleteRequest)
        {
            Parent = this,
            Position = new ScalableVector2( MouseManager.CurrentState.X - AbsolutePosition.X, MouseManager.CurrentState.Y - AbsolutePosition.Y)
        };
        RequestMenu.Open();
    }

    private void DismissRequestMenu()
    {
        RequestMenu?.Destroy();
        RequestMenu = null;
    }

    private void DeleteRequest(SongRequest request)
    {
        OnlineManager.SongRequests.Remove(request);
        RefreshRequests();
    }

    private void ClearRequests()
    {
        if (PreviewMode)
            return;

        DismissRequestMenu();
        OnlineManager.SongRequests.Clear();
        RefreshRequests();
    }

    private void RefreshRequests()
    {
        Scroll.RefreshFeed(OnlineManager.SongRequests);
        UpdateEmptyMessage();
    }

    private void PlayRequest(SongRequest request)
    {
        if (GameBase.Game is not QuaverGame game || game.CurrentScreen?.Type != QuaverScreenType.Select)
        {
            NotificationManager.Show(NotificationLevel.Warning, LocalizationManager.Get("Screen_Hub_RequestPlayRequiresSelection"));
            return;
        }

        Scroll.MarkPlayed(request);

        if (!string.IsNullOrEmpty(request.MapMd5))
        {
            var map = MapManager.FindMapFromMd5(request.MapMd5);
            if (map != null)
            {
                MapManager.PlaySongRequest(request, map);
                return;
            }
        }

        switch ((MapGame)request.Game)
        {
            case MapGame.Quaver:
                var mapset = MapManager.Mapsets.Find(x => x.Maps.Count > 0 && x.Maps.First().Game == MapGame.Quaver && x.Maps.First().MapSetId == request.MapsetId);
                if (mapset != null)
                {
                    MapManager.PlaySongRequest(request, mapset.Maps.First());
                    return;
                }

                if (request.MapsetId <= 0 || MapsetDownloadManager.IsMapsetInQueue(request.MapsetId))
                    return;

                var download = MapsetDownloadManager.Download(request.MapsetId, request.Artist, request.Title);
                if (download == null)
                    return;

                download.Status.ValueChanged += (sender, args) =>
                {
                    if (args.Value.Status != FileDownloaderStatus.Complete || game.CurrentScreen?.Type != QuaverScreenType.Select)
                        return;

                    game.CurrentScreen.Exit(() => QuaverScreenFactory.CreateImporting(null, true));
                    DialogManager.Dialogs.OfType<OverlayDialog>().FirstOrDefault()?.Close();
                };
                break;
            case MapGame.Osu:
                BrowserHelper.OpenURL($"https://osu.ppy.sh/beatmapsets/{request.MapsetId}", true);
                break;
        }
    }

    private void SubscribeToRequests()
    {
        if (OnlineManager.Status.Value != ConnectionStatus.Connected || OnlineManager.Client == null || SubscribedClient == OnlineManager.Client)
            return;

        UnsubscribeFromRequests();
        SubscribedClient = OnlineManager.Client;
        SubscribedClient.OnSongRequestReceived += OnSongRequestReceived;
    }

    private void UnsubscribeFromRequests()
    {
        if (SubscribedClient == null)
            return;

        SubscribedClient.OnSongRequestReceived -= OnSongRequestReceived;
        SubscribedClient = null;
    }

    private void OnSongRequestReceived(object sender, SongRequestEventArgs e) => AddScheduledUpdate(RefreshRequests);

    private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e) => AddScheduledUpdate(() =>
    {
        if (e.Value == ConnectionStatus.Connected)
            SubscribeToRequests();
        else
            UnsubscribeFromRequests();

        RefreshRequests();
    });

    public override void Update(GameTime gameTime)
    {
        UpdateHeaderButtons();
        base.Update(gameTime);
    }

    public void Deactivate() => DismissRequestMenu();

    internal void ResizeHeight(float height)
    {
        if (Math.Abs(Height - height) < 0.01f)
            return;

        Height = height;
        Layout.Height = Math.Max(1, height - SectionConfig.ContentPadding * 2);
        Layout.RefreshLayout();
    }

    public override void Destroy()
    {
        OnlineManager.Status.ValueChanged -= OnConnectionStatusChanged;
        UnsubscribeFromRequests();
        Scroll.OnRowClicked -= ShowRequestMenu;
        Deactivate();
        base.Destroy();
    }
}
