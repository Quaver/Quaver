using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Online.Chat;
using Quaver.Screens.Menu;
using Quaver.Screens.Menu.UI.Navigation;
using Quaver.Screens.Menu.UI.Navigation.User;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Primitives;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Screens.SongSelect
{
    public class SongSelectScreenView : ScreenView
    {
        /// <summary>
        ///     The navigation bar used to go back/open up dialogs.
        /// </summary>
        public Navbar Navbar { get; private set; }

        /// <summary>
        ///     The line on the bottom.
        /// </summary>
        public Line BottomLine { get; private set; }

        /// <summary>
        ///     The user's profile when the click on their name in the navbar.
        /// </summary>
        public UserProfileContainer UserProfile { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public SongSelectScreenView(Screen screen) : base(screen)
        {
            CreateNavbar();
            CreateBottomLine();
            CreateUserProfile();
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
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();

        /// <summary>
        ///     Creates the navbar for this screen.
        /// </summary>
        private void CreateNavbar() => Navbar = new Navbar(new List<NavbarItem>
        {
            new NavbarItem("Home", false, OnHomeButtonClicked),
            new NavbarItem("Play", true),
            new NavbarItem("Download Maps"),
            new NavbarItem("Open Chat", false, (o, e) => ChatManager.ToggleChatOverlay(true))
        }, new List<NavbarItem>
        {
            new NavbarItemUser(this)
        }) { Parent = Container };

        /// <summary>
        ///     Called when the home button is clicked in the navbar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnHomeButtonClicked(object sender, EventArgs e) => QuaverScreenManager.ChangeScreen(new MenuScreen());

        /// <summary>
        ///     Creates the line at the bottom of the screen.
        /// </summary>
        private void CreateBottomLine()
        {
            BottomLine = new Line(Vector2.Zero, Color.LightGray, 2)
            {
                Parent = Container,
                Position = new ScalableVector2(64, WindowManager.Height - 64),
                Alpha = 0.90f
            };

            BottomLine.EndPosition = new Vector2(WindowManager.Width - BottomLine.X, BottomLine.AbsolutePosition.Y);
        }

        /// <summary>
        ///     Creates the user profile container.
        /// </summary>
        private void CreateUserProfile() => UserProfile = new UserProfileContainer(this)
        {
            Parent = Container,
            Alignment = Alignment.TopRight,
            Y = Navbar.Line.Y + Navbar.Line.Thickness,
            X = -64
        };
    }
}