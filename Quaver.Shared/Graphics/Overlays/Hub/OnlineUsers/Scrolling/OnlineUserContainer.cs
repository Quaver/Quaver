using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Client;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Music.UI.ListenerList;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;

namespace Quaver.Shared.Graphics.Overlays.Hub.OnlineUsers.Scrolling
{
    public class OnlineUserContainer : PoolableScrollContainer<User>
    {
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
        private Bindable<string> CurrentSearchQuery { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <param name="size"></param>
        public OnlineUserContainer(Bindable<string> searchQuery, ScalableVector2 size) : base(new List<User>(),
            int.MaxValue, 0, size, size, true)
        {
            CurrentSearchQuery = searchQuery;
            Tint = ColorHelper.HexToColor("#242424");
            Alpha = 1;

            Scrollbar.Width = 4;
            Scrollbar.Tint = Color.White;

            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 220;

            CreatePool(false, false);

            TargetY = 0;
            PreviousTargetY = 0;
            PreviousContentContainerY = 0;
            ContentContainer.Y = 0;

            CreateLoadingWheel();

            SubscribeToEvents();
            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
            CurrentSearchQuery.ValueChanged += OnSearched;
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

            lock (Pool)
            {
                ContainPooledDrawables();
                LoadingWheel.Visible = string.IsNullOrEmpty(CurrentSearchQuery.Value) && (Pool?.Count == 0 || ContentContainer.Children.Count == 0);

                RequestUserInfo(gameTime);
                RequestUserStatuses(gameTime);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            OnlineManager.Status.ValueChanged -= OnConnectionStatusChanged;
            UnsubscribeToEvents();

            base.Destroy();
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

                ScheduleUpdate(() =>
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
            {
                UnsubscribeToEvents();

                ScheduleUpdate(() =>
                {
                    Pool.ForEach(x => x.Destroy());
                    Pool.Clear();

                    AvailableItems.Clear();
                    RecalculateContainerHeight();
                });
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSearched(object sender, BindableValueChangedEventArgs<string> e)
        {
            if (string.IsNullOrEmpty(e.Value))
                AvailableItems = OnlineManager.OnlineUsers.Values.ToList();
            else
            {
                AvailableItems = OnlineManager.OnlineUsers.Values.ToList().FindAll(x =>
                {
                    if (x.OnlineUser.Username == null)
                        return false;

                    return x.OnlineUser.Username.ToLower().Contains(e.Value.ToLower());
                }).ToList();
            }

            TargetY = 0;
            PreviousTargetY = 0;
            PreviousContentContainerY = 0;
            ContentContainer.Y = 0;

            Pool.ForEach(x => x.Destroy());
            Pool.Clear();
            CreatePool(false);
        }

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
        private void SubscribeToEvents()
        {
            if (OnlineManager.Client == null || OnlineManager.Status.Value != ConnectionStatus.Connected)
                return;

            OnlineManager.Client.OnUsersOnline += OnUsersOnline;
            OnlineManager.Client.OnUserConnected += OnUserConnected;
            OnlineManager.Client.OnUserDisconnected += OnUserDisconnected;
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
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUsersOnline(object sender, UsersOnlineEventArgs e)
        {
            lock (AvailableItems)
                AvailableItems = OnlineManager.OnlineUsers.Values.ToList();

            // Create the initial pool, but don't contain the drawable automatically,
            // because this'll be done in Update(), so we can contain/update the correct users.
            Pool?.ForEach(x => x.Destroy());
            Pool?.Clear();
            CreatePool(false, false);

            RecalculateContainerHeight();
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

            lock (AvailableItems)
            {
                if (!AvailableItems.Contains(user))
                    AvailableItems.Add(user);
            }

            lock (Pool)
            {
                if (Pool.Any(x => x.Item == e.User))
                    return;

                AddObjectToBottom(user, false);
            }
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
    }
}