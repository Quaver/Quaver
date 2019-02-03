/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Scheduling;
using Wobble.Assets;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI;

namespace Quaver.Shared.Graphics.Backgrounds
{
    public static class BackgroundManager
    {
        /// <summary>
        ///     The background image sprite to use.
        /// </summary>
        public static BackgroundImage Background { get; private set; }

        /// <summary>
        ///     When a background is loaded, this'll be emitted, this is mainly for
        ///     song select thumbnails to know when they should fade back in.
        /// </summary>
        public static event EventHandler<BackgroundLoadedEventArgs> Loaded;

        /// <summary>
        ///     The amount of times a load request was made, used to prevent multi-threading
        ///     race conditions.
        /// </summary>
        private static int LoadRequestCount { get; set; }

        /// <summary>
        ///     If the background is permitted to fade in.
        /// </summary>
        public static bool PermittedToFadeIn { get; set; }

        /// <summary>
        ///     Initializes the background sprite.
        /// </summary>
        public static void Initialize() => Background = new BackgroundImage(UserInterface.MenuBackground);

        /// <summary>
        ///     Updates the background sprite.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Update(GameTime gameTime)
        {
            if (!PermittedToFadeIn && Background.Animations.Count > 0)
                Background.BrightnessSprite.Animations.Clear();

            Background.Update(gameTime);
        }

        /// <summary>
        ///     Draws the background sprite.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Draw(GameTime gameTime) => Background?.Draw(gameTime);

        /// <summary>
        ///     Loads a background for an individual map.
        /// </summary>
        public static void Load(Map map, int dim = 0)
        {
            FadeOut();
            LoadRequestCount++;
            var requestCount = LoadRequestCount;

            var oldTexture = Background.Image;

            ThreadScheduler.Run(delegate
            {
                if (requestCount != LoadRequestCount)
                {
                    if (oldTexture != UserInterface.MenuBackground)
                        oldTexture.Dispose();

                    return;
                }

                var tex = LoadTexture(MapManager.GetBackgroundPath(map));

                if (requestCount != LoadRequestCount)
                {
                    if (oldTexture != UserInterface.MenuBackground)
                        oldTexture.Dispose();

                    return;
                }

                Background.Image = tex;
                Loaded?.Invoke(typeof(BackgroundManager), new BackgroundLoadedEventArgs(map, tex));

                if (PermittedToFadeIn)
                    FadeIn(dim);

                if (oldTexture != UserInterface.MenuBackground)
                    oldTexture.Dispose();
            });
        }

        /// <summary>
        ///     Fades out BG to black.
        /// </summary>
        public static void FadeOut()
        {
            Background.BrightnessSprite.Animations.Clear();

            var t = new Animation(AnimationProperty.Alpha, Easing.OutQuad, Background.BrightnessSprite.Alpha, 1, 300);
            Background.BrightnessSprite.Animations.Add(t);
        }

        /// <summary>
        ///     Fades in the BG from black.
        /// </summary>
        public static void FadeIn(int dim)
        {
            Background.BrightnessSprite.Animations.Clear();

            var t = new Animation(AnimationProperty.Alpha, Easing.InQuad, Background.BrightnessSprite.Alpha, dim / 100f, 300);
            Background.BrightnessSprite.Animations.Add(t);
        }

        /// <summary>
        ///     Laods a new background up.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static Texture2D LoadTexture(string path)
        {
            Texture2D newBackground;

            try
            {
                newBackground = AssetLoader.LoadTexture2DFromFile(path);
            }
            catch (Exception)
            {
                // If the background couldn't be loaded.
                newBackground = UserInterface.MenuBackground;
            }

            return newBackground;
        }
    }
}
