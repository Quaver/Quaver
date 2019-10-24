using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;

namespace Quaver.Shared.Graphics.Overlays.Hub
{
    public class OnlineHubSectionOnlineUsers : OnlineHubSection
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override string Name { get; } = "Online Users".ToUpper();

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
            CreateNotImplementedText();
        }
    }
}