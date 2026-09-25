using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MoreLinq;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Objects;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Online;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Graphics.Overlays.V2Hub.Users;

public class UserList : PoolableScrollContainer<User>
{
    private LoadingWheel LoadingWheel { get; set; }

    private readonly Bindable<string> SearchQuery;
    private readonly Bindable<UsersSection.UserFilter> Filter;
    
    public event Action<int> ResultsCountChanged;
    public event Action<User> OnRowClicked;
    
    private double timeSinceLastInfoRequest { get; set; } = 0;
    private double timeSinceLastStatusRequest { get; set; } = 0;
    
    public UserList(List<User> availableItems, int poolSize, int poolStartingIndex, ScalableVector2 size, ScalableVector2 contentSize, Bindable<string> searchQuery, Bindable<UsersSection.UserFilter> filter, bool startFromBottom = false) : base(availableItems, poolSize, poolStartingIndex, size, contentSize, startFromBottom)
    {
        SearchQuery = searchQuery;
        SearchQuery.ValueChanged += OnSearchChanged;

        Filter = filter;
        Filter.ValueChanged += OnFilterChanged;
        
        CreateLoadingWheel();
        CreatePool();

        LoadingWheel.Visible = Pool.Count == 0;
        
        if (OnlineManager.Client != null)
        {
            OnlineManager.Client.OnUserConnected += OnUserConnected;
            OnlineManager.Client.OnUserDisconnected += OnUserDisconnected;
            OnlineManager.Client.OnUsersOnline += OnUserOnline;
            OnlineManager.Client.OnUserInfoReceived += OnUserInfoRecieved;
            OnlineManager.Client.OnUserStatusReceived += OnUserStatusReceived;
            OnlineManager.Client.OnUserFriendsListReceived += OnUserFriendsListReceived;
        }
        OnlineManager.FriendsListUserChanged += OnFriendsListUserChanged;
    }

    protected override PoolableSprite<User> CreateObject(User item, int index)
    {
        var row = new UserRow(this, item, index)
        {
            Alpha = 0
        };
        row.OnClick += user => OnRowClicked?.Invoke(user);
        return row;
    }

    private void RebuildPool()
    {
        DestroyPool();
        AvailableItems = OnlineManager.OnlineUsers.Values.Where(MatchUser).ToList();

        PoolStartingIndex = 0;
        ContentContainer.Animations.Clear();
        ContentContainer.Y = 0;
        TargetY = 0;
        PreviousTargetY = 0;
        PreviousContentContainerY = 0;

        CreatePool();
        NotifyResultsCountChanged();
    }
    private bool MatchUser(User user)
    {
        var query = SearchQuery.Value?.Trim();
        var matchSearch = string.IsNullOrEmpty(query) || (user.HasUserInfo && user.OnlineUser.Username.Contains(query, StringComparison.OrdinalIgnoreCase));
        
        var matchFilter = true;
        switch (Filter.Value)
        {
            case UsersSection.UserFilter.Country:
                matchFilter = user.HasUserInfo && user.OnlineUser.CountryFlag == OnlineManager.Self.OnlineUser.CountryFlag;
                break;
            case UsersSection.UserFilter.Friends:
                matchFilter = OnlineManager.FriendsList.Contains(user.OnlineUser.Id);
                break;
                
        }
        return matchSearch && matchFilter;
    }

    private void NotifyResultsCountChanged() => ResultsCountChanged?.Invoke(AvailableItems.Count);
    
    private void OnSearchChanged(object sender, BindableValueChangedEventArgs<string> e) => RebuildPool();
    private void OnFilterChanged(object sender, BindableValueChangedEventArgs<UsersSection.UserFilter> e) => RebuildPool();
    
    private void OnUserConnected(object sender, UserConnectedEventArgs e) => AddScheduledUpdate(() => AddUser(e.User));
    private void OnUserDisconnected(object sender, UserDisconnectedEventArgs e) => AddScheduledUpdate(() => RemoveUser(e.UserId));
    private void OnUserOnline(object sender, UsersOnlineEventArgs e) => AddScheduledUpdate(() =>
    {
        e.UserIds.ForEach(id =>
        {
            if (OnlineManager.OnlineUsers.TryGetValue(id, out var user))
                AddUser(user);
        });
        LoadingWheel.Visible = false;
    });
    private void OnUserInfoRecieved(object sender, UserInfoEventArgs e) => AddScheduledUpdate(() =>
    {
        // Handle filter for new user connecting
        e.Users.ForEach(onlineUser =>
        {
            if (!OnlineManager.OnlineUsers.TryGetValue(onlineUser.Id, out var user))
                return;
            
            if (MatchUser(user))
                AddUser(user);
            else
                RemoveUser(onlineUser.Id);
        });
        UpdateContent(e.Users);
    });

    private void OnUserStatusReceived(object sender, UserStatusEventArgs e) => AddScheduledUpdate(() =>
    {
        foreach (var row in Pool.OfType<UserRow>())
        {
            if (!row.Item.HasUserInfo || !e.Statuses.TryGetValue(row.Item.OnlineUser.Id, out var status))
                continue;

            row.OnStatusUpdate(status ??
                new UserClientStatus(ClientStatus.InMenus, -1, "", 1, "", 0));
        }
    });

