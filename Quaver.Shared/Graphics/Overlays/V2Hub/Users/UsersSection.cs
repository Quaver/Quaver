using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Server.Client;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Overlays.Hub.OnlineUsers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.V2.Profile;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Screens.V2.UI.Filters;
using Quaver.Shared.Skinning.V2;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.V2Hub.Users;

public class UsersSection : Container
{
    private SkinV2HubConfig HubConfig { get; }
    private SkinV2HubSectionConfig SectionConfig => HubConfig.Sections;
    private SkinV2HubUsersConfig Config => HubConfig.Users;
    private bool PreviewMode { get; }

    private V2FilterSearchTextbox SearchBox { get; set; }
    private Bindable<string> SearchQuery { get; } = new Bindable<string>(string.Empty);
    
    private Bindable<OnlineUserListFilter> Filter => ConfigManager.OnlineUserListFilterType;
    private V2Dropdown<OnlineUserListFilter> FilterDropdown { get; set; }
    private SkinV2DropdownConfig DropdownStyle { get; set; }
    
    private FlexContainer Layout { get; set; }
    private UserList Scroll { get; set; }
    private SpriteTextPlus OfflineMessage { get; set; }
    internal Sprite Header { get; private set; }

    private float HeaderHeight => SectionConfig.ControlHeight + SectionConfig.ContentPadding;

    public UsersSection(ScalableVector2 size, SkinV2Config rootConfig, bool previewMode = false)
    {
        HubConfig = rootConfig.Screens.Hub;
        PreviewMode = previewMode;
        Size = size;
        CreateLayout();
        CreateHeader();
        CreateContent();
        OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
        
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
            Size = new ScalableVector2(Layout.Width, HeaderHeight),
            Tint = SkinV2Color.Parse(section.BackgroundColor)
        };
        Layout.SetItemOptions(Header, new FlexItemOptions { Basis = HeaderHeight, Shrink = 0 });

        var headerLayout = new FlexContainer
        {
            Parent = Header,
            Size = new ScalableVector2(Header.Width - section.ContentPadding, section.ControlHeight),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            JustifyContent = FlexJustifyContent.SpaceBetween,
            ColumnGap = section.ContentPadding
        };
        
        var searchStyle = new V2FilterFieldStyle
        {
            Height = section.ControlHeight,
            SearchIconSize = Config.SearchIconSize,
            SearchIconInset = Config.SearchIconInset,
            CornerRadius = section.CornerRadius,
            BackgroundColor = SkinV2Color.Parse(section.ControlColor),
            TextColor = SkinV2Color.Parse(section.TextColor),
            PlaceholderColor = SkinV2Color.Parse(section.AccentTextColor),
            CursorColor = SkinV2Color.Parse(section.TextColor)
        };

        SearchBox = new V2FilterSearchTextbox(SearchQuery, LocalizationManager.Get("Screen_Hub_SearchUsers"), FontManager.GetWobbleFont(section.Font), section.FontSize, searchStyle, Config.SearchWidth)
        {
            Parent = headerLayout
        };
        headerLayout.SetItemOptions(SearchBox, new FlexItemOptions { Basis = Config.SearchWidth, Grow = 1, Shrink = 1 });

        DropdownStyle = new SkinV2DropdownConfig
        {
            Height = section.ControlHeight,
            ItemHeight = section.ControlHeight,
            Font = section.Font,
            FontSize = section.FontSize,
            TriggerColor = section.ControlColor,
            ItemColor = SkinV2DropdownConfig.DefaultItemColor,
            HoverColor = section.ControlHoverColor,
            SelectedItemColor = section.SelectedControlColor,
            TextColor = section.AccentTextColor,
            IconColor = section.AccentTextColor,
            CornerRadius = section.CornerRadius
        };

        FilterDropdown = new V2Dropdown<OnlineUserListFilter>(Config.FilterWidth, Filter,
            new DropdownEntry<OnlineUserListFilter>[]
            {
                new DropdownOption<OnlineUserListFilter>(OnlineUserListFilter.All, LocalizationManager.Get("Screen_Selection_All")),
                new DropdownOption<OnlineUserListFilter>(OnlineUserListFilter.Friends, LocalizationManager.Get("Screen_Selection_Friends")),
                new DropdownOption<OnlineUserListFilter>(OnlineUserListFilter.Country, LocalizationManager.Get("Screen_Selection_Country"))
            },
            FontManager.GetWobbleFont(section.Font), DropdownStyle, this)
        {
            Parent = headerLayout
        };
        headerLayout.SetItemOptions(FilterDropdown, new FlexItemOptions { Basis = Config.FilterWidth, Shrink = 0 });
    }
    
    private void CreateContent()
    {
        var userList = OnlineManager.Connected && OnlineManager.OnlineUsers != null ? OnlineManager.OnlineUsers.Values.ToList() : new List<User>();
        var availableHeight = Height - HeaderHeight - SectionConfig.ContentPadding * 2;
        Scroll = new UserList(userList, (int) Math.Ceiling(Math.Max(0, availableHeight) / 80) + 3, 0, new ScalableVector2(Layout.Width, 1), new ScalableVector2(Layout.Width, 1), SearchQuery, Filter)
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

        Scroll.OnRowClicked += ShowProfileViewer;
        Scroll.ResultsCountChanged += UpdateResultsCount;
        UpdateResultsCount(Scroll.AvailableItems.Count);

        OfflineMessage = new SpriteTextPlus(FontManager.GetWobbleFont(SectionConfig.Font), LocalizationManager.Get("Screen_Hub_UsersLoginRequired"), SectionConfig.EmptyMessageFontSize)
        {
            Parent = Scroll,
            Alignment = Alignment.MidCenter,
            TextAlignment = TextAlignment.Center,
            MaxWidth = Scroll.Width - SectionConfig.ContentPadding * 4,
            Tint = SkinV2Color.Parse(SectionConfig.TextColor),
            UsePreviousSpriteBatchOptions = true
        };
        UpdateConnectionState();
    }

    private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e) => AddScheduledUpdate(() =>
        {
            Scroll.RefreshUsers();
            UpdateConnectionState();
        });

    private void UpdateConnectionState() => OfflineMessage.Visible = !OnlineManager.Connected;

    private void UpdateResultsCount(int count) => SearchBox.SetResultsText(LocalizationManager.Get("Screen_Hub_UsersFound", count));

    private void ShowProfileViewer(User user, RectangleF anchor)
    {
        if (PreviewMode)
            return;

        FilterDropdown.CloseImmediately();
        ProfileViewerDialog.Show(user, anchor);
    }

    public void Deactivate()
    {
        SearchBox.Focused = false;
        FilterDropdown.CloseImmediately();
    }

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
        Deactivate();
        OnlineManager.Status.ValueChanged -= OnConnectionStatusChanged;
        
        Scroll.OnRowClicked -= ShowProfileViewer;
        Scroll.ResultsCountChanged -= UpdateResultsCount;
        
        base.Destroy();
        SearchQuery.Dispose();
    }
}
