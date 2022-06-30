using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Server.Client;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Overlays.Hub.OnlineUsers.Filter;
using Quaver.Shared.Graphics.Overlays.Hub.OnlineUsers.Scrolling;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.Hub.OnlineUsers
{
    public class OnlineHubSectionOnlineUsers : OnlineHubSection
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override string Name { get; } = "Online Users".ToUpper();

        /// <summary>
        /// </summary>
        private OnlineHubOnlineUsersFilterPanel FilterPanel { get; set; }

        /// <summary>
        /// </summary>
        private OnlineUserContainer UserContainer { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus UserCount { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="hub"></param>
        public OnlineHubSectionOnlineUsers(OnlineHub hub) : base(hub)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override Texture2D GetIcon() => UserInterface.HubOnlineUsers;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void CreateContent()
        {
            CreateFilterPanel();
            CreateUserContainer();
            CreateUserCount();

            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;

            // Reset the filter panel's parent so the contained dropdown draws on top of the user container.
            FilterPanel.Parent = Container;
        }

        public override void Update(GameTime gameTime)
        {
            UserContainer.Height = Container.Height - FilterPanel.Height;
            UserContainer.RecalculateContainerHeight();

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateFilterPanel()
        {
            FilterPanel = new OnlineHubOnlineUsersFilterPanel(new ScalableVector2(Container.Width, 61))
            {
                Parent = Container,
                Tint = ColorHelper.HexToColor("#2a2a2a")
            };
        }

        /// <summary>
        /// </summary>
        private void CreateUserContainer()
        {
            UserContainer = new OnlineUserContainer(FilterPanel.CurrentSearchQuery,
                new ScalableVector2(Container.Width, Container.Height - FilterPanel.Height), FilterPanel.FilterDropdown)
            {
                Parent = Container,
                Y = FilterPanel.Y + FilterPanel.Height
            };
        }

        /// <summary>
        /// </summary>
        private void CreateUserCount()
        {
            UserCount = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "0 USERS ONLINE", 20)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                X = -Hub.HeaderText.X,
                Y = -Hub.HeaderBackground.Height * 2 - 2
            };

            UserCount.Y += UserCount.Height;
            SetUserCount();
        }

        /// <summary>
        /// </summary>
        private void SetUserCount()
        {
            if (!OnlineManager.Connected)
            {
                UserCount.Text = "OFFLINE";
                return;
            }

            UserCount.ScheduleUpdate(() =>
            {
                var users = OnlineManager.OnlineUsers?.Count;

                if (users == 0 || users > 1)
                    UserCount.Text = $"{users} USERS ONLINE";
                else
                    UserCount.Text = $"{users} USER ONLINE";
            });
        }

        /// <summary>
        /// </summary>
        private void SubscribeToEvents()
        {
            if (OnlineManager.Client == null)
                return;

            OnlineManager.Client.OnUsersOnline += OnUsersOnline;
            OnlineManager.Client.OnUserConnected += OnUserConnected;
            OnlineManager.Client.OnUserDisconnected += OnUserDisconnected;
        }

        /// <summary>
        /// </summary>
        private void UnsubcribeToEvents()
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
        private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
        {
            if (e.Value == ConnectionStatus.Connected)
                SubscribeToEvents();
            else
                UnsubcribeToEvents();

            SetUserCount();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserDisconnected(object sender, UserDisconnectedEventArgs e) => SetUserCount();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserConnected(object sender, UserConnectedEventArgs e) => SetUserCount();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUsersOnline(object sender, UsersOnlineEventArgs e) => SetUserCount();
    }
}