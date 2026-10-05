using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Quaver.Server.Client;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Objects;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Overlays.V2Hub.Users;
using Quaver.Shared.Online;
using Quaver.Shared.Online.API.User;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Logging;
using Wobble.Scheduling;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Overlays.V2Chatting;

public sealed class ChatUserSearchList : PoolableScrollContainer<User>
{
    private ChatChannel SelectedChannel { get; set; }
    private Bindable<string> SearchQuery { get; }
    private OnlineClient SubscribedClient { get; }
    private TaskHandler<string, List<User>> SearchTask { get; }
    private TaskHandler<int, List<User>> ClanMembersTask { get; }
    private Dictionary<int, List<User>> ClanMembersCache { get; } = new Dictionary<int, List<User>>();
    private int? LoadingClanId { get; set; }

    private double TimeSinceLastInfoRequest { get; set; }
    private double TimeSinceLastStatusRequest { get; set; }
    private float PreviousViewportWidth { get; set; }
    private float PreviousViewportHeight { get; set; }

    public event Action<User> OnRowClicked;

    public ChatUserSearchList(Bindable<ChatChannel> activeChannel, Bindable<string> searchQuery, ScalableVector2 size) : base(new List<User>(), 1, 0, size, size)
    {
        SelectedChannel = activeChannel.Value;
        SearchQuery = searchQuery;
        SubscribedClient = OnlineManager.Client;
        SearchTask = new TaskHandler<string, List<User>>(SearchUsers);
        ClanMembersTask = new TaskHandler<int, List<User>>(GetClanMembers);
        SearchQuery.ValueChanged += OnSearchChanged;
        SearchTask.OnCompleted += OnSearchCompleted;
        ClanMembersTask.OnCompleted += OnClanMembersCompleted;
        OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;

        if (SubscribedClient != null)
        {
            SubscribedClient.OnUserConnected += OnUserConnected;
            SubscribedClient.OnUserDisconnected += OnUserDisconnected;
            SubscribedClient.OnUsersOnline += OnUsersOnline;
            SubscribedClient.OnUserInfoReceived += OnUserInfoReceived;
            SubscribedClient.OnUserStatusReceived += OnUserStatusReceived;
        }

        RefreshViewport();
        RefreshDefaultUsers();
    }

    protected override PoolableSprite<User> CreateObject(User item, int index)
    {
        var row = new UserRow(this, item, index, 0)
        {
            Alpha = 0
        };
        row.OnClick += user => OnRowClicked?.Invoke(user);
        return row;
    }

    public void RefreshViewport(bool hideUntilRebuilt = false)
    {
        var widthChanged = Math.Abs(PreviousViewportWidth - Width) > 0.001f;
        var heightChanged = Math.Abs(PreviousViewportHeight - Height) > 0.001f;
        if (!widthChanged && !heightChanged)
            return;

        PreviousViewportWidth = Width;
        PreviousViewportHeight = Height;

        var desiredPoolSize = Math.Min(50, Math.Max(1, (int)Math.Ceiling(WindowManager.Height / 80f) + 3));
        if (!widthChanged && Pool != null && PoolSize == desiredPoolSize)
        {
            RecalculateContainerHeight();
            return;
        }

        if (hideUntilRebuilt)
            Visible = false;

        if (Pool != null)
            DestroyPool();

        PoolSize = desiredPoolSize;
        PoolStartingIndex = Math.Clamp(PoolStartingIndex, 0, Math.Max(0, AvailableItems.Count - PoolSize));
        CreatePool();

        if (hideUntilRebuilt)
            AddScheduledUpdate(() => Visible = true);
    }

