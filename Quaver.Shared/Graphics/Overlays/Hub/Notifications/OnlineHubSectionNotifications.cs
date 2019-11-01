using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.Hub.Notifications
{
    public class OnlineHubSectionNotifications : OnlineHubSection
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override string Name { get; } = "Notifications".ToUpper();

        /// <summary>
        /// </summary>
        private NotificationScrollContainer ScrollContainer { get; set; }

        /// <summary>
        /// </summary>
        private IconTextButton ClearButton { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="hub"></param>
        public OnlineHubSectionNotifications(OnlineHub hub) : base(hub)
        {
        }

        public override Texture2D GetIcon() => UserInterface.HubNotifications;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void CreateContent()
        {
            ScrollContainer = new NotificationScrollContainer(new ScalableVector2(Container.Width, Container.Height))
            {
                Parent = Container
            };

            ClearButton = new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_times),
                FontManager.GetWobbleFont(Fonts.LatoBlack), "Clear All",
                (sender, args) => ScrollContainer.ClearAll(), ColorHelper.HexToColor($"#FF6868"))
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

            ClearButton.Y += ClearButton.Height + 6;
        }
    }
}