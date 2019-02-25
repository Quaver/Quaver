/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.UI;
using Wobble.Logging;
using Logger = Wobble.Logging.Logger;

namespace Quaver.Shared.Graphics.Backgrounds
{
    public static class BackgroundHelper
    {
        /// <summary>
        ///     The current background that we're working with.
        /// </summary>
        public static BackgroundImage Background { get; private set; }

        /// <summary>
        ///     The currently loaded raw background texture.
        /// </summary>
        public static Texture2D RawTexture { get; private set; }

        /// <summary>
        ///     A cached version of the blurred texture
        /// </summary>
        public static Texture2D BlurredTexture { get; private set; }

        /// <summary>
        ///     The individual map this background is for.
        /// </summary>
        public static Map Map { get; private set; }

        /// <summary>
        ///     Dictates if we have a cached blurred texture of the current background
        /// </summary>
        private static bool ShouldBlur { get; set; }

        /// <summary>
        ///     Cancellation token to stop the existing background load tasks
        /// </summary>
        private static CancellationTokenSource Source { get; set; }

        /// <summary>
        ///     Event invoked when a new background has been loaded
        /// </summary>
        public static event EventHandler<BackgroundLoadedEventArgs> Loaded;

        /// <summary>
        ///     Event invoked when a new background has been blurred
        /// </summary>
        public static event EventHandler<BackgroundBlurredEventArgs> Blurred;

        /// <summary>
        ///     Initializes the background helper for the entire game.
        /// </summary>
        public static void Initialize()
        {
            Background = new BackgroundImage(UserInterface.MenuBackground, 0);
            Source = new CancellationTokenSource();
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Update(GameTime gameTime) => Background?.Update(gameTime);

        /// <summary>
        ///     Set per screen.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Draw(GameTime gameTime)
        {
            if (ShouldBlur)
            {
                try
                {
                    GameBase.Game.SpriteBatch.End();
                }
                catch (Exception e)
                {
                    // ignored
                }

                var blur = new GaussianBlur(0.1f);
                BlurredTexture = blur.PerformGaussianBlur(blur.PerformGaussianBlur(blur.PerformGaussianBlur(blur.PerformGaussianBlur(RawTexture))));
                ShouldBlur = false;
                Blurred?.Invoke(typeof(BackgroundHelper), new BackgroundBlurredEventArgs(Map, BlurredTexture));
            }

            Background?.Draw(gameTime);
        }

        /// <summary>
        ///     Queues a load of the background for a map
        /// </summary>
        public static void Load(Map map) => ThreadScheduler.Run(() =>
        {
            Task.Run(async () =>
            {
                Source.Cancel();
                Source.Dispose();
                Source = new CancellationTokenSource();

                Map = map;
                var token = Source.Token;

                token.ThrowIfCancellationRequested();

                try
                {
                    var path = MapManager.GetBackgroundPath(map);

                    var tex = File.Exists(path) ? AssetLoader.LoadTexture2DFromFile(path) : UserInterface.MenuBackground;
                    RawTexture = tex;

                    token.ThrowIfCancellationRequested();

                    await Task.Delay(100, token);
                    ShouldBlur = true;
                    Loaded?.Invoke(typeof(BackgroundHelper), new BackgroundLoadedEventArgs(map, tex));
                }
                catch (OperationCanceledException e)
                {
                    // ignored
                    Console.WriteLine(e);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            });
        });

        /// <summary>
        ///     Fades the background brightness all the way to black
        /// </summary>
        public static void FadeToBlack()
        {
            Background.BrightnessSprite.ClearAnimations();
            Background.BrightnessSprite.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Background.BrightnessSprite.Alpha, 1, 250));
        }

        /// <summary>
        ///     Unfades the background
        /// </summary>
        public static void FadeIn()
        {
            Background.BrightnessSprite.ClearAnimations();
            Background.BrightnessSprite.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Background.BrightnessSprite.Alpha, 0.30f, 250));
        }

        /// <summary>
        ///     Unfades the background to a specific alpha
        /// </summary>
        /// <param name="alpha"></param>
        public static void FadeIn(float alpha)
        {
            Background.BrightnessSprite.ClearAnimations();
            Background.BrightnessSprite.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Background.BrightnessSprite.Alpha, alpha, 250));
        }
    }
}
