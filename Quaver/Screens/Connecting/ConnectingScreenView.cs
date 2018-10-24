using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using osu_database_reader;
using Quaver.Assets;
using Quaver.Graphics;
using Quaver.Graphics.Notifications;
using Quaver.Online;
using Quaver.Screens.Connecting.UI;
using Quaver.Screens.Menu;
using Quaver.Server.Client.Events.Disconnnection;
using Quaver.Server.Client.Events.Login;
using Quaver.Server.Client.Handlers;
using Quaver.Skinning;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
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
        ///     The Sprite that gives a background for the footer.
        /// </summary>
        private Sprite FooterBackground { get; set; }

        /// <summary>
        ///     Text that displays who the game is powered by.
        /// </summary>
        private SpriteText PoweredBy { get; set; }

        /// <summary>
        ///     The button to quit the game during connecting.
        /// </summary>
        private ImageButton QuitButton { get; set; }

        /// <summary>
        ///     Sprite that lays ontop of the screen to provide fade transitions.
        /// </summary>
        private Sprite ScreenTransitioner { get; set; }

        /// <summary>
        ///     Container specifically for handling UI related to server connection.
        /// </summary>
        private ConnectingContainer ConnectingContainer { get; }

        /// <summary>
        ///     If we're curerntly exiting the screen.
        /// </summary>
        private bool IsExitingScreen { get; set; }

        /// <summary>
        ///     The amount of time that has passed since the screen exit has initiated.
        /// </summary>
        private double TimeElapsedSinceExitInitiated { get; set; }

        /// <summary>
        ///     Whenever the screen needs to exit,this will be called (usually to switch screens.)
        /// </summary>
        private Action OnExitScreen { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public ConnectingScreenView(Screen screen) : base(screen)
        {
            CreateBackground();
            CreateQuaverLogo();

            ConnectingContainer = new ConnectingContainer(this) {Parent = Container};

            // Not sure if these should be added on this screen specifically, but included anyway.
            //CreateFooterBackground();
            //CreatePoweredByText();

            CreateQuitButton();
            CreateScreenTransitioner();
            HookToOnlineEvents();
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
            HandleScreenExiting(gameTime);

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
        public override void Destroy()
        {
            Container?.Destroy();
            OnExitScreen = null;

            if (OnlineManager.Client != null)
            {
                OnlineManager.Client.OnLoginSuccess -= OnLoginSuccess;
                OnlineManager.Client.OnLoginFailed -= OnLoginFailed;
                OnlineManager.Client.OnDisconnection -= OnDisconnection;
            }
        }

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
            Alpha = 0,
        };

        /// <summary>
        ///     When connection to the server was a success.
        /// </summary>
        private void OnConnected() => ConnectingContainer.OnConnected();

        /// <summary>
        ///     When the connection to the server was a failure.
        /// </summary>
        private void OnFailure() => ConnectingContainer.OnConnectionFailure();

        /// <summary>
        ///     Handles connecting to the server.
        /// </summary>
        public void Connect()
        {
            OnlineManager.Login();
            HookToOnlineEvents();
        }

        /// <summary>
        ///     Adds event handlers for all related online events.
        /// </summary>
        private void HookToOnlineEvents()
        {
            OnlineManager.Client.OnLoginSuccess += OnLoginSuccess;
            OnlineManager.Client.OnLoginFailed += OnLoginFailed;
            OnlineManager.Client.OnDisconnection += OnDisconnection;
        }

        /// <summary>
        ///     Exits to a specified screen whenever OnExitScreen is called.
        /// </summary>
        /// <param name="screen"></param>
        public void ExitToScreen(QuaverScreen screen)
        {
            IsExitingScreen = true;

            ScreenTransitioner.Transformations.Clear();
            ScreenTransitioner.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear, ScreenTransitioner.Alpha, 1, 1000));

            OnExitScreen += () => QuaverScreenManager.ChangeScreen(screen);
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
                OnExitScreen?.Invoke();
        }

        /// <summary>
        ///     Called when successfully logging into the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoginSuccess(object sender, LoginReplyEventArgs e)
        {
            OnConnected();
            NotificationManager.Show(NotificationLevel.Success, $"Successfully logged in as: {e.Self.OnlineUser.Username}!");

            ExitToScreen(new MenuScreen());
        }

        /// <summary>
        ///     When the login fails, this'll be called.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoginFailed(object sender, FailureToLoginEventArgs e) => OnFailure();

        /// <summary>
        ///     Called when disconnected from the server/failed to connect to the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisconnection(object sender, DisconnectedEventArgs e)
        {
            // If the user can't initially connect to the server (server is down.)
            switch (e.CloseEventArgs.Code)
            {
                // Error ocurred while connecting.
                case 1006:
                case 1002:
                    OnFailure();
                    return;
            }
        }
    }
}