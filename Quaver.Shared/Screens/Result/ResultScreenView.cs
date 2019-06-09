/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Menu;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Result.UI;
using TagLib.Riff;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Result
{
    public class ResultScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        private MenuHeader MenuHeader { get; set; }

        /// <summary>
        /// </summary>
        private MenuFooter MenuFooter { get; set; }

        /// <summary>
        ///     Displays the information for the map & score
        /// </summary>
        private ResultMapInformation MapInformation { get; set; }

        /// <summary>
        ///     Container for displaying everything about the achieved score
        /// </summary>
        private ResultScoreContainer ScoreContainer { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public ResultScreenView(Screen screen) : base(screen)
        {
            CreateMapInformation();
            CreateScoreContainer();
            CreateMenuHeader();
            CreateMenuFooter();

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
        private void OnBackgroundBlurred(object sender, BackgroundBlurredEventArgs e)
        {
            HandleBackgroundChange();
        }

        /// <summary>
        /// </summary>
        private void CreateMenuHeader()
        {
            MenuHeader = new MenuHeader(FontAwesome.Get(FontAwesomeIcon.fa_gamepad_console), "score", "results",
                "View in-depth results of a play", ColorHelper.HexToColor("#69acc5"))
            {
                Parent = Container
            };
        }

        /// <summary>
        /// </summary>
        private void CreateMenuFooter()
        {
            var screen = (ResultScreen) Screen;

            var rightButtons = new List<ButtonText>();

            if (screen.Gameplay == null || (screen.Gameplay != null && !screen.Gameplay.IsMultiplayerGame))
            {
                rightButtons.Add(new ButtonText(FontsBitmap.GothamRegular, "Retry", 14, (sender, args) => screen.ExitToRetryMap()));
                rightButtons.Add(new ButtonText(FontsBitmap.GothamRegular, "Watch Replay", 14, (sender, args) => screen.ExitToRetryMap()));
            }

            MenuFooter = new ResultMenuFooter(new List<ButtonText>()
            {
                new ButtonText(FontsBitmap.GothamRegular, "BACK", 14, (sender, args) => screen.ExitToMenu()),
                new ButtonText(FontsBitmap.GothamRegular, "EXPORT REPLAY", 14, (sender, args) => screen.ExportReplay())
            }, rightButtons)
            {
                Parent = Container,
                Alignment = Alignment.BotLeft,
            };
        }

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
            MapInformation.MoveToY(46 + 20, Easing.OutQuint, 800);
        }

        /// <summary>
        ///     Creates the sprite that displays the achieved score
        /// </summary>
        private void CreateScoreContainer()
        {
            ScoreContainer = new ResultScoreContainer(Screen as ResultScreen)
            {
                Parent = Container,
                Alignment = Alignment.BotCenter,
                Y = -46 - 20
            };

            ScoreContainer.X = -ScoreContainer.Width - 100;
            ScoreContainer.MoveToX(0, Easing.OutQuint, 800);
        }
    }
}
