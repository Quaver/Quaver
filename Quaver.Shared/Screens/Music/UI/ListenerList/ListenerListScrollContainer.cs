using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.Server.Client;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Online;
using Quaver.Shared.Online.API.Leaderboard;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using TagLib.Ape;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Music.UI.ListenerList
{
    public class ListenerListScrollContainer : PoolableScrollContainer<OnlineUser>
    {
        /// <summary>
        /// </summary>
        private SpriteTextPlus Status { get; set; }

        /// <summary>
        /// </summary>
        private LoadingWheel LoadingWheel { get; set; }

        /// <summary>
        /// </summary>
        private IconButton LoginButton { get; set; }

        /// <summary>
        /// </summary>
        private string StatusText => "Please log in to start a\nlistening party!".ToUpper();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public ListenerListScrollContainer(ScalableVector2 size) : base(OnlineManager.ListeningParty?.Listeners ?? new List<OnlineUser>(),
            int.MaxValue, 0, size, size)
        {
            Alpha = 0;

            Scrollbar.Tint = Color.White;
            Scrollbar.Width = 4;

            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 220;

            CreateStatusText();
            CreateLoadingWheel();
            CreateLoginButton();

            CreatePool();
            SubscribeToEvents();

            SetStatusTextAndAnimations();
            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && DialogManager.Dialogs.Count == 0
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt);

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
        ///     Adds an item to the list
        /// </summary>
        /// <param name="user"></param>
        public void AddUser(OnlineUser user)
        {
            lock (AvailableItems)
            lock (Pool)
            {
                AddObjectToBottom(user, false);
            }
        }

        /// <summary>
        ///     Removes a user from the list
        /// </summary>
        /// <param name="user"></param>
        public void RemoveUser(OnlineUser user)
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

                // Reset the pool item index
                for (var i = 0; i < Pool.Count; i++)
                {
                    Pool[i].Index = i;

                    Pool[i].ClearAnimations();
                    Pool[i].MoveToY((PoolStartingIndex + i) * DrawableListener.ItemHeight, Easing.OutQuint, 400);
                    Pool[i].UpdateContent(Pool[i].Item, i);
                }

                RecalculateContainerHeight();
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<OnlineUser> CreateObject(OnlineUser item, int index)
            => new DrawableListener(this, item, index);

        /// <summary>
        /// </summary>
        private void CreateStatusText()
        {
            Status = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), StatusText, 20)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                TextAlignment = TextAlignment.Center,
                Alpha = 0
            };
        }

        /// <summary>
        /// </summary>
        private void CreateLoadingWheel()
        {
            LoadingWheel = new LoadingWheel
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(50, 50),
                Alpha = 0
            };
        }

        /// <summary>
        /// </summary>
        private void CreateLoginButton()
        {
            LoginButton = new IconButton(UserInterface.LoginButton, (sender, args) =>
            {
                OnlineManager.Login();
            })
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Y = Status.Y + Status.Height + 12,
                Size = new ScalableVector2(221, 41),
                Alpha = 0
            };
        }

        /// <summary>
        /// </summary>
        private void SetStatusTextAndAnimations()
        {
            string text;

            const int time = 150;

            switch (OnlineManager.Status.Value)
            {
                case ConnectionStatus.Disconnected:
                    text = StatusText;
                    LoginButton.IsPerformingFadeAnimations = true;

                    Status.ClearAnimations();
                    Status.FadeTo(1, Easing.Linear, time);

                    LoginButton.ClearAnimations();
                    LoginButton.FadeTo(1, Easing.Linear, time);

                    LoadingWheel.ClearAnimations();
                    LoadingWheel.FadeTo(0, Easing.Linear, time);
                    break;
                case ConnectionStatus.Connecting:
                case ConnectionStatus.Reconnecting:
                    text = "";
                    LoginButton.IsPerformingFadeAnimations = false;

                    Status.ClearAnimations();
                    Status.FadeTo(0, Easing.Linear, time);

                    LoginButton.ClearAnimations();
                    LoginButton.FadeTo(0, Easing.Linear, time);

                    LoadingWheel.ClearAnimations();
                    LoadingWheel.FadeTo(1, Easing.Linear, time);
                    break;
                case ConnectionStatus.Connected:
                    text = "";
                    LoginButton.IsPerformingFadeAnimations = false;

                    Status.ClearAnimations();
                    Status.FadeTo(0, Easing.Linear, time);

                    LoginButton.ClearAnimations();
                    LoginButton.FadeTo(0, Easing.Linear, time);

                    LoadingWheel.ClearAnimations();
                    LoadingWheel.FadeTo(0, Easing.Linear, time);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Status.ScheduleUpdate(() => Status.Text = text);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<OnlineUser> CreateTestUsers()
        {
            var leaderboard = new APIRequestLeaderboard(GameMode.Keys4).ExecuteRequest();

            var users = new List<OnlineUser>();

            for (var i = 0; i < leaderboard.Users.Count; i++)
            {
                var user = leaderboard.Users[i];

                users.Add(new OnlineUser
                {
                    Id = user.Id,
                    SteamId = user.SteamId,
                    Username =  user.Username,
                    UserGroups = user.UserGroups,
                    CountryFlag = user.Country
                });
            }

            return users;
        }

        /// <summary>
        /// </summary>
        private void SubscribeToEvents()
        {
            if (OnlineManager.Client == null)
                return;

            OnlineManager.Client.OnListeningPartyFellowJoined += OnListeningPartyFellowJoined;
            OnlineManager.Client.OnListeningPartyFellowLeft += OnListeningPartyFellowLeft;
        }

        /// <summary>
        /// </summary>
        private void UnsubscribeToEvents()
        {
            if (OnlineManager.Client == null)
                return;

            OnlineManager.Client.OnListeningPartyFellowJoined -= OnListeningPartyFellowJoined;
            OnlineManager.Client.OnListeningPartyFellowLeft -= OnListeningPartyFellowLeft;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListeningPartyFellowJoined(object sender, ListeningPartyFellowJoinedEventArgs e)
            => ScheduleUpdate(() => AddUser(OnlineManager.OnlineUsers[e.UserId].OnlineUser));

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListeningPartyFellowLeft(object sender, ListeningPartyFellowLeftEventArgs e)
            => ScheduleUpdate(() => RemoveUser(OnlineManager.OnlineUsers[e.UserId].OnlineUser));

        /// <summary>
        ///     When the connection status changes, reset the state of the listener list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
        {
            AvailableItems.Clear();

            // Remove all the players upon change in the next frame
            ScheduleUpdate(() =>
            {
                Pool.ForEach(x => x.Destroy());
                Pool.Clear();
                RecalculateContainerHeight();
            });

            // We've reconnected, so re-subscribe to events
            if (e.Value == ConnectionStatus.Connected)
                SubscribeToEvents();

            SetStatusTextAndAnimations();
        }
    }
}