    public override void Update(GameTime gameTime)
    {
        InputEnabled = IsHovered();
        base.Update(gameTime);

        TimeSinceLastInfoRequest += gameTime.ElapsedGameTime.TotalMilliseconds;
        TimeSinceLastStatusRequest += gameTime.ElapsedGameTime.TotalMilliseconds;

        // Get users info of potential query match if needed
        if (string.IsNullOrWhiteSpace(SearchQuery.Value) && TimeSinceLastInfoRequest >= 1000)
        {
            TimeSinceLastInfoRequest = 0;
            IEnumerable<User> candidates = HasAccessByDefault(SelectedChannel) ? AvailableItems : OnlineManager.OnlineUsers.Values;
            var ids = candidates
                .Where(user => !user.HasUserInfo)
                .Take(20)
                .Select(user => user.OnlineUser.Id)
                .ToList();

            if (ids.Count > 0)
                SubscribedClient?.RequestUserInfo(ids);
        }

        // Refresh statuses of displayed users
        if (TimeSinceLastStatusRequest >= 2000)
        {
            TimeSinceLastStatusRequest = 0;
            var ids = AvailableItems
                .Where(user => OnlineManager.OnlineUsers.ContainsKey(user.OnlineUser.Id))
                .Select(user => user.OnlineUser.Id)
                .ToList();
            if (ids.Count > 0)
                SubscribedClient?.RequestUserStatuses(ids);
        }
    }

