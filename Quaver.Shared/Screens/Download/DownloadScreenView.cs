/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
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
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Menu;
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

        /// <summary>
        /// </summary>
        public MenuHeader Header { get; private set; }

        /// <summary>
        /// </summary>
        public MenuFooter Footer { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public DownloadScreenView(Screen screen) : base(screen)
        {
            CreateBackground();
            CreateMenuHeader();
            CreateMenuFooter();
            CreateDownloadSearchBox();
            CreateDownloadSearchFilter();
            CreateDownloadScrollContainer();
            CreateCurrentlySearchingInterface();
            CreateDownloadStatusBox();
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
        /// </summary>
        private void CreateBackground() => Background = new BackgroundImage(UserInterface.MenuBackgroundNormal, 45)
        {
            Parent = Container,
        };

        /// <summary>
        /// </summary>
        private void CreateMenuHeader()
        {
            Header = new MenuHeader(FontAwesome.Get(FontAwesomeIcon.fa_download_to_storage_drive), "Download", "Maps",
                "Download new maps to play", Colors.MainAccent)
            {
                Parent = Container,
                Alignment = Alignment.TopLeft
            };
        }

        private void CreateMenuFooter()
        {
            Footer = new MenuFooter(new List<ButtonText>()
            {
                new ButtonText(FontsBitmap.GothamRegular, "Back", 14, (sender, args) =>
                {
                    var screen = Screen as DownloadScreen;
                    screen?.Exit(() => new MenuScreen());
                })
            }, new List<ButtonText>(), Colors.MainAccent)
            {
                Parent = Container,
                Alignment = Alignment.BotLeft
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDownloadSearchBox()
        {
            SearchBox = new DownloadSearchBox(this)
            {
                Parent = Container,
                Y = Header.Height + 20,
            };

            SearchBox.X = -SearchBox.Width;
            SearchBox.MoveToX(25, Easing.OutQuint, 600);
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
            Filters.MoveToX(25, Easing.OutQuint, 800);
        }

        /// <summary>
        /// </summary>
        private void CreateDownloadScrollContainer() => ScrollContainer = new DownloadScrollContainer(this)
        {
            Parent = Container,
            Alignment = Alignment.TopRight,
            X = -25,
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
            MapsetInformationBox.MoveToX(25, Easing.OutQuint, 1000);
        }
    }
}
