using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Overlays.Hub.OnlineUsers.Filter;
using Quaver.Shared.Graphics.Overlays.Hub.OnlineUsers.Scrolling;
using Quaver.Shared.Helpers;
using Wobble.Graphics;

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

            // Reset the filter panel's parent so the contained dropdown draws on top of the user container.
            FilterPanel.Parent = Container;
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
                new ScalableVector2(Container.Width, Container.Height - FilterPanel.Height))
            {
                Parent = Container,
                Y = FilterPanel.Y + FilterPanel.Height
            };
        }
    }
}