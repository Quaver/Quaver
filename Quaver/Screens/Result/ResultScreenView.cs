using System;
using Microsoft.Xna.Framework;
using Quaver.Database.Maps;
using Quaver.Graphics.Backgrounds;
using Quaver.Screens.Result.UI;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Screens;

namespace Quaver.Screens.Result
{
    public class ResultScreenView : ScreenView
    {
        /// <summary>
        ///     Displays the information for the map & score
        /// </summary>
        private ResultMapInformation MapInformation { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public ResultScreenView(Screen screen) : base(screen)
        {
            CreateMapInformation();
            BackgroundHelper.Blurred += OnBackgroundBlurred;
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

            BackgroundHelper.Draw(gameTime);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            BackgroundHelper.Blurred -= OnBackgroundBlurred;
            Container?.Destroy();
        }

        /// <summary>
        ///     Makes sure that the background of the map is up-to-date.
        /// </summary>
        public void HandleBackgroundChange()
        {
            // If we've already got the background loaded up, then use it.
            if (MapManager.GetBackgroundPath(BackgroundHelper.Map) == MapManager.GetBackgroundPath(MapManager.Selected.Value))
            {
                BackgroundHelper.Background.Image = BackgroundHelper.BlurredTexture;
                BackgroundHelper.FadeIn();
                return;
            }

            // Otherwise queue a load on it.
            BackgroundHelper.Load(MapManager.Selected.Value);
        }

        /// <summary>
        ///     Called after a background has been loaded and blurred.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackgroundBlurred(object sender, BackgroundBlurredEventArgs e) => HandleBackgroundChange();

        /// <summary>
        ///     Creates the sprite that displays the map information
        /// </summary>
        private void CreateMapInformation()
        {
            MapInformation = new ResultMapInformation(Screen as ResultScreen)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
            };

            MapInformation.Y = -MapInformation.Height;
            MapInformation.MoveToY(28, Easing.OutQuint, 800);
        }
    }
}