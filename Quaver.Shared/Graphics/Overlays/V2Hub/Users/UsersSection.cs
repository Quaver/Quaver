using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Screens.V2.UI.Filters;
using Quaver.Shared.Skinning;
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
    private V2FilterSearchTextbox SearchBox { get; set; }
    private Bindable<string> SearchQuery { get; } = new Bindable<string>(string.Empty);
    
    private Bindable<UserFilter> Filter { get; } = new Bindable<UserFilter>(UserFilter.All);
    private V2Dropdown<UserFilter> FilterDropdown { get; set; }
    private SkinV2DropdownConfig DropdownStyle { get; set; }
    private UserRightClickOptions UserDropdownMenu { get; set; }
    
    private FlexContainer Layout { get; set; }
    private UserList Scroll { get; set; }

    public UsersSection(ScalableVector2 size)
    {
        Size = size;
        CreateLayout();
        CreateHeader();
        CreateContent();
        
        Layout.RefreshLayout();
    }

    private void CreateLayout()
    {
        Layout = new FlexContainer
        {
            Parent = this,
            Position = new ScalableVector2(10, 10),
            Size = new ScalableVector2(Width - 20 + 10, Height - 20), // +14 is for scroll thumb spacing
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
            Tint = ColorHelper.FromHex("#273038")
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
        
        var searchStyle = new V2FilterFieldStyle
        {
            Height = 40,
            SearchIconSize = 18,
            SearchIconInset = 10,
            CornerRadius = SkinV2BorderRadiusConfig.Normal,
            BackgroundColor = ColorHelper.FromHex("#181E25"),
            TextColor = Color.White,
            PlaceholderColor = ColorHelper.FromHex("#8CAFEA80"),
            CursorColor = Color.White
        };

        SearchBox = new V2FilterSearchTextbox(SearchQuery, LocalizationManager.Get("Screen_Hub_SearchUsers"), FontManager.GetWobbleFont(Fonts.InterBold), 18, searchStyle, 464)
        {
            Parent = headerLayout
        };

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
        
        FilterDropdown = new V2Dropdown<UserFilter>(242, Filter, new DropdownEntry<UserFilter>[] 
            {
                new DropdownOption<UserFilter>(UserFilter.All, LocalizationManager.Get("Screen_Selection_All")),
                new DropdownOption<UserFilter>(UserFilter.Friends, LocalizationManager.Get("Screen_Selection_Friends")),
                new DropdownOption<UserFilter>(UserFilter.Country, LocalizationManager.Get("Screen_Selection_Country"))
            },
            FontManager.GetWobbleFont(Fonts.InterBold), DropdownStyle, this)
        {
            Parent = headerLayout
        };
    }
    
    private void CreateContent()
    {
        var userList = OnlineManager.OnlineUsers == null ? new List<User>() : OnlineManager.OnlineUsers.Values.ToList();
        Scroll = new UserList(userList, ((int)Math.Ceiling(Height)-80)/80 +3, 0, new ScalableVector2(736, 1), new ScalableVector2(736, 1), SearchQuery, Filter)
        {
            Parent = Layout,
            Tint = ColorHelper.FromHex("#273038"),
            EasingType = Easing.OutQuint,
            TimeToCompleteScroll = 1200,
            ScrollSpeed = 220,
            InputEnabled = true
        };
        Scroll.Scrollbar.Width = 4;
        Scroll.Scrollbar.Tint = ColorHelper.FromHex("#D9E3F4");
        Layout.SetItemOptions(Scroll, new FlexItemOptions { Basis = 0, Grow = 1 });

        Scroll.OnRowClicked += ShowUserMenu;
        Scroll.ResultsCountChanged += UpdateResultsCount;
        UpdateResultsCount(Scroll.AvailableItems.Count);
    }

    private void UpdateResultsCount(int count) => SearchBox.SetResultsText(LocalizationManager.Get("Screen_Hub_UsersFound", count));

    private void ShowUserMenu(User user)
    {
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
