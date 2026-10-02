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

    public SongRequestsSection(ScalableVector2 size)
    {
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
        Layout = new FlexContainer
        {
            Parent = this,
            Position = new ScalableVector2(10, 10),
            Size = new ScalableVector2(Width - 20 + 10, Height - 20),
            Direction = FlexDirection.Column,
            AlignItems = FlexAlignItems.Stretch
        };
    }

    private void CreateHeader()
    {
        var header = new Sprite
        {
            Parent = Layout,
            Size = new ScalableVector2(736, 60),
            Tint = ColorHelper.HexToColor("#273038")
        };
        Layout.SetItemOptions(header, new FlexItemOptions { Basis = 60, Shrink = 0 });

        var headerLayout = new FlexContainer
        {
            Parent = header,
            Size = new ScalableVector2(header.Width - 20, header.Height - 20),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            JustifyContent = FlexJustifyContent.SpaceBetween
        };

        var actionsLayout = new FlexContainer
        {
            Parent = headerLayout,
            Size = new ScalableVector2(234 + 10 + 232, 40),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            ColumnGap = 10
        };
        headerLayout.SetItemOptions(actionsLayout, new FlexItemOptions { Basis = actionsLayout.Width, Shrink = 0 });

        AlertsButton = new RoundedButton((sender, args) => ToggleAlerts())
        {
            Parent = actionsLayout,
            Size = new ScalableVector2(234, 40),
            CornerRadius = SkinV2BorderRadiusConfig.Normal,
            Tint = ColorHelper.HexToColor("#181E25"),
            PerformHoverFade = false
        };
        AlertsButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Hub_DisplayAlert"), 18, Color.White);
        AlertsButton.Label.Alignment = Alignment.MidLeft;
        AlertsButton.Label.X = 10;

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
            Size = new ScalableVector2(232, 40),
            CornerRadius = SkinV2BorderRadiusConfig.Normal,
            Tint = ColorHelper.HexToColor("#9146FF"),
            PerformHoverFade = false
        };
        TwitchButton.SetIcon(UserInterface.TwitchIconV2, new Vector2(18, 20));

        ClearButton = new RoundedButton((sender, args) => ClearRequests())
        {
            Parent = headerLayout,
            Size = new ScalableVector2(124, 40),
            CornerRadius = SkinV2BorderRadiusConfig.Normal,
            Tint = ColorHelper.HexToColor("#181E25"),
            PerformHoverFade = false
        };
        ClearButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Hub_NotificationClearAll"), 18, Color.White);
        ClearButton.SetIcon(UserInterface.HubNotificationsClearIcon, new Vector2(20, 20));

        UpdateHeaderButtons();

        DropdownStyle = new SkinV2DropdownConfig
        {
            Height = 40,
            ItemHeight = 40,
            FontSize = 18,
            TriggerColor = "#181E25FF",
            ItemColor = "#181E25FF",
            HoverColor = "#354451FF",
            SelectedItemColor = "#6B83B2FF",
            TextColor = "#8CAFEAFF",
            IconColor = "#8CAFEAFF",
            CornerRadius = SkinV2BorderRadiusConfig.Normal
        };
    }

    private void CreateContent()
    {
        var requests = new List<SongRequest>(OnlineManager.SongRequests);
        var poolSize = (int)Math.Ceiling(Math.Max(0, Height - 80) / 146) + 3;
        Scroll = new SongRequestList(requests, poolSize, new ScalableVector2(736, 1))
        {
            Parent = Layout,
            Tint = ColorHelper.HexToColor("#273038"),
            EasingType = Easing.OutQuint,
            TimeToCompleteScroll = 1200,
            ScrollSpeed = 220,
            InputEnabled = true
        };
        Scroll.Scrollbar.Width = 4;
        Scroll.Scrollbar.Tint = ColorHelper.HexToColor("#D9E3F4");
        Layout.SetItemOptions(Scroll, new FlexItemOptions { Basis = 0, Grow = 1 });
        Scroll.OnRowClicked += ShowRequestMenu;

        EmptyMessage = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), "", 24)
        {
            Parent = Scroll,
            Alignment = Alignment.MidCenter,
            TextAlignment = TextAlignment.Center,
            MaxWidth = Scroll.Width - 40,
            Tint = Color.White,
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
            TwitchButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get(string.IsNullOrEmpty(username) ? "Screen_Hub_ConnectTwitch" : "Screen_Hub_UnlinkTwitch"), 18, Color.White);
        }
    }

    private void UpdateEmptyMessage()
    {
        EmptyMessage.Visible = Scroll.AvailableItems.Count == 0;
        EmptyMessage.Text = LocalizationManager.Get(OnlineManager.Connected ? "Screen_Hub_NoRequests" : "Screen_Hub_RequestsLoginRequired");
    }

    private void ToggleAlerts()
    {
        ConfigManager.DisplaySongRequestNotifications.Value = !ConfigManager.DisplaySongRequestNotifications.Value;
    }

    private void ToggleTwitch()
    {
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

    public override void Destroy()
    {
        OnlineManager.Status.ValueChanged -= OnConnectionStatusChanged;
        UnsubscribeFromRequests();
        Scroll.OnRowClicked -= ShowRequestMenu;
        Deactivate();
        base.Destroy();
    }
}
