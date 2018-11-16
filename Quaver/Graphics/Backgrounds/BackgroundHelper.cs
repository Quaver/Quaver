using System;
using System.Drawing;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Database.Maps;
using Quaver.Assets;
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
        public static void Draw(GameTime gameTime) => Background?.Draw(gameTime);

        /// <summary>
        ///     Queues the background for the currently selected map to load.
        ///     Then calls "afterLoad();"
        /// </summary>
        /// <param name="afterLoad"></param>
        public static void QueueLoad(Action<Texture2D, Map, Texture2D> afterLoad) => ThreadScheduler.Run(() =>
        {
            var currentTex = Background.Image;
            Map = MapManager.Selected.Value;

            try
            {
                var path = MapManager.GetBackgroundPath(Map);

                if (File.Exists(path))
                {
                    var tex = AssetLoader.LoadTexture2DFromFile(path);
                    Background.Image = tex;
                    afterLoad(tex, Map, currentTex);
                }
                else
                {
                    var tex = UserInterface.MenuBackground;
                    Background.Image = tex;
                    afterLoad(tex, Map, currentTex);
                }
            }
            catch (Exception e)
            {
                Background.Image = UserInterface.MenuBackground;
                afterLoad(UserInterface.MenuBackground, Map, currentTex);
            }
        });
    }
}