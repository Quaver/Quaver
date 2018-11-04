using System;
using System.Drawing;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Database.Maps;
using Quaver.Resources;
using Quaver.Scheduling;
using WebSocketSharp;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.UI;
using Wobble.Logging;
using Logger = Wobble.Logging.Logger;

namespace Quaver.Graphics.Backgrounds
{
    public static class BackgroundHelper
    {
        /// <summary>
        ///     The current background that we're working with.
        /// </summary>
        public static BackgroundImage Background { get; private set; }

        /// <summary>
        ///     The individual map this background is for.
        /// </summary>
        public static Map Map { get; private set; }

        /// <summary>
        ///     Determines if the background has been blurred yet.
        /// </summary>
        public static bool HasBlurred { get; set; } = true;

        /// <summary>
        ///     Initializes the background helper for the entire game.
        /// </summary>
        public static void Initialize() => Background = new BackgroundImage(UserInterface.MenuBackground, 0);

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
            /*if (!HasBlurred)
            {
                try
                {
                    GameBase.Game.SpriteBatch.End();
                }
                catch (Exception e)
                {

                }

                var oldTex = Background.Image;

                var blur = new GaussianBlur(0.75f);
                var newTex = blur.PerformGaussianBlur(oldTex);

                Background.Image = newTex;
                HasBlurred = true;
            }*/

            Background?.Draw(gameTime);
        }

        /// <summary>
        ///     Queues the background for the currently selected map to load.
        ///     Then calls "afterLoad();"
        /// </summary>
        /// <param name="afterLoad"></param>
        public static void QueueLoad(Action<Texture2D, Map, Texture2D> afterLoad) => Scheduler.RunThread(() =>
        {
            afterLoad(UserInterface.MenuBackground, Map, Background.Image);

            return;
            var currentTex = Background.Image;
            Map = MapManager.Selected.Value;

            try
            {
                var path = MapManager.GetBackgroundPath(Map);
                var tex = AssetLoader.LoadTexture2DFromFile(path);
                afterLoad(tex, Map, currentTex);
            }
            catch (Exception e)
            {
                afterLoad(UserInterface.MenuBackground, Map, currentTex);
            }
        });
    }
}