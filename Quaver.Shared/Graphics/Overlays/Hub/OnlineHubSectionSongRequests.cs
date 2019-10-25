using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;

namespace Quaver.Shared.Graphics.Overlays.Hub
{
    public class OnlineHubSectionSongRequests : OnlineHubSection
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override string Name { get; } = "Song Requests".ToUpper();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="hub"></param>
        public OnlineHubSectionSongRequests(OnlineHub hub) : base(hub)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override Texture2D GetIcon() => UserInterface.HubSongRequests;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void CreateContent()
        {
            CreateNotImplementedText();
        }
    }
}