/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.Settings;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.Importing
{
    public class ImportingScreenView : ScreenView
    {
        /// <summary>
        ///     The background image for the screen
        /// </summary>
        private BackgroundImage Background { get; set; }

        /// <summary>
        ///     The loading wheel that shows we're currently importing maps.
        /// </summary>
        private Sprite LoadingWheel { get; set; }

        /// <summary>
        ///     Header sprite for what's currently going on.
        /// </summary>
        public SpriteText Header { get; private set; }

        /// <summary>
        ///     Information on what maps are being imported
        /// </summary>
        public SpriteText InformationText { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public ImportingScreenView(Screen screen) : base(screen)
        {
            CreateBackground();
            CreateBanner();
            MapsetImporter.ImportingMapset += OnImportingMapset;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleLoadingWheelAnimations();
            Container?.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            MapsetImporter.ImportingMapset -= OnImportingMapset;
            Container?.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateBackground() => Background = new BackgroundImage(UserInterface.MenuBackground, 10, false)
        {
            Parent = Container
        };

        /// <summary>
        /// </summary>
        private void CreateLoadingWheel(Sprite parent) => LoadingWheel = new Sprite
        {
            Parent = parent,
            Alignment = Alignment.BotCenter,
            Size = new ScalableVector2(60, 60),
            Image = UserInterface.LoadingWheel,
            Tint = Color.Yellow,
            Y = -10
        };

        /// <summary>
        ///     Animates the loading wheel.
        /// </summary>
        private void HandleLoadingWheelAnimations()
        {
            if (LoadingWheel.Animations.Count != 0)
                return;

            var rotation = MathHelper.ToDegrees(LoadingWheel.Rotation);
            LoadingWheel.ClearAnimations();
            LoadingWheel.Animations.Add(new Animation(AnimationProperty.Rotation, Easing.Linear, rotation, rotation + 360, 1000));
        }

        /// <summary>
        ///     This method will update the screen when a mapset is finished being imported.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnImportingMapset(object sender, ImportingMapsetEventArgs e) => InformationText.Text = $"Now importing {e.FileName}. {e.Index + 1}/{e.Queue.Count}";

        /// <summary>
        ///     Creates the banner at the top of the screen
        /// </summary>
        private void CreateBanner()
        {
            var background = new Sprite()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(WindowManager.Width, 150),
                Tint = Color.Black,
                Alpha = 0.85f
            };

            var line = new Sprite()
            {
                Parent = background,
                Size = new ScalableVector2(background.Width, 1),
                Tint = Colors.MainAccent
            };

            Header = new SpriteText(Fonts.Exo2SemiBold, "Please wait. Maps are being imported.", 16)
            {
                Parent = background,
                Alignment = Alignment.TopCenter,
                Y = 10
            };

            InformationText = new SpriteText(Fonts.Exo2SemiBold, "", 14)
            {
                Parent = background,
                Alignment = Alignment.TopCenter,
                Y = Header.Y + Header.Height + 5
            };

            CreateLoadingWheel(background);
        }
    }
}
