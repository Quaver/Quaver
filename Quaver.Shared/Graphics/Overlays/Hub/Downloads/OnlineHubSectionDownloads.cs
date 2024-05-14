using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Overlays.Hub.Downloads.Scrolling;
using Wobble.Graphics;

namespace Quaver.Shared.Graphics.Overlays.Hub.Downloads
{
    public class OnlineHubSectionDownloads : OnlineHubSection
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override string Name { get; } = "ACTIVE DOWNLOADS";

        /// <summary>
        /// </summary>
        private DownloadScrollContainer ScrollContainer { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="hub"></param>
        public OnlineHubSectionDownloads(OnlineHub hub) : base(hub)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override Texture2D GetIcon() => UserInterface.HubDownloads;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void CreateContent()
        {
            ScrollContainer = new DownloadScrollContainer(new ScalableVector2(Container.Width, Container.Height))
            {
                Parent = Container
            };
        }

        public override void Update(GameTime gameTime)
        {
            ScrollContainer.Height = Container.Height;
            ScrollContainer.RecalculateContainerHeight();
            base.Update(gameTime);
        }
    }
}