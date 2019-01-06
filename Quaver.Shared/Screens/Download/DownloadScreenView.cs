/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Server.Client;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Download.UI;
using Quaver.Shared.Screens.Download.UI.Drawable;
using Quaver.Shared.Screens.Download.UI.Filter;
using Quaver.Shared.Screens.Download.UI.Status;
using Quaver.Shared.Screens.Menu;
using Quaver.Shared.Screens.Menu.UI.Navigation;
using Quaver.Shared.Screens.Menu.UI.Navigation.User;
using Quaver.Shared.Screens.Select;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.UI;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.Download
{
    public class DownloadScreenView : ScreenView
    {
        /// <summary>
        ///     The main menu background.
        /// </summary>
        public BackgroundImage Background { get; set; }

        /// <summary>
        /// </summary>
        public Navbar Navbar { get; private set; }

        /// <summary>
        /// </summary>
        public Line BottomLine { get; set; }

        /// <summary>
        /// </summary>
        public DownloadSearchBox SearchBox { get; private set; }

        /// <summary>
        /// </summary>
        public DownloadSearchFilters Filters { get; private set; }

        /// <summary>
        ///     The user's profile when the click on their name in the navbar.
        /// </summary>
        public UserProfileContainer UserProfile { get; set; }

        /// <summary>
        /// </summary>
        public DownloadScrollContainer ScrollContainer { get; private set; }

        /// <summary>
        /// </summary>
        public CurrentlySearchingInterface CurrentlySearching { get; private set; }

        /// <summary>
        /// </summary>
        public MapsetInformation MapsetInformationBox { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public DownloadScreenView(Screen screen) : base(screen)
        {
            CreateBackground();
            CreateNavbar();
            CreateBottomLine();
            CreateDownloadSearchBox();
            CreateDownloadSearchFilter();
            CreateDownloadScrollContainer();
            CreateCurrentlySearchingInterface();
            CreateDownloadStatusBox();
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
            GameBase.Game.GraphicsDevice.Clear(Color.Black);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();

        /// <summary>
        ///    Creates the navigation bar for the screen.
        /// </summary>
        private void CreateNavbar() => Navbar = new NavbarMain((QuaverScreen) Screen, new List<NavbarItem>
            {
                new NavbarItem(UserInterface.QuaverLogoFull, false, (o, e) => BrowserHelper.OpenURL(OnlineClient.WEBSITE_URL), false),
                new NavbarItem("Home", false, OnHomeButtonClicked),
            }, new List<NavbarItem>
            {
                new NavbarItemUser(this),
                new NavbarItem("Report Bugs", false, (o, e) => BrowserHelper.OpenURL("https://github.com/Quaver/Quaver/issues")),
            })
            { Parent = Container };

        /// <summary>
        ///     Called when the home button is clicked in the navbar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHomeButtonClicked(object sender, EventArgs e)
        {
            var screen = Screen as DownloadScreen;

            screen?.Exit(() =>
            {
                if (AudioEngine.Track != null)
                {
                    lock (AudioEngine.Track)
                        AudioEngine.Track?.Fade(10, 300);
                }

                return new MenuScreen();
            });
        }

        /// <summary>
        ///     Creates the top and bottom lines.
        /// </summary>
        private void CreateBottomLine()
        {
            BottomLine = new Line(Vector2.Zero, Color.LightGray, 2)
            {
                Parent = Container,
                Position = new ScalableVector2(28, WindowManager.Height - 54),
                Alpha = 0.90f
            };

            BottomLine.EndPosition = new Vector2(WindowManager.Width - BottomLine.X, BottomLine.AbsolutePosition.Y);
        }

        /// <summary>
        ///     Creates the container for user profiles.
        /// </summary>
        private void CreateUserProfile() => UserProfile = new UserProfileContainer(this)
        {
            Parent = Container,
            Alignment = Alignment.TopRight,
            Y = Navbar.Line.Y + Navbar.Line.Thickness,
            X = -28
        };

        /// <summary>
        /// </summary>
        private void CreateBackground() => Background = new BackgroundImage(UserInterface.MenuBackground, 20)
        {
            Parent = Container,
        };

        /// <summary>
        /// </summary>
        private void CreateDownloadSearchBox()
        {
            SearchBox = new DownloadSearchBox(this)
            {
                Parent = Container,
                Y = Navbar.Line.Y + Navbar.Line.Thickness + 20,
            };

            SearchBox.X = -SearchBox.Width;
            SearchBox.MoveToX(Navbar.Line.X, Easing.OutQuint, 600);
        }
        /// <summary>
        /// </summary>
        private void CreateDownloadSearchFilter()
        {
            Filters = new DownloadSearchFilters(this)
            {
                Parent = Container,
                Y = SearchBox.Y + SearchBox.Height + 20,
                X = SearchBox.X
            };

            Filters.X = -Filters.Width;
            Filters.MoveToX(Navbar.Line.X, Easing.OutQuint, 800);
        }

        /// <summary>
        /// </summary>
        private void CreateDownloadScrollContainer() => ScrollContainer = new DownloadScrollContainer(this)
        {
            Parent = Container,
            Alignment = Alignment.TopRight,
            X = -Navbar.Line.X,
            Y = SearchBox.Y
        };

        /// <summary>
        /// </summary>
        private void CreateCurrentlySearchingInterface() => CurrentlySearching = new CurrentlySearchingInterface(this, ScrollContainer)
        {
            Parent = ScrollContainer,
            Alignment = Alignment.MidCenter
        };

        /// <summary>
        /// </summary>
        private void CreateDownloadStatusBox()
        {
            MapsetInformationBox = new MapsetInformation(this)
            {
                Parent = Container,
                Y = Filters.Y + Filters.Height + 20
            };

            MapsetInformationBox.X = -MapsetInformationBox.Width;
            MapsetInformationBox.MoveToX(Navbar.Line.X, Easing.OutQuint, 1000);
        }
    }
}