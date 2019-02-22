/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Screens.Result.UI;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Result
{
    public class ResultScreenView : ScreenView
    {
        /// <summary>
        ///     Displays the information for the map & score
        /// </summary>
        private ResultMapInformation MapInformation { get; set; }

        /// <summary>
        ///     Container for displaying everything about the achieved score
        /// </summary>
        private ResultScoreContainer ScoreContainer { get; set; }

        /// <summary>
        ///     Container for displaying buttons on the screen
        /// </summary>
        public ResultButtonContainer ButtonContainer { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public ResultScreenView(Screen screen) : base(screen)
        {
            CreateMapInformation();
            CreateScoreContainer();
            CreateButtonContainer();
            BackgroundHelper.Blurred += OnBackgroundBlurred;

            var quaverScreen = Screen as QuaverScreen;

            // ReSharper disable once PossibleNullReferenceException
            quaverScreen.ScreenExiting += OnScreenExiting;
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

            var quaverScreen = Screen as QuaverScreen;

            // ReSharper disable once PossibleNullReferenceException
            quaverScreen.ScreenExiting -= OnScreenExiting;

            Container?.Destroy();
        }

        /// <summary>
        ///     Makes sure that the background of the map is up-to-date.
        /// </summary>
        public void HandleBackgroundChange()
        {
            // If we've already got the background loaded up, then use it.
            if (BackgroundHelper.Map != null && MapManager.GetBackgroundPath(BackgroundHelper.Map) == MapManager.GetBackgroundPath(MapManager.Selected.Value))
            {
                BackgroundHelper.Background.Image = BackgroundHelper.BlurredTexture;
                BackgroundHelper.FadeIn(0.5f);
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

        /// <summary>
        ///     Creates the sprite that displays the achieved score
        /// </summary>
        private void CreateScoreContainer()
        {
            ScoreContainer = new ResultScoreContainer(Screen as ResultScreen)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 28 + MapInformation.Height + 30
            };

            ScoreContainer.X = -ScoreContainer.Width - 100;
            ScoreContainer.MoveToX(0, Easing.OutQuint, 800);
        }

        /// <summary>
        ///     Creates the sprite that contains all of the navigation buttons for the screen
        /// </summary>
        private void CreateButtonContainer()
        {
            ButtonContainer = new ResultButtonContainer(Screen as ResultScreen)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
            };

            ButtonContainer.Y = ScoreContainer.Y + ScoreContainer.Height + 20 + ButtonContainer.Height;
            ButtonContainer.MoveToY((int) (ButtonContainer.Y - ButtonContainer.Height + 10), Easing.OutQuint, 600);
        }

        /// <summary>
        ///     Called when the screen is exiting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScreenExiting(object sender, ScreenExitingEventArgs e)
        {
            MapInformation.ClearAnimations();
            MapInformation.MoveToY((int)-MapInformation.Height, Easing.OutQuint, 600);

            ButtonContainer.ClearAnimations();
            ButtonContainer.MoveToY((int) (ScoreContainer.Y + ScoreContainer.Height + 50 + ButtonContainer.Height), Easing.OutQuint, 600);

            ScoreContainer.ClearAnimations();
            ScoreContainer.MoveToX(ScoreContainer.Width + 100, Easing.OutQuint, 600);
        }
    }
}
