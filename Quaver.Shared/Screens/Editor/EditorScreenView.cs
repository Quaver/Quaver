/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2019 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Editor.UI;
using Quaver.Shared.Screens.Editor.UI.Dialogs.Metadata;
using Quaver.Shared.Screens.Editor.UI.Navigation;
using Quaver.Shared.Screens.Editor.UI.Rulesets;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Editor
{
    public class EditorScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        private BackgroundImage Background { get; set; }

        /// <summary>
        /// </summary>
        public EditorControlBar ControlBar { get; private set; }

        /// <summary>
        /// </summary>
        public EditorNavigationBar NavigationBar { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public EditorScreenView(Screen screen) : base(screen)
        {
            CreateBackground();
            CreateControlBar();
            CreateNavigationBar();

            /*new Sprite()
            {
                Parent = Container,
                Y = 120,
                Size = new ScalableVector2(170, 38),
                Image = AssetLoader.LoadTexture2DFromFile($@"C:\users\admin\desktop\aaas.png"),
                Alpha = 0.65f
            };

            new Sprite()
            {
                Parent = Container,
                Y = 170,
                Size = new ScalableVector2(170, 38),
                Image = AssetLoader.LoadTexture2DFromFile($@"C:\users\admin\desktop\aaad.png"),
                Alpha = 0.65f
            };

            new Sprite()
            {
                Parent = Container,
                Y = 220,
                Size = new ScalableVector2(170, 38),
                Image = AssetLoader.LoadTexture2DFromFile($@"C:\users\admin\desktop\aaab.png"),
                Alpha = 1
            };

            new Sprite()
            {
                Parent = Container,
                Y = 270,
                Size = new ScalableVector2(170, 38),
                Image = AssetLoader.LoadTexture2DFromFile($@"C:\users\admin\desktop\aaam.png"),
                Alpha = 0.65f
            };*/
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Background.Update(gameTime);

            var screen = (EditorScreen) Screen;
            screen.Ruleset.Update(gameTime);

            Container?.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.Black);
            Background.Draw(gameTime);

            var screen = (EditorScreen) Screen;
            screen.Ruleset.Draw(gameTime);

            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            var screen = (EditorScreen) Screen;
            screen.Ruleset.Destroy();

            BackgroundHelper.Loaded -= OnBackgroundLoaded;
            Container?.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateBackground()
        {
            Background = new BackgroundImage(UserInterface.BlankBox, 100, false);
            BackgroundHelper.Loaded += OnBackgroundLoaded;

            if (BackgroundHelper.Map == MapManager.Selected.Value)
            {
                Background.Image = BackgroundHelper.RawTexture;
                FadeBackgroundIn();
                return;
            }

            BackgroundHelper.Load(MapManager.Selected.Value);
        }

        /// <summary>
        ///     Called when the map's background is loaded, so we can fade it in.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackgroundLoaded(object sender, BackgroundLoadedEventArgs e)
        {
            Background.Image = e.Texture;
            FadeBackgroundIn();
        }

        /// <summary>
        ///     Fades the background in upon load.
        /// </summary>
        private void FadeBackgroundIn()
        {
            Background.BrightnessSprite.ClearAnimations();

            Background.BrightnessSprite.Animations.Add(new Animation(AnimationProperty.Alpha,
                Easing.Linear, Background.BrightnessSprite.Alpha, 0.40f, 200));
        }

        /// <summary>
        /// </summary>
        public void FadeBackgroundOut()
        {
            Background.BrightnessSprite.ClearAnimations();

            Background.BrightnessSprite.Animations.Add(new Animation(AnimationProperty.Alpha,
                Easing.Linear, Background.BrightnessSprite.Alpha, 1f, 200));
        }

        /// <summary>
        /// </summary>
        private void CreateControlBar()
        {
            var screen = (EditorScreen) Screen;

            ControlBar = new EditorControlBar(this, screen.WorkingMap)
            {
                Parent = Container,
                Alignment = Alignment.BotLeft,
            };

            ControlBar.Y = ControlBar.Height;

            ControlBar.MoveToY(1, Easing.OutQuint, 800);
        }

        /// <summary>
        /// </summary>
        private void CreateNavigationBar()
        {
            NavigationBar = new MainEditorNavigationBar(Screen as EditorScreen) {Parent = Container};
            NavigationBar.Y = -NavigationBar.Height;

            NavigationBar.MoveToY(0, Easing.OutQuint, 800);
        }
    }
}