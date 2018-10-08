using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Graphics;
using Quaver.Screens.Menu;
using Quaver.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI.Buttons;
using Wobble.Window;

namespace Quaver.Screens.Connecting.UI
{
    public class ConnectingContainer : Container
    {
        /// <summary>
        ///     The parent screen view.
        /// </summary>
        private ConnectingScreenView View { get; }

        /// <summary>
        ///     Text that displays that we're currently connecting to the server.
        /// </summary>
        private SpriteText TextConnectingToServer { get; set; }

        /// <summary>
        ///     The wheel that makes it look like we're loading.
        /// </summary>
        private Sprite LoadingWheel { get; set; }

        /// <summary>
        ///     The button to retry the connection.
        /// </summary>
        private TextButton RetryButton { get; set; }

        /// <summary>
        ///     The button to play offline.
        /// </summary>
        private TextButton PlayOfflineButton { get; set; }

        /// <summary>
        ///     The time since the connecting to server text was last changed.
        /// </summary>
        private double TimeSinceLastTextConnectingToServerChange { get; set; }

        /// <summary>
        /// </summary>
        public ConnectingContainer(ConnectingScreenView screenView)
        {
            View = screenView;

            CreateConnectingToServerText();
            CreateLoadingWheel();
            CreateRetryButton();
            CreatePlayOfflineButton();
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            AnimateRetryAndQuitButtons(gameTime);
            AnimateLoadingWheel(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        ///     Creates the text that displays we're connecting to the server.
        /// </summary>
        private void CreateConnectingToServerText() => TextConnectingToServer = new SpriteText(Fonts.Exo2Regular24, "Connecting to the server. Please wait...", 0.70f)
        {
            Parent = this,
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
            Parent = this,
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

                View.ExitToScreen(new MenuScreen());
            })
            {
                Parent = this,
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
                View.Connect();
            })
            {
                Parent = this,
                Size = new ScalableVector2(175, 50),
                Alignment = Alignment.MidLeft,
                Tint = Color.Black,
                Alpha = 0.65f,
                Y = TextConnectingToServer.Y + TextConnectingToServer.MeasureString().Y / 2f + 55,
            };

            RetryButton.X = -RetryButton.Width - 5;
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

        /// <summary>
        ///     Called when successfully connected to the server.
        /// </summary>
        public void OnConnected()
        {
            TextConnectingToServer.Text = "Connected! Waiting for a response...";
        }

        /// <summary>
        ///     Called when failed to connect to the server.
        /// </summary>
        public void OnConnectionFailure()
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
    }
}