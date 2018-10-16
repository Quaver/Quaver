using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Graphics;
using Quaver.Graphics.Notifications;
using Quaver.Online;
using Quaver.Scheduling;
using Quaver.Server.Client;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Screens.Menu.UI.Navigation.User
{
    public class UserProfileContainer : ScrollContainer
    {
        /// <summary>
        ///     Reference to the parent menu screern view.
        /// </summary>
        private MenuScreenView View { get; }

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
        private SpriteTextBitmap TextConnectionStatus { get; set; }

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
        public UserProfileContainer(MenuScreenView view) : base(new ScalableVector2(OriginalWidth, 0),
            new ScalableVector2(OriginalWidth, OriginalHeight))
        {
            View = view;

            Tint = Color.Black;
            Alpha = 0.80f;
            Scrollbar.Visible = false;

            NavbarButton = View.Navbar.RightAlignedItems.First() as NavbarItemUser;

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
            lock (Transformations)
            {
                Transformations.Clear();

                var targetHeight = selected ? OriginalHeight : 0;
                Transformations.Add(new Transformation(TransformationProperty.Height, Easing.EaseOutQuint, Height, targetHeight, 500));
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

            TextConnectionStatus = new SpriteTextBitmap(BitmapFonts.Exo2SemiBold, " ", 24, Color.White, Alignment.MidCenter, int.MaxValue)
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
            };

            ViewProfileButton = new BorderedTextButton("View Profile", Colors.MainAccent, OnViewProfileButtonClicked)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = TextConnectionStatus.Y + TextConnectionStatus.Height + 20,
                X = -100,
                SetChildrenVisibility = true
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
            TextConnectionStatus.Size = new ScalableVector2(TextConnectionStatus.Width * 0.60f, TextConnectionStatus.Height * 0.60f);
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
                    LoginButton.UpdateText("Log In", 0.55f);

                    LoginButton.Transformations.Clear();
                    LoginButton.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutQuint, LoginButton.X, 0, 225));

                    ViewProfileButton.Visible = false;
                    break;
                case ConnectionStatus.Connecting:
                    LoginButton.OriginalColor = Color.Lavender;
                    LoginButton.UpdateText("Please Wait...", 0.55f);
                    break;
                case ConnectionStatus.Connected:
                    LoginButton.OriginalColor = Color.Crimson;
                    LoginButton.UpdateText("Log Out", 0.55f);

                    LoginButton.Transformations.Clear();
                    LoginButton.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutQuint, LoginButton.X, 100, 225));

                    ViewProfileButton.Visible = true;
                    break;
                case ConnectionStatus.Reconnecting:
                    LoginButton.OriginalColor = Color.Lavender;
                    LoginButton.UpdateText("Please Wait...", 0.55f);
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
                    Scheduler.RunThread(() => OnlineManager.Client?.Disconnect());
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
            NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet. Check back soon!");
        }
    }
}