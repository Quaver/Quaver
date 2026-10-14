using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Screens.V2.UI.Filters;
using Quaver.Shared.Skinning.V2;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Input;
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
    
    private Bindable<UserFilter> Filter { get; } = new Bindable<UserFilter>(UserFilter.All);
    private V2Dropdown<UserFilter> FilterDropdown { get; set; }
    private SkinV2DropdownConfig DropdownStyle { get; set; }
    private UserRightClickOptions UserDropdownMenu { get; set; }
    
    private FlexContainer Layout { get; set; }
    private UserList Scroll { get; set; }
    internal Sprite Header { get; private set; }

    public UsersSection(ScalableVector2 size, SkinV2Config rootConfig, bool previewMode = false)
    {
        HubConfig = rootConfig.Screens.Hub;
        PreviewMode = previewMode;
        Size = size;
        CreateLayout();
        CreateHeader();
        CreateContent();
        
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
        Layout.SetItemOptions(Header, new FlexItemOptions { Basis = section.HeaderHeight, Shrink = 0 });

        var headerLayout = new FlexContainer
        {
            Parent = Header,
            Size = new ScalableVector2(Header.Width - section.ContentPadding, Header.Height - section.ContentPadding * 2),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            JustifyContent = FlexJustifyContent.SpaceBetween
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

        FilterDropdown = new V2Dropdown<UserFilter>(Config.FilterWidth, Filter,
            new DropdownEntry<UserFilter>[]
            {
                new DropdownOption<UserFilter>(UserFilter.All, LocalizationManager.Get("Screen_Selection_All")),
                new DropdownOption<UserFilter>(UserFilter.Friends, LocalizationManager.Get("Screen_Selection_Friends")),
                new DropdownOption<UserFilter>(UserFilter.Country, LocalizationManager.Get("Screen_Selection_Country"))
            },
            FontManager.GetWobbleFont(section.Font), DropdownStyle, this)
        {
            Parent = headerLayout
        };
    }
    
    private void CreateContent()
    {
        var userList = OnlineManager.OnlineUsers == null ? new List<User>() : OnlineManager.OnlineUsers.Values.ToList();
        var availableHeight = Height - SectionConfig.HeaderHeight - SectionConfig.ContentPadding * 2;
        Scroll = new UserList(userList, (int) Math.Ceiling(Math.Max(0, availableHeight) / 80) + 3, 0, new ScalableVector2(Width, 1), new ScalableVector2(Width, 1), SearchQuery, Filter)
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

        Scroll.OnRowClicked += ShowUserMenu;
        Scroll.ResultsCountChanged += UpdateResultsCount;
        UpdateResultsCount(Scroll.AvailableItems.Count);
    }

    private void UpdateResultsCount(int count) => SearchBox.SetResultsText(LocalizationManager.Get("Screen_Hub_UsersFound", count));

    private void ShowUserMenu(User user)
    {
        if (PreviewMode)
            return;

        FilterDropdown.CloseImmediately();
        DismissUserMenu();

        UserDropdownMenu = new UserRightClickOptions(user, 200, DropdownStyle, this)
        {
            Parent = this,
            Position = new ScalableVector2(
                MouseManager.CurrentState.X - AbsolutePosition.X,
                MouseManager.CurrentState.Y - AbsolutePosition.Y)
        };
        UserDropdownMenu.Open();
    }

    private void DismissUserMenu()
    {
        UserDropdownMenu?.Destroy();
        UserDropdownMenu = null;
    }
    public void Deactivate()
    {
        SearchBox.Focused = false;
        FilterDropdown.CloseImmediately();
        DismissUserMenu();
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
        
        Scroll.OnRowClicked -= ShowUserMenu;
        Scroll.ResultsCountChanged -= UpdateResultsCount;
        
        base.Destroy();
        SearchQuery.Dispose();
        Filter.Dispose();
    }

    public enum UserFilter
    {
        All,
        Friends,
        Country
    }
}
