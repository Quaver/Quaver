using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Client;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Graphics.Overlays.Hub.OnlineUsers.Filter;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Music.UI.ListenerList;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Overlays.Hub.OnlineUsers.Scrolling
{
    public class OnlineUserContainer : PoolableScrollContainer<User>
    {
        /// <summary>
        /// </summary>
        public OnlineHubOnlineUsersFilterDropdown FilterDropdown { get; }

        /// <summary>
        ///     The currently active right click options for the screen
        /// </summary>
        public RightClickOptions ActiveRightClickOptions { get; private set; }

        /// <summary>
        /// </summary>
        private double TimeSinceLastRequestedStatuses { get; set; }

        /// <summary>
        /// </summary>
        private double TimeSinceLastRequestedUserInfo { get; set; }

        /// <summary>
        ///     The amount of items max that are drawn/displayed at a time
        /// </summary>
        private const int MAX_DISPLAYED_ITEMS = 15;

        /// <summary>
        ///     Returns the scroll/"pool" starting index based on the container's scroll position
        /// </summary>
        private int ScrollIndex => Pool.Count == 0 ? 0 : (int) Math.Round(Math.Abs(ContentContainer.Y / Pool.First().Height));

        /// <summary>
        /// </summary>
        private LoadingWheel LoadingWheel { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus OfflineNotice { get; set; }

        /// <summary>
        /// </summary>
        private Bindable<string> CurrentSearchQuery { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <param name="size"></param>
        /// <param name="filterDropdown"></param>
        public OnlineUserContainer(Bindable<string> searchQuery, ScalableVector2 size, OnlineHubOnlineUsersFilterDropdown filterDropdown)
            : base(new List<User>(), int.MaxValue, 0, size, size, true)
        {
            FilterDropdown = filterDropdown;
            CurrentSearchQuery = searchQuery;
            Tint = ColorHelper.HexToColor("#242424");
            Alpha = 1;

            Scrollbar.Width = 4;
            Scrollbar.Tint = Color.White;

            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 220;

            CreateLoadingWheel();
            CreateOfflineNotice();

            SubscribeToEvents();
            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
            OnlineManager.FriendsListUserChanged += OnFriendsListUserChanged;
            CurrentSearchQuery.ValueChanged += OnSearched;

            if (ConfigManager.OnlineUserListFilterType != null)
                ConfigManager.OnlineUserListFilterType.ValueChanged += OnFilterChanged;

            AddScheduledUpdate(() =>
            {
                CreatePool(false, false);
                ResetPool();
            });
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt);

            // Manually run scheduled updates here to get rid of flashing when resetting the pool
            // under a different thread.
            RunScheduledUpdates();

            ContainPooledDrawables();

            LoadingWheel.Visible = string.IsNullOrEmpty(CurrentSearchQuery.Value)
                                   && (Pool?.Count == 0 || ContentContainer.Children.Count == 0)
                                   && OnlineManager.Status.Value != ConnectionStatus.Disconnected;

            OfflineNotice.Visible = OnlineManager.Status.Value == ConnectionStatus.Disconnected;

            RequestUserInfo(gameTime);
            RequestUserStatuses(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            OnlineManager.Status.ValueChanged -= OnConnectionStatusChanged;
            OnlineManager.FriendsListUserChanged -= OnFriendsListUserChanged;

            UnsubscribeToEvents();

            if (ConfigManager.OnlineUserListFilterType != null)
            {
                // ReSharper disable once DelegateSubtraction
                ConfigManager.OnlineUserListFilterType.ValueChanged -= OnFilterChanged;
            }

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="user"></param>
        private void AddUser(User user)
        {
            if (!UserMeetsFilter(user))
                return;

            lock (AvailableItems)
            {
                if (!AvailableItems.Contains(user))
                    AvailableItems.Add(user);
            }

            lock (Pool)
            {
                if (Pool.Any(x => x.Item == user))
                    return;

                AddContainedDrawable(AddObject(AvailableItems.IndexOf(user)));
                RecalculateContainerHeight();
            }
        }

        /// <summary>
        ///     Removes a user from the list
        /// </summary>
        /// <param name="user"></param>
        public void RemoveUser(User user)
        {
            lock (AvailableItems)
            lock (Pool)
            {
                var item = Pool.Find(x => x.Item == user);
                AvailableItems.Remove(user);

                // Remove the item if it exists in the pool.
                if (item != null)
                {
                    item.Destroy();
                    RemoveContainedDrawable(item);
                    Pool.Remove(item);
                }

                RecalculateContainerHeight();

                AddScheduledUpdate(() =>
                {
                    // Reset the pool item index
                    for (var i = 0; i < Pool.Count; i++)
                    {
                        Pool[i].Index = i;
                        Pool[i].ClearAnimations();
                        Pool[i].Y = (PoolStartingIndex + i) * DrawableListener.ItemHeight;
                        Pool[i].UpdateContent(Pool[i].Item, i);
                    }
                });
            }
        }

        /// <summary>
        ///     Handles containing objects that are in view of the container.
        /// </summary>
        private void ContainPooledDrawables()
        {
            if (Pool.Count == 0 || Pool.Count != AvailableItems.Count)
                return;

            for (var i = ScrollIndex; i < ScrollIndex + MAX_DISPLAYED_ITEMS; i++)
            {
                if (i >= Pool.Count)
                    break;

                if (Pool[i].Parent != ContentContainer)
                    AddContainedDrawable(Pool[i]);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void RequestUserInfo(GameTime gameTime)
        {
            TimeSinceLastRequestedUserInfo += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (TimeSinceLastRequestedUserInfo < 250 || Pool.Count == 0)
                return;

            TimeSinceLastRequestedUserInfo = 0;

            var users = new List<int>();

            for (var i = ScrollIndex; i < ScrollIndex + MAX_DISPLAYED_ITEMS; i++)
            {
                if (i >= Pool.Count)
                    break;

                if (!Pool[i].Item.HasUserInfo)
                    users.Add(Pool[i].Item.OnlineUser.Id);
            }

            if (users.Count == 0)
                return;

            OnlineManager.Client?.RequestUserInfo(users);
        }

        /// <summary>
        ///     Periodically requests new user statuses
        /// </summary>
        /// <param name="gameTime"></param>
        private void RequestUserStatuses(GameTime gameTime)
        {
            TimeSinceLastRequestedStatuses += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (TimeSinceLastRequestedStatuses < 2000)
                return;

            TimeSinceLastRequestedStatuses = 0;

            if (Pool.Count == 0)
                return;

            var users = new List<int>();

            for (var i = ScrollIndex; i < ScrollIndex + MAX_DISPLAYED_ITEMS; i++)
            {
                if (i >= Pool.Count)
                    break;

                if (Pool[i].Item.HasUserInfo)
                    users.Add(Pool[i].Item.OnlineUser.Id);
            }

            if (users.Count == 0)
                return;

            OnlineManager.Client?.RequestUserStatuses(users);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
        {
            if (e.Value == ConnectionStatus.Connected)
                SubscribeToEvents();
            else
                UnsubscribeToEvents();

            ResetPool();
        }

        /// <summary>
        ///     Completely resets and refreshes the object pool
        /// </summary>
        private void ResetPool()
        {
            lock (AvailableItems)
                AvailableItems = FilterUsers();

            AddScheduledUpdate(() =>
            {
                TargetY = 0;
                PreviousTargetY = 0;
                PreviousContentContainerY = 0;
                ContentContainer.Y = 0;

                Pool.ForEach(x => x.Destroy());
                Pool.Clear();
                CreatePool(false);
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSearched(object sender, BindableValueChangedEventArgs<string> e) => ResetPool();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<User> CreateObject(User item, int index)
            => new DrawableOnlineUser(this, item, index);

        /// <summary>
        ///
        /// </summary>
        private void CreateLoadingWheel()
        {
            LoadingWheel = new LoadingWheel
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(50, 50)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateOfflineNotice()
        {
            OfflineNotice = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 20)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                TextAlignment = TextAlignment.Center
            };

            AddScheduledUpdate(() => OfflineNotice.Text = "You must be logged in to \nview the online user list!".ToUpper());
        }

        /// <summary>
        /// </summary>
        private void SubscribeToEvents()
        {
            if (OnlineManager.Client == null || OnlineManager.Status.Value != ConnectionStatus.Connected)
                return;

            OnlineManager.Client.OnUsersOnline += OnUsersOnline;
            OnlineManager.Client.OnUserConnected += OnUserConnected;
            OnlineManager.Client.OnUserDisconnected += OnUserDisconnected;
            OnlineManager.Client.OnUserFriendsListReceived += OnFriendsListReceived;
        }

        /// <summary>
        /// </summary>
        private void UnsubscribeToEvents()
        {
            if (OnlineManager.Client == null)
                return;

            OnlineManager.Client.OnUsersOnline -= OnUsersOnline;
            OnlineManager.Client.OnUserConnected -= OnUserConnected;
            OnlineManager.Client.OnUserDisconnected -= OnUserDisconnected;
            OnlineManager.Client.OnUserFriendsListReceived -= OnFriendsListReceived;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUsersOnline(object sender, UsersOnlineEventArgs e)
        {
            ResetPool();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserConnected(object sender, UserConnectedEventArgs e)
        {
            if (e.User.OnlineUser.Id == OnlineManager.Self.OnlineUser.Id)
                return;

            var user = e.User;
            AddUser(user);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnUserDisconnected(object sender, UserDisconnectedEventArgs e)
        {
            var user = Pool.Find(x => x.Item.OnlineUser.Id == e.UserId);

            if (user == null)
                return;

            RemoveUser(user.Item);
        }

        private void OnFriendsListReceived(object sender, UserFriendsListEventArgs e) => ResetPool();

        /// <summary>
        ///     Returns a list of filtered users
        /// </summary>
        /// <returns></returns>
        private List<User> FilterUsers()
        {
            lock (AvailableItems)
            {
                if (!OnlineManager.Connected)
                    return new List<User>();

                var users = OnlineManager.OnlineUsers.Values.ToList();

                if (ConfigManager.OnlineUserListFilterType != null)
                {
                    switch (ConfigManager.OnlineUserListFilterType.Value)
                    {
                        case OnlineUserListFilter.All:
                            break;
                        case OnlineUserListFilter.Friends:
                            users = users.FindAll(x => OnlineManager.FriendsList.Contains(x.OnlineUser.Id));
                            break;
                        case OnlineUserListFilter.Country:
                            users = users.FindAll(x => x.OnlineUser.CountryFlag == OnlineManager.Self.OnlineUser.CountryFlag);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                // Filter by searched value
                if (!string.IsNullOrEmpty(CurrentSearchQuery.Value))
                {
                    users = users.FindAll(x => x.OnlineUser.Username != null
                                               && x.OnlineUser.Username.ToLower().Contains(CurrentSearchQuery.Value.ToLower())).ToList();
                }

                return users;
            }
        }

        /// <summary>
        ///     Returns if a user meets the filter requirements
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private bool UserMeetsFilter(User user)
        {
            if (ConfigManager.OnlineUserListFilterType != null)
            {
                switch (ConfigManager.OnlineUserListFilterType.Value)
                {
                    case OnlineUserListFilter.All:
                        break;
                    // Check if the user is on the friends list.
                    case OnlineUserListFilter.Friends:
                        if (!OnlineManager.FriendsList.Contains(user.OnlineUser.Id))
                            return false;
                        break;
                    // Check if the user is in the same country
                    case OnlineUserListFilter.Country:
                        if (user.OnlineUser.CountryFlag != OnlineManager.Self.OnlineUser.CountryFlag)
                            return false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // No search query, so we can assume the user fits the criteria
            if (string.IsNullOrEmpty(user.OnlineUser.Username))
                return true;

            // Check if user meets search filter
            return user.OnlineUser.Username.ToLower().Contains(CurrentSearchQuery.Value.ToLower());
        }

        /// <summary>
        /// </summary>
        /// <param name="rco"></param>
        public void ActivateRightClickOptions(RightClickOptions rco)
        {
            if (ActiveRightClickOptions != null)
            {
                ActiveRightClickOptions.Visible = false;
                ActiveRightClickOptions.Parent = null;
                ActiveRightClickOptions.Destroy();
            }

            ActiveRightClickOptions = rco;
            ActiveRightClickOptions.Parent = this;

            ActiveRightClickOptions.ItemContainer.Height = 0;
            ActiveRightClickOptions.Visible = true;

            var x = MathHelper.Clamp(MouseManager.CurrentState.X - ActiveRightClickOptions.Width - AbsolutePosition.X, 0,
                Width - ActiveRightClickOptions.Width);

            var y = MathHelper.Clamp(MouseManager.CurrentState.Y - AbsolutePosition.Y, 0,
                Height - ActiveRightClickOptions.Items.Count * ActiveRightClickOptions.Items.First().Height - 60);

            ActiveRightClickOptions.Position = new ScalableVector2(x, y);
            ActiveRightClickOptions.Open(350);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFilterChanged(object sender, BindableValueChangedEventArgs<OnlineUserListFilter> e) => ResetPool();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnFriendsListUserChanged(object sender, FriendsListUserChangedEventArgs e)
        {
            var user = Pool.Find(x => x.Item.OnlineUser.Id == e.UserId);

            if (user == null)
                return;

            switch (e.Action)
            {
                case FriendsListAction.Add:
                    AddUser(user.Item);
                    break;
                case FriendsListAction.Remove:
                    if (ConfigManager.OnlineUserListFilterType?.Value == OnlineUserListFilter.Friends)
                        RemoveUser(user.Item);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}