    private void OnUserFriendsListReceived(object sender, UserFriendsListEventArgs e) => AddScheduledUpdate(() =>
    {
        if (Filter.Value != UsersSection.UserFilter.Friends)
            return;

        AvailableItems.ToList().ForEach(userShown =>
        {
            if (!MatchUser(userShown))
                RemoveUser(userShown.OnlineUser.Id);
        });

        e.Friends.ForEach(friendId =>
        {
            if (OnlineManager.OnlineUsers.TryGetValue(friendId, out var friend))
                AddUser(friend);
        });
    });
    private void OnFriendsListUserChanged(object sender, FriendsListUserChangedEventArgs e) => AddScheduledUpdate(() =>
    {
        if (Filter.Value != UsersSection.UserFilter.Friends)
            return;

        if (!OnlineManager.OnlineUsers.TryGetValue(e.UserId, out var user))
            return;

        if (MatchUser(user))
            AddUser(user);
        else
            RemoveUser(e.UserId);

    });
    
    public override void Update(GameTime gameTime)
    {
        InputEnabled = IsHovered();
        base.Update(gameTime);
        
        timeSinceLastInfoRequest += gameTime.ElapsedGameTime.TotalMilliseconds;
        timeSinceLastStatusRequest += gameTime.ElapsedGameTime.TotalMilliseconds;
        
        if (timeSinceLastInfoRequest >= 1000)
        {
            timeSinceLastInfoRequest = 0;

            // Find rows that still needs to be updated
            var candidates = Pool.Select(row => row.Item);
        
            // If a search filter is active, we should look for all rows that matches that search query
            // We also do this when the filter is set to country because some user's info might have not loaded yet
            if (!string.IsNullOrEmpty(SearchQuery.Value) || Filter.Value == UsersSection.UserFilter.Country)
                candidates = OnlineManager.OnlineUsers.Values;
        
            // Take 20 row at most to prevent large update pools
            var ids = candidates.Where(row => !row.HasUserInfo).Take(20).Select(user => user.OnlineUser.Id).ToList();
            if(ids.Count > 0)
                OnlineManager.Client?.RequestUserInfo(ids);   
        }

        // Request user's current statuses
        if (timeSinceLastStatusRequest >= 2000)
        {
            timeSinceLastStatusRequest = 0;
            
            var ids = Pool.Select(row => row.Item.OnlineUser.Id).ToList();
            OnlineManager.Client?.RequestUserStatuses(ids);
        }
        
    }
    
    private void CreateLoadingWheel()
    {
        LoadingWheel = new LoadingWheel
        {
            Parent = this,
            Alignment = Alignment.MidCenter,
            Size = new ScalableVector2(50, 50)
        };
    }
    
    private void AddUser(User user)
    {
        if (!MatchUser(user))
            return;
        
        if (AvailableItems.Find(u => u.OnlineUser.Id == user.OnlineUser.Id) != null)
            return;
        
        AddObjectToBottom(user, false);
        NotifyResultsCountChanged();
    }
    
    private void RemoveUser(int userId)
    {
        var user = AvailableItems.Find(u => u.OnlineUser.Id == userId);
        if (user == null)
            return;
        
        AvailableItems.Remove(user);
        DestroyPool();
        
        // Recalculate container size
        PoolStartingIndex = Math.Clamp(PoolStartingIndex, 0, Math.Max(0, AvailableItems.Count - PoolSize));
        CreatePool();
        
        // Recalculate scroll position
        var minY = Math.Min(0f, Height - ContentContainer.Height);
        var y = Math.Clamp(ContentContainer.Y, minY, 0f);
        ContentContainer.Animations.Clear();
        ContentContainer.Y = y;
        TargetY = y;
        PreviousTargetY = y;

        NotifyResultsCountChanged();
    }

    private void UpdateContent(List<OnlineUser> users)
    {
        users.ForEach(user =>
        {
            var row = Pool.Find(row => row.Item.OnlineUser.Id == user.Id);
            if (row != null)
                row.UpdateContent(row.Item, row.Index);
        });
    }

    public override void Destroy()
    {
        SearchQuery.ValueChanged -= OnSearchChanged;
        Filter.ValueChanged -= OnFilterChanged;
        
        if (OnlineManager.Client != null)
        {
            OnlineManager.Client.OnUserConnected -= OnUserConnected;
            OnlineManager.Client.OnUserDisconnected -= OnUserDisconnected;
            OnlineManager.Client.OnUsersOnline -= OnUserOnline;
            OnlineManager.Client.OnUserInfoReceived -= OnUserInfoRecieved;
            OnlineManager.Client.OnUserStatusReceived -= OnUserStatusReceived;
            OnlineManager.Client.OnUserFriendsListReceived -= OnUserFriendsListReceived;
        }
        OnlineManager.FriendsListUserChanged -= OnFriendsListUserChanged;
        
        base.Destroy();
    }
}
