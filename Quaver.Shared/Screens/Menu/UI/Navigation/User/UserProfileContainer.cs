/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Select;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Menu.UI.Navigation.User
{
    public class UserProfileContainer : ScrollContainer
    {
        /// <summary>
        ///     Reference to the parent menu screern view.
        /// </summary>
        private ScreenView View { get; }

        /// <summary>
        ///     The original width of the profile container.
        /// </summary>
        private const int OriginalWidth = 450;

        /// <summary>
        ///     The original height of the profile container.
        /// </summary>
        private const int OriginalHeight = 135;

        /// <summary>
        ///     The container for the user profile.
        ///
        ///     Note: The container itself is a button to prevent clicking on objects under.
        /// </summary>
        private ImageButton Container { get; set; }

        /// <summary>
        ///     Reference to the navbar button.
        /// </summary>
        private NavbarItemUser NavbarButton { get; }

        /// <summary>
        ///     The line at the bottom of the container.
        /// </summary>
        private Sprite BottomLine { get; set; }

        /// <summary>
        ///     Displays the current connection status.
        /// </summary>
        private SpriteText TextConnectionStatus { get; set; }

        /// <summary>
        ///     The button to log in and out of the server.
        /// </summary>
        private BorderedTextButton LoginButton { get; set; }

        /// <summary>
        ///     The button to view the user's profile.
        /// </summary>
        private BorderedTextButton ViewProfileButton { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="view"></param>
        public UserProfileContainer(ScreenView view) : base(new ScalableVector2(OriginalWidth, 0),
            new ScalableVector2(OriginalWidth, OriginalHeight))
        {
            View = view;

            Tint = Color.Black;
            Alpha = 0.80f;
            Scrollbar.Visible = false;

            switch (View)
            {
                case MenuScreenView menuView:
                    NavbarButton = menuView?.Navbar.RightAlignedItems.First() as NavbarItemUser;
                    break;
                case SelectScreenView selectView:
                    NavbarButton = selectView?.Navbar.RightAlignedItems.First() as NavbarItemUser;
                    break;
                case DownloadScreenView downloadScreenView:
                    NavbarButton = downloadScreenView?.Navbar.RightAlignedItems.First() as NavbarItemUser;
                    break;
            }

            if (NavbarButton == null)
                throw new InvalidOperationException("Tried to get NavbarItemUser, but it's null!");

            CreateContainer();
            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Container.IsClickable = NavbarButton.Selected;
            ViewProfileButton.IsClickable = NavbarButton.Selected;
            LoginButton.IsClickable = NavbarButton.Selected;
            BottomLine.Visible = NavbarButton.Selected;
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            OnlineManager.Status.ValueChanged -= OnConnectionStatusChanged;

            base.Destroy();
        }

        /// <summary>
        ///     When the navbar button is clicked, this will
        /// </summary>
        public void PerformClickAnimation(bool selected)
        {
            lock (Animations)
            {
                Animations.Clear();

                var targetHeight = selected ? OriginalHeight : 0;
                Animations.Add(new Animation(AnimationProperty.Height, Easing.OutQuint, Height, targetHeight, 500));
            }
        }

        /// <summary>
        ///     Creates the container where everything will live.
        /// </summary>
        private void CreateContainer()
        {
            Container = new ImageButton(UserInterface.BlankBox)
            {
                Size = ContentContainer.Size,
                Alpha = 0
            };

            Container.ClickedOutside += (sender, args) =>
            {
                // User clicked the navbar button, that handles closing automatically.
                if (NavbarButton.IsHovered)
                    return;

                NavbarButton.Selected = false;
                PerformClickAnimation(false);
            };

            BottomLine = new Sprite
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(OriginalWidth, 2),
                Tint = Color.White,
                Visible = false
            };

            TextConnectionStatus = new SpriteText(Fonts.Exo2SemiBold, " ", 14)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = 20,
            };

            UpdateConnectionStatus();

            LoginButton = new BorderedTextButton(" ", Color.Crimson, OnLoginButtonClicked)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = TextConnectionStatus.Y + TextConnectionStatus.Height + 20,
                X = 100,
                Text =
                {
                    FontSize = 13,
                    Font = Fonts.SourceSansProSemiBold
                }
            };

            ViewProfileButton = new BorderedTextButton("View Profile", Colors.MainAccent, OnViewProfileButtonClicked)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = TextConnectionStatus.Y + TextConnectionStatus.Height + 20,
                X = -100,
                SetChildrenVisibility = true,
                Text =
                {
                    FontSize = 13,
                    Font = Fonts.SourceSansProSemiBold
                }
            };

            UpdateButtons();

            AddContainedDrawable(Container);
            AddContainedDrawable(TextConnectionStatus);
            AddContainedDrawable(LoginButton);
            AddContainedDrawable(ViewProfileButton);
        }

        /// <summary>
        ///     Updates the connection status text to the current status.
        /// </summary>
        private void UpdateConnectionStatus()
        {
            string status;

            switch (OnlineManager.Status.Value)
            {
                case ConnectionStatus.Disconnected:
                    status = "Disconnected";
                    break;
                case ConnectionStatus.Connecting:
                    status = "Connecting to the server...";
                    break;
                case ConnectionStatus.Connected:
                    status = $"Logged in as: {OnlineManager.Self.OnlineUser.Username}";
                    break;
                case ConnectionStatus.Reconnecting:
                    status = $"Reconnecting. Please wait";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            TextConnectionStatus.Text = status;
        }

        /// <summary>
        ///     Called when the connection status has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
        {
            UpdateConnectionStatus();
            UpdateButtons();
        }

        /// <summary>
        ///    Updates the login button text with the current online status
        /// </summary>
        private void UpdateButtons()
        {
            switch (OnlineManager.Status.Value)
            {
                case ConnectionStatus.Disconnected:
                    LoginButton.OriginalColor = Color.LimeGreen;
                    LoginButton.Text.Text = "Login";

                    LoginButton.Animations.Clear();
                    LoginButton.Animations.Add(new Animation(AnimationProperty.X, Easing.OutQuint, LoginButton.X, 0, 225));

                    ViewProfileButton.Visible = false;
                    break;
                case ConnectionStatus.Connecting:
                    LoginButton.OriginalColor = Color.Lavender;
                    LoginButton.Text.Text = "Please Wait...";
                    break;
                case ConnectionStatus.Connected:
                    LoginButton.OriginalColor = Color.Crimson;
                    LoginButton.Text.Text = "Log Out";

                    LoginButton.Animations.Clear();
                    LoginButton.Animations.Add(new Animation(AnimationProperty.X, Easing.OutQuint, LoginButton.X, 100, 225));

                    ViewProfileButton.Visible = true;
                    break;
                case ConnectionStatus.Reconnecting:
                    LoginButton.OriginalColor = Color.Lavender;
                    LoginButton.Text.Text = "Please Wait...";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Called when the login button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnLoginButtonClicked(object sender, EventArgs e)
        {
            switch (OnlineManager.Status.Value)
            {
                case ConnectionStatus.Disconnected:
                    OnlineManager.Login();
                    break;
                case ConnectionStatus.Connecting:
                    break;
                case ConnectionStatus.Connected:
                    ThreadScheduler.Run(() => OnlineManager.Client?.Disconnect());
                    break;
                case ConnectionStatus.Reconnecting:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Called when the view profile button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnViewProfileButtonClicked(object sender, EventArgs e)
        {
            if (OnlineManager.Self == null)
                return;

            BrowserHelper.OpenURL($"{OnlineClient.WEBSITE_URL}/profile/{OnlineManager.Self.OnlineUser.Id}");
        }
    }
}
