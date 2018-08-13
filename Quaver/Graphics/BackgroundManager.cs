using System;
using Amib.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Database.Maps;
using Wobble.Assets;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI;
using Wobble.Screens;

namespace Quaver.Graphics
{
    public static class BackgroundManager
    {
        /// <summary>
        ///     The background image sprite to use.
        /// </summary>
        public static BackgroundImage Background { get; private set; }

        /// <summary>
        ///     The current thread pool worker thread.
        /// </summary>
        public static IWorkItemResult CurrentWorkerThread { get; private set; }

        /// <summary>
        ///     Thread pool used to run things in the background.
        /// </summary>
        public static readonly SmartThreadPool ThreadPool = new SmartThreadPool(new STPStartInfo
        {
            AreThreadsBackground = true,
            IdleTimeout = 600000,
            MaxWorkerThreads = 1,
            MinWorkerThreads = 1
        });

        /// <summary>
        ///     Initializes the background sprite.
        /// </summary>
        public static void Initialize() => Background = new BackgroundImage(UserInterface.MenuBackground);

        /// <summary>
        ///     Updates the background sprite.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Update(GameTime gameTime) => Background.Update(gameTime);

        /// <summary>
        ///     Draws the background sprite.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Draw(GameTime gameTime) => Background?.Draw(gameTime);

        /// <summary>
        ///     Loads a background for an individual mapset.
        /// </summary>
        /// <param name="set"></param>
        public static void Load(Mapset set)
        {
            FadeOut();

            if (CurrentWorkerThread != null && !CurrentWorkerThread.IsCompleted)
                CurrentWorkerThread.Cancel();

            CurrentWorkerThread = ThreadPool.QueueWorkItem(delegate
            {
                var tex = LoadTexture(set.Background);

                // dispose of the previous background
                if (Background.Image != UserInterface.MenuBackground)
                    Background.Image.Dispose();

                Background.Image = tex;
                FadeIn();
            });
        }

        /// <summary>
        ///     Loads a background for an individual map.
        /// </summary>
        public static void Load(Map map)
        {
            FadeOut();

            if (CurrentWorkerThread != null && !CurrentWorkerThread.IsCompleted)
                CurrentWorkerThread.Cancel();

            CurrentWorkerThread = ThreadPool.QueueWorkItem(delegate
            {
                var tex = LoadTexture(MapManager.GetBackgroundPath(map));

                // dispose of the previous background
                if (Background.Image != UserInterface.MenuBackground)
                    Background.Image.Dispose();

                Background.Image = tex;
                FadeIn();
            });
        }

        /// <summary>
        ///     Fades the background out to black.
        /// </summary>
        public static void FadeOut()
        {
            Background.BrightnessSprite.Transformations.Clear();

            var t = new Transformation(TransformationProperty.Alpha, Easing.EaseOutQuint, Background.BrightnessSprite.Alpha, 1, 600);
            Background.BrightnessSprite.Transformations.Add(t);
        }

        /// <summary>
        ///     Fades the background in.
        /// </summary>
        public static void FadeIn()
        {
            Background.BrightnessSprite.Transformations.Clear();

            var t = new Transformation(TransformationProperty.Alpha, Easing.EaseInQuad, Background.BrightnessSprite.Alpha, 0.35f, 500);
            Background.BrightnessSprite.Transformations.Add(t);
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