    private List<User> SearchUsers(string query, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            var users = new APIRequestUserSearch(query).ExecuteRequest();
            token.ThrowIfCancellationRequested();
            return users;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            Logger.Error(e, LogType.Network);
            return new List<User>();
        }
    }

    private List<User> GetClanMembers(int clanId, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            var users = new APIRequestClanMembers(clanId).ExecuteRequest();
            token.ThrowIfCancellationRequested();
            return users;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            Logger.Error(e, LogType.Network);
            return new List<User>();
        }
    }

    private void OnSearchChanged(object sender, BindableValueChangedEventArgs<string> e)
    {
        var query = e.Value?.Trim();

        if (string.IsNullOrEmpty(query))
        {
            SearchTask.Cancel();
            RefreshDefaultUsers();
            return;
        }

        StartSearch(query, 300);
    }

    private void StartSearch(string query, int delay)
    {
        if (TryGetClanIdFromSelectedChannel(out var clanId))
        {
            SearchTask.Cancel();

            if (ClanMembersCache.TryGetValue(clanId, out var members))
            {
                ApplyClanMembers(members, query);
                return;
            }

            ReplaceUsers(new List<User>());

            if (LoadingClanId == clanId && ClanMembersTask.IsRunning)
                return;

            ClanMembersTask.Cancel();
            LoadingClanId = clanId;
            ClanMembersTask.Run(clanId);
            return;
        }

        SearchTask.Cancel();
        ClanMembersTask.Cancel();
        LoadingClanId = null;
        ReplaceUsers(new List<User>());

        SearchTask.Run(query, delay);
    }

    private void OnSearchCompleted(object sender, TaskCompleteEventArgs<string, List<User>> e) => AddScheduledUpdate(() =>
    {
        if (IsDisposed || !string.Equals(e.Input, SearchQuery.Value?.Trim(), StringComparison.Ordinal))
            return;

        var users = e.Result
            .Where(user => user?.OnlineUser != null && !IsSelf(user))
            .Where(CanAccessActiveChannel)
            .OrderByDescending(GetLastSeen)
            .ThenBy(user => user.OnlineUser.Username, StringComparer.OrdinalIgnoreCase)
            .Select(GetOnlineUserIfAvailable)
            .Take(50)
            .ToList();

        ReplaceUsers(users);
    });

    private void OnClanMembersCompleted(object sender, TaskCompleteEventArgs<int, List<User>> e) => AddScheduledUpdate(() =>
    {
        if (IsDisposed)
            return;

        ClanMembersCache[e.Input] = e.Result;
        LoadingClanId = null;

        if (!TryGetClanIdFromSelectedChannel(out var selectedClanId) || selectedClanId != e.Input)
            return;

        ApplyClanMembers(e.Result, SearchQuery.Value?.Trim());
    });

    public void RefreshResult(ChatChannel channel)
    {
        SelectedChannel = channel;

        if (string.IsNullOrWhiteSpace(SearchQuery.Value))
        {
            RefreshDefaultUsers();
            return;
        }

        StartSearch(SearchQuery.Value.Trim(), 0);
    }

    private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e) => AddScheduledUpdate(RefreshCurrentResults);

    private void OnUserConnected(object sender, UserConnectedEventArgs e) => AddScheduledUpdate(RefreshCurrentResults);

    private void OnUserDisconnected(object sender, UserDisconnectedEventArgs e) => AddScheduledUpdate(RefreshCurrentResults);

    private void OnUsersOnline(object sender, UsersOnlineEventArgs e) => AddScheduledUpdate(RefreshCurrentResults);

    private void OnUserInfoReceived(object sender, UserInfoEventArgs e) => AddScheduledUpdate(RefreshCurrentResults);

    private void OnUserStatusReceived(object sender, UserStatusEventArgs e) => AddScheduledUpdate(() =>
    {
        if (IsDisposed || Pool == null)
            return;

        foreach (var row in Pool.OfType<UserRow>())
        {
            if (e.Statuses.TryGetValue(row.Item.OnlineUser.Id, out var status))
                row.OnStatusUpdate(status ?? new UserClientStatus(ClientStatus.InMenus, -1, string.Empty, 1, string.Empty, 0));
        }
    });

    private void RefreshCurrentResults()
    {
        if (IsDisposed)
            return;

        if (string.IsNullOrWhiteSpace(SearchQuery.Value))
        {
            RefreshDefaultUsers(false);
            return;
        }

        var users = AvailableItems
            .Select(GetOnlineUserIfAvailable)
            .Where(CanAccessActiveChannel)
            .OrderByDescending(GetLastSeen)
            .ThenBy(user => user.OnlineUser.Username, StringComparer.OrdinalIgnoreCase)
            .ToList();

        ReplaceUsers(users, false);
    }

    private void RefreshDefaultUsers(bool resetScroll = true)
    {
        SearchTask.Cancel();

        if (!TryGetClanIdFromSelectedChannel(out var clanId))
        {
            ClanMembersTask.Cancel();
            LoadingClanId = null;
            RefreshOnlineUsers(resetScroll);
            return;
        }

        if (ClanMembersCache.TryGetValue(clanId, out var members))
        {
            ApplyClanMembers(members, resetScroll: resetScroll);
            return;
        }

        if (LoadingClanId == clanId && ClanMembersTask.IsRunning)
            return;

        ClanMembersTask.Cancel();
        LoadingClanId = clanId;
        ReplaceUsers(new List<User>());

        ClanMembersTask.Run(clanId);
    }

    private void ApplyClanMembers(IEnumerable<User> members, string query = null, bool resetScroll = true)
    {
        var users = members
            .Where(user => user?.OnlineUser != null && !IsSelf(user))
            .Where(user => string.IsNullOrWhiteSpace(query) || user.OnlineUser.Username.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Where(CanAccessActiveChannel)
            .OrderByDescending(GetLastSeen)
            .ThenBy(user => user.OnlineUser.Username, StringComparer.OrdinalIgnoreCase)
            .Select(GetOnlineUserIfAvailable)
            .Take(50)
            .ToList();

        ReplaceUsers(users, resetScroll);
    }

    private void RefreshOnlineUsers(bool resetScroll = true)
    {
        if (IsDisposed)
            return;

        var users = (OnlineManager.OnlineUsers?.Values ?? Enumerable.Empty<User>())
            .Where(user => user?.OnlineUser != null && !IsSelf(user) && CanAccessActiveChannel(user))
            .OrderBy(user => user.HasUserInfo ? user.OnlineUser.Username : string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(user => user.OnlineUser.Id)
            .Take(50)
            .ToList();

        ReplaceUsers(users, resetScroll);
    }

    private bool CanAccessActiveChannel(User user)
    {
        var channel = SelectedChannel;
        if (channel == null)
            return false;

        if (channel.IsPrivate)
        {
            var directMessageUser = channel.DirectMessageUser?.OnlineUser;
            return string.Equals(channel.Name, user.OnlineUser.Username, StringComparison.OrdinalIgnoreCase);
        }

        if (!user.HasUserInfo)
            return HasAccessByDefault(channel);

        if (channel.Name.StartsWith("#clan_", StringComparison.OrdinalIgnoreCase))
            return string.Equals(user.OnlineUser.ClanId, channel.Name.Substring("#clan_".Length), StringComparison.OrdinalIgnoreCase);

        if (channel.Name.StartsWith("#multiplayer", StringComparison.OrdinalIgnoreCase))
            return OnlineManager.CurrentGame?.PlayerIds?.Contains(user.OnlineUser.Id) == true;

        if (channel.Name.StartsWith("#spectator_", StringComparison.OrdinalIgnoreCase))
        {
            if (!int.TryParse(channel.Name.Substring("#spectator_".Length), out var playerId))
                return false;

            if (user.OnlineUser.Id == playerId)
                return true;

            return playerId == OnlineManager.Self?.OnlineUser?.Id && OnlineManager.Spectators.ContainsKey(user.OnlineUser.Id);
        }

        if (channel.AllowedUserGroups != 0)
            return (user.OnlineUser.UserGroups & channel.AllowedUserGroups) != 0;

        return true;
    }

    private static bool HasAccessByDefault(ChatChannel channel) =>
        channel == null || channel.AllowedUserGroups == 0 || channel.DirectMessageUser?.OnlineUser != null ||
         (!channel.IsPrivate && !string.Equals(channel.Name, "#admin", StringComparison.OrdinalIgnoreCase) && !channel.Name.StartsWith("#clan_", StringComparison.OrdinalIgnoreCase));

    private bool TryGetClanIdFromSelectedChannel(out int clanId)
    {
        clanId = 0;
        return SelectedChannel?.Name?.StartsWith("#clan_", StringComparison.OrdinalIgnoreCase) == true && int.TryParse(SelectedChannel.Name.Substring("#clan_".Length), out clanId);
    }

    private static bool IsSelf(User user) => OnlineManager.Self?.OnlineUser != null && user.OnlineUser.Id == OnlineManager.Self.OnlineUser.Id;

    private static User GetOnlineUserIfAvailable(User user) =>
        OnlineManager.OnlineUsers != null && OnlineManager.OnlineUsers.TryGetValue(user.OnlineUser.Id, out var onlineUser) ? onlineUser : user;

    private static DateTime GetLastSeen(User user)
    {
        if (OnlineManager.OnlineUsers?.ContainsKey(user.OnlineUser.Id) == true)
            return DateTime.MaxValue;

        return user is APIUserSearchResult result ? result.LatestActivity : DateTime.MinValue;
    }

    private void ReplaceUsers(List<User> users, bool resetScroll = true)
    {
        var previousY = ContentContainer.Y;
        var previousTargetY = TargetY;

        DestroyPool();
        AvailableItems = users.Take(50).ToList();
        PoolStartingIndex = 0;
        ContentContainer.Animations.Clear();

        if (!resetScroll)
        {
            CreatePool();

            var minimumY = Math.Min(0, Height - ContentContainer.Height);
            var currentY = Math.Clamp(previousY, minimumY, 0);
            var targetY = Math.Clamp(previousTargetY, minimumY, 0);
            ContentContainer.Y = currentY;
            TargetY = targetY;
            PreviousTargetY = currentY;
            PreviousContentContainerY = currentY;
            HandlePoolShifting();
            return;
        }

        ContentContainer.Y = 0;
        TargetY = 0;
        PreviousTargetY = 0;
        PreviousContentContainerY = 0;
        CreatePool();
    }

    public override void Destroy()
    {
        SearchQuery.ValueChanged -= OnSearchChanged;
        SearchTask.OnCompleted -= OnSearchCompleted;
        SearchTask.Dispose();
        ClanMembersTask.OnCompleted -= OnClanMembersCompleted;
        ClanMembersTask.Dispose();
        OnlineManager.Status.ValueChanged -= OnConnectionStatusChanged;

        if (SubscribedClient != null)
        {
            SubscribedClient.OnUserConnected -= OnUserConnected;
            SubscribedClient.OnUserDisconnected -= OnUserDisconnected;
            SubscribedClient.OnUsersOnline -= OnUsersOnline;
            SubscribedClient.OnUserInfoReceived -= OnUserInfoReceived;
            SubscribedClient.OnUserStatusReceived -= OnUserStatusReceived;
        }

        base.Destroy();
    }
}
