using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using osu_database_reader;
using Quaver.Assets;
using Quaver.Graphics;
using Quaver.Graphics.Notifications;
using Quaver.Screens.Menu;
using Quaver.Skinning;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Screens;
using Wobble.Window;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Quaver.Screens.Connecting
{
    public class ConnectingScreenView : ScreenView
    {
        /// <summary>
        ///     The background image for the screen.
        /// </summary>
        private BackgroundImage Background { get; set;  }

        /// <summary>
        ///     The huge Quaver logo in the middle of the screen.
        /// </summary>
        private Sprite QuaverLogo { get; set; }
        /// <summary>
        ///     Text that displays that we're currently connecting to the server.
        /// </summary>
        private SpriteText TextConnectingToServer { get; set; }

        /// <summary>
        ///     The wheel that makes it look like we're loading.
        /// </summary>
        private Sprite LoadingWheel { get; set; }

        /// <summary>
        ///     The Sprite that gives a background for the footer.
        /// </summary>
        private Sprite FooterBackground { get; set; }

        /// <summary>
        ///     Text that displays who the game is powered by.
        /// </summary>
        private SpriteText PoweredBy { get; set; }

        /// <summary>
        ///     The time since the connecting to server text was last changed.
        /// </summary>
        private double TimeSinceLastTextConnectingToServerChange { get; set; }

        /// <summary>
        ///     The button to quit the game during connecting.
        /// </summary>
        private ImageButton QuitButton { get; set; }

        /// <summary>
        ///     The button to retry the connection.
        /// </summary>
        private TextButton RetryButton { get; set; }

        /// <summary>
        ///     The button to play offline.
        /// </summary>
        private TextButton PlayOfflineButton { get; set; }

        /// <summary>
        ///     Sprite that lays ontop of the screen to provide fade transitions.
        /// </summary>
        private Sprite ScreenTransitioner { get; set; }

        /// <summary>
        ///     If we're curerntly exiting the screen.
        /// </summary>
        private bool IsExitingScreen { get; set; }

        /// <summary>
        ///     The amount of time that has passed since the screen exit has initiated.
        /// </summary>
        private double TimeElapsedSinceExitInitiated { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public ConnectingScreenView(Screen screen) : base(screen)
        {
            CreateBackground();
            CreateQuaverLogo();
            CreateConnectingToServerText();
            CreateLoadingWheel();

            // Not sure if these should be added on this screen specifically, but included anyway.
            //CreateFooterBackground();
            //CreatePoweredByText();

            CreateQuitButton();
            CreateRetryButton();
            CreatePlayOfflineButton();
            CreateScreenTransitioner();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Debating if this should be added. Maybe the loading wheel is enough?
            // AnimateConnectingToServerText(gameTime);

            AnimateQuitButton(gameTime);
            AnimateLoadingWheel(gameTime);
            AnimateRetryAndQuitButtons(gameTime);
            HandleScreenExiting(gameTime);

            // Obviously don't keep these keybindings around. This is just for testing purposes.
            if (KeyboardManager.IsUniqueKeyPress(Keys.D1))
                OnConnected();

            if (KeyboardManager.IsUniqueKeyPress(Keys.D2))
                OnFailure();

            Container?.Update(gameTime);
        }

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
        ///     Creates the background image for the screen.
        /// </summary>
        private void CreateBackground() => Background = new BackgroundImage(UserInterface.ConnectingBackground, 0, false)
        {
            Parent = Container
        };

        /// <summary>
        ///     Creates the Quaver logo for the screen.
        /// </summary>
        private void CreateQuaverLogo() => QuaverLogo = new Sprite()
        {
            Parent = Container,
            Alignment = Alignment.MidCenter,
            SpriteBatchOptions = new SpriteBatchOptions() { BlendState = BlendState.NonPremultiplied },
            Size = new ScalableVector2(261 * 0.85f, 246 * 0.85f),
            Image = UserInterface.QuaverLogoStylish,
            Y = -100
        };

        /// <summary>
        ///     Creates the text that displays we're connecting to the server.
        /// </summary>
        private void CreateConnectingToServerText() => TextConnectingToServer = new SpriteText(Fonts.Exo2Regular24, "Connecting to the server. Please wait...", 0.70f)
        {
            Parent = Container,
            Alignment = Alignment.MidCenter,
            Y = 25
        };

        /// <summary>
        ///     Performs a "." ".." "..." animation for the connecting to server text.
        /// </summary>
        /// <param name="gameTime"></param>
        private void AnimateConnectingToServerText(GameTime gameTime)
        {
            TimeSinceLastTextConnectingToServerChange += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (TimeSinceLastTextConnectingToServerChange < 600)
                return;

            var count = TextConnectingToServer.Text.Count(f => f == '.');

            if (count < 3)
                TextConnectingToServer.Text += ".";
            else
                TextConnectingToServer.Text = "Connecting to the server. Please wait";

            TimeSinceLastTextConnectingToServerChange = 0;
        }

        /// <summary>
        ///     Creates the loading wheel sprite.
        /// </summary>
        private void CreateLoadingWheel() => LoadingWheel = new Sprite
        {
            Parent = Container,
            Alignment = Alignment.MidCenter,
            Image = UserInterface.LoadingWheel,
            Size = new ScalableVector2(75, 75),
            Y = TextConnectingToServer.Y + TextConnectingToServer.MeasureString().Y / 2f + 55,
            SpriteBatchOptions = new SpriteBatchOptions { BlendState = BlendState.NonPremultiplied }
        };

        /// <summary>
        ///     Animates the loading wheel sprite.
        /// </summary>
        /// <param name="gameTime"></param>
        private void AnimateLoadingWheel(GameTime gameTime)
        {
            var deltaTime = gameTime.ElapsedGameTime.TotalMilliseconds;
            LoadingWheel.Rotation = (float)(MathHelper.ToDegrees(LoadingWheel.Rotation) + 8 * deltaTime / 30f);
        }

        /// <summary>
        ///     Creates the background of the footer.
        /// </summary>
        private void CreateFooterBackground() => FooterBackground = new Sprite()
        {
            Parent = Container,
            Size = new ScalableVector2(WindowManager.Width, 40),
            Alignment = Alignment.BotLeft,
            Tint = Color.Black,
            Alpha = 0.45f,
        };

        /// <summary>
        ///
        /// </summary>
        private void CreatePoweredByText() => PoweredBy = new SpriteText(Fonts.Exo2Regular24, "Powered by Swan. Created by the community 2017-2018.", 0.45f)
        {
            Parent = FooterBackground,
            Alignment = Alignment.MidCenter,
        };

        /// <summary>
        ///     Creates the button to quit the game.
        /// </summary>
        private void CreateQuitButton() => QuitButton = new ImageButton(FontAwesome.PowerOff, (o, e) => { GameBase.Game.Exit(); })
        {
            Parent = Container,
            Alignment = Alignment.TopRight,
            Position = new ScalableVector2(-20, 15),
            Size = new ScalableVector2(25, 25),
            SpriteBatchOptions = new SpriteBatchOptions() { BlendState = BlendState.NonPremultiplied },
        };

        /// <summary>
        ///     Animates the quit button based on if it's hovered.
        /// </summary>
        /// <param name="gameTime"></param>
        private void AnimateQuitButton(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
            QuitButton.FadeToColor(QuitButton.IsHovered ? Colors.SecondaryAccent : Color.White, dt, 30);
        }

        /// <summary>
        ///     Creates the ScreenTransitioner sprite.
        /// </summary>
        private void CreateScreenTransitioner() => ScreenTransitioner = new Sprite()
        {
            Parent = Container,
            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height),
            Tint = Color.Black,
            Alpha = 1,
            Transformations =
            {
                new Transformation(TransformationProperty.Alpha, Easing.EaseInQuint, 1, 0, 1000)
            }
        };

        /// <summary>
        ///     When connection to the server was a success.
        /// </summary>
        private void OnConnected()
        {
            TextConnectingToServer.Text = "Connected! Waiting for a response...";
        }

        /// <summary>
        ///     When the connection to the server was a failure.
        /// </summary>
        private void OnFailure()
        {
            TextConnectingToServer.Text = "Failed to connect to the server.";
            LoadingWheel.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.EaseInQuint, 1, 0, 400));

            // Retry button animation
            RetryButton.Transformations.Clear();
            RetryButton.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutBounce, RetryButton.X,
                WindowManager.Width / 2f - 15 - RetryButton.Width, 500));

            // Play offline button animation.
            PlayOfflineButton.Transformations.Clear();
            PlayOfflineButton.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutBounce, PlayOfflineButton.X,
                -WindowManager.Width / 2f + 15 + RetryButton.Width, 500));
        }

        /// <summary>
        ///     Handles connecting to the server.
        /// </summary>
        private void Connect()
        {
            NotificationManager.Show(NotificationLevel.Warning, "Connecting to the server is only available in the Steam branch, sorry.");
        }

        /// <summary>
        ///     Exits the screen to the main menu.
        /// </summary>
        private void ExitToMainMenu()
        {
            IsExitingScreen = true;

            ScreenTransitioner.Transformations.Clear();
            ScreenTransitioner.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear, ScreenTransitioner.Alpha, 1, 600));
        }

        /// <summary>
        ///     Creates the button to go play offline.
        /// </summary>
        private void CreatePlayOfflineButton()
        {
            PlayOfflineButton = new TextButton(UserInterface.BlankBox, Fonts.Exo2Regular24, "PLAY OFFLINE", 0.50f, (o, e) =>
            {
                SkinManager.Skin.SoundClick.CreateChannel()?.Play();

                // Retry button animation
                RetryButton.Transformations.Clear();
                RetryButton.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutBounce, RetryButton.X,
                    -WindowManager.Width - RetryButton.Width - 5, 750));

                // Play offline button animation.
                PlayOfflineButton.Transformations.Clear();
                PlayOfflineButton.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutBounce, PlayOfflineButton.X,
                    WindowManager.Width + RetryButton.Width + 5, 750));

                ExitToMainMenu();
            })
            {
                Parent = Container,
                Size = new ScalableVector2(175, 50),
                Alignment = Alignment.MidRight,
                Tint = Color.Black,
                Alpha = 0.65f,
                Y = TextConnectingToServer.Y + TextConnectingToServer.MeasureString().Y / 2f + 55,
            };

            PlayOfflineButton.X = PlayOfflineButton.Width + 5;
        }

        /// <summary>
        ///     Creates the button to retry the connection.
        /// </summary>
        private void CreateRetryButton()
        {
            // Add buttons to retry the connection
            RetryButton = new TextButton(UserInterface.BlankBox, Fonts.Exo2Regular24, "RETRY", 0.50f, (o, e) =>
            {
                SkinManager.Skin.SoundClick.CreateChannel()?.Play();
                Connect();
            })
            {
                Parent = Container,
                Size = new ScalableVector2(175, 50),
                Alignment = Alignment.MidLeft,
                Tint = Color.Black,
                Alpha = 0.65f,
                Y = TextConnectingToServer.Y + TextConnectingToServer.MeasureString().Y / 2f + 55,
            };

            RetryButton.X = -RetryButton.Width - 5;
        }

        /// <summary>
        ///     The amount of time that has elapsed since the screen has begun editing.
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandleScreenExiting(GameTime gameTime)
        {
            if (!IsExitingScreen)
                return;

            TimeElapsedSinceExitInitiated += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (TimeElapsedSinceExitInitiated > 1200)
                ScreenManager.ChangeScreen(new MainMenuScreen());
        }

        /// <summary>
        ///     Animates the retry and quit buttons with their hover animations.
        /// </summary>
        /// <param name="gameTime"></param>
        private void AnimateRetryAndQuitButtons(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            RetryButton.FadeToColor(RetryButton.IsHovered ? Colors.MainAccent : Color.Black, dt, 60);
            PlayOfflineButton.FadeToColor(PlayOfflineButton.IsHovered ? Colors.MainAccent : Color.Black, dt, 60);
        }
    }
}