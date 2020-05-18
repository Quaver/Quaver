using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Graphics.Overlays.Hub.SongRequests.Header;
using Quaver.Shared.Graphics.Overlays.Hub.SongRequests.Scrolling;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.Hub.SongRequests
{
    public class OnlineHubSectionSongRequests : OnlineHubSection
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override string Name { get; } = "Song Requests".ToUpper();

        /// <summary>
        /// </summary>
        private OnlineHubSongRequestsHeader Header { get; set; }

        /// <summary>
        /// </summary>
        private SongRequestScrollContainer RequestContainer { get; set; }

        /// <summary>
        /// </summary>
        private IconTextButton ClearButton { get; set; }

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
            CreateHeader();
            CreateRequestContainer();
            CreateClearButton();
        }

        public override void Update(GameTime gameTime)
        {
            RequestContainer.Height = Container.Height - Header.Height;
            RequestContainer.RecalculateContainerHeight();

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateHeader()
        {
            Header = new OnlineHubSongRequestsHeader(new ScalableVector2(Container.Width, 61))
            {
                Parent = Container,
                Tint = ColorHelper.HexToColor("#2a2a2a")
            };
        }

        /// <summary>
        /// </summary>
        private void CreateRequestContainer()
        {
            RequestContainer = new SongRequestScrollContainer(new ScalableVector2(Container.Width, Container.Height - Header.Height))
            {
                Parent = Container,
                Y = Header.Height
            };
        }

        /// <summary>
        /// </summary>
        private void CreateClearButton()
        {
            ClearButton = new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_times),
                FontManager.GetWobbleFont(Fonts.LatoBlack), "Clear All",
                (sender, args) => RequestContainer.RemoveAll(), ColorHelper.HexToColor($"#FF6868"))
            {
                Parent = Container,
                X = -Hub.HeaderText.X,
                Y = -Hub.HeaderBackground.Height * 2 - 2,
                Alignment = Alignment.TopRight,
                Text =
                {
                    Y = 1
                }
            };
        }
    }
}