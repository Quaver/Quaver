using System;
using System.Collections.Generic;
using System.Linq;
using Amib.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Database.Maps;
using Quaver.Scheduling;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics.UI;

namespace Quaver.Graphics.Backgrounds
{
    public static class BackgroundManager
    {
        public static BackgroundImage PreviousBackground { get; private set; }

        /// <summary>
        ///     The background image sprite to use.
        /// </summary>
        public static BackgroundImage Background { get; private set; }

        /// <summary>
        ///     The current thread pool worker thread.
        /// </summary>
        private static IWorkItemResult CurrentWorkerThread { get; set; }

        /// <summary>
        ///     Thread pool used to load backgrounds.
        /// </summary>
        private static readonly SmartThreadPool LoadingThread = new SmartThreadPool(new STPStartInfo
        {
            AreThreadsBackground = true,
            IdleTimeout = 600000,
            MaxWorkerThreads = 1,
            MinWorkerThreads = 1
        });

        /// <summary>
        ///     When a background is loaded, this'll be emitted, this is mainly for
        ///     song select thumbnails to know when they should fade back in.
        /// </summary>
        public static event EventHandler<BackgroundLoadedEventArgs> Loaded;

        /// <summary>
        ///     Initializes the background sprite.
        /// </summary>
        public static void Initialize()
        {
            Background = new BackgroundImage(UserInterface.MenuBackground);
            PreviousBackground = new BackgroundImage(UserInterface.MenuBackground);
        }

        /// <summary>
        ///     Updates the background sprite.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            Background.Alpha = MathHelper.Lerp(Background.Alpha, 1, (float) Math.Min(dt / 480, 1));
            PreviousBackground.Alpha = MathHelper.Lerp(PreviousBackground.Alpha, 0, (float) Math.Min(dt / 480, 1));


            Background?.Update(gameTime);
            PreviousBackground?.Update(gameTime);
        }

        /// <summary>
        ///     Draws the background sprite.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Draw(GameTime gameTime)
        {
            Background?.Draw(gameTime);
            PreviousBackground?.Draw(gameTime);
        }

        /// <summary>
        ///     Loads a background for an individual mapset.
        /// </summary>
        /// <param name="set"></param>
        public static void Load(Mapset set)
        {
            // Dispose previous background.
            if (Background.Image != UserInterface.MenuBackground)
            {
                var bgImage = Background.Image;
                Scheduler.RunAfter(() =>  bgImage.Dispose(), 5000);
            }

            if (CurrentWorkerThread != null && (!CurrentWorkerThread.IsCanceled || !CurrentWorkerThread.IsCompleted))
                CurrentWorkerThread.Cancel();

            CurrentWorkerThread = LoadingThread.QueueWorkItem(delegate
            {
                var tex = LoadTexture(set.Background);

                PreviousBackground = Background;

                Background = new BackgroundImage(tex) {Alpha = 0};

                if (MapManager.Selected.Value == set.Maps.First())
                    Loaded?.Invoke(typeof(BackgroundManager), new BackgroundLoadedEventArgs(set.Maps.First(), tex));
                else if (tex != UserInterface.MenuBackground)
                        Scheduler.RunAfter(() => tex.Dispose(), 5000);
            });
        }

        /// <summary>
        ///     Loads a background for an individual map.
        /// </summary>
        public static void Load(Map map)
        {
            // Dispose previous background.
            if (Background.Image != UserInterface.MenuBackground)
            {
                var bgImage = Background.Image;
                Scheduler.RunAfter(() =>  bgImage.Dispose(), 5000);
            }

            if (CurrentWorkerThread != null && (!CurrentWorkerThread.IsCanceled || !CurrentWorkerThread.IsCompleted))
                CurrentWorkerThread.Cancel();

            CurrentWorkerThread = LoadingThread.QueueWorkItem(delegate
            {
                var tex = LoadTexture(MapManager.GetBackgroundPath(map));

                PreviousBackground = Background;

                Background = new BackgroundImage(tex) {Alpha = 0};

                if (MapManager.Selected.Value == map)
                    Loaded?.Invoke(typeof(BackgroundManager), new BackgroundLoadedEventArgs(map, tex));
                else if (tex != UserInterface.MenuBackground)
                        Scheduler.RunAfter(() => tex.Dispose(), 5000);
            });
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