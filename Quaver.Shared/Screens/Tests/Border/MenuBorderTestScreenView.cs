using Microsoft.Xna.Framework;
using Quaver.Server.Client.Structures;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;
using Quaver.Shared.Graphics.Playercards;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Tests.UI.Borders;
using Wobble;
using Wobble.Graphics;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Border
{
    public class MenuBorderTestScreenView : ScreenView
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public MenuBorderTestScreenView(Screen screen) : base(screen)
        {
            // ReSharper disable twice ObjectCreationAsStatement
            new TestMenuBorderHeader { Parent = Container };
            new TestMenuBorderFooter
            {
                Parent = Container,
                Alignment = Alignment.BotLeft
            };

            new UserPlayercardLoggedOut()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
            };

            new UserPlayercard(new User(new OnlineUser()
            {
                Id = 0,
                CountryFlag = "CA",
                SteamId = 0,
                UserGroups = UserGroups.Normal,
                Username = "TestUser27"
            })
            {
                CurrentStatus = new UserClientStatus(ClientStatus.Paused, -1, "", 1, "", 0)
            })
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                X = 400
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#000000"));
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Container?.Destroy();
        }
    }
}
