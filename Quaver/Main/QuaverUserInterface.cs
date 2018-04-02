using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using Quaver.Graphics.Base;

namespace Quaver.Main
{
    /// <summary>
    ///     All UI loaded into the game (Non-Skinnable)
    /// </summary>
    internal class QuaverUserInterface
    {
        /// <summary>
        ///     diff-select-mask 
        /// </summary>
        internal Texture2D DiffSelectMask { get; set; }

        /// <summary>
        ///     set-select-mask 
        /// </summary>
        internal Texture2D SetSelectMask { get; set; }

        /// <summary>
        ///     blank-box
        /// </summary>
        internal Texture2D BlankBox { get; set; }

        /// <summary>
        ///     hollow-box
        /// </summary>
        internal Texture2D HollowBox { get; set; }

        /// <summary>
        ///     bar-cap
        /// </summary>
        internal Texture2D BarCap { get; set; }

        /// <summary>
        ///     bar-corner
        /// </summary>
        internal Texture2D BarCorner { get; set; }

        /// <summary>
        ///     Loads all the ui elements into content
        /// </summary>
        public void LoadElementsAsContent()
        {
            DiffSelectMask = GameBase.Content.Load<Texture2D>("main-diff-select-mask");
            SetSelectMask = GameBase.Content.Load<Texture2D>("main-set-select-mask");
            BlankBox = GameBase.Content.Load<Texture2D>("main-blank-box");
            HollowBox = GameBase.Content.Load<Texture2D>("main-hollow-box");
            BarCap = GameBase.Content.Load<Texture2D>("main-bar-cap");
            BarCorner = GameBase.Content.Load<Texture2D>("main-bar-corner");
        }

        /// <summary>
        ///     Whenever the settings for window size is changed, call this method to update the window.
        /// </summary>
        /// <param name="newSize"></param>
        public static void UpdateWindow(Point newSize)
        {
            // NOTE: Unfinished
            GameBase.WindowRectangle = new DrawRectangle(0, 0, ConfigManager.WindowWidth, ConfigManager.WindowHeight);
            GameBase.MainRenderTarget = new RenderTarget2D(GameBase.GraphicsDevice, GameBase.GraphicsDevice.Viewport.Width, GameBase.GraphicsDevice.Viewport.Height);
            //Rectangle mainWindow = GraphicsDevice.PresentationParameters.Bounds;

            //Align letterboxed window
            //Window = GraphicsHelper.DrawRect(Alignment.MidCenter, Window, mainWindow);
            GameBase.WindowUIScale = GameBase.WindowRectangle.Y / GameBase.ReferenceResolution.Y;
        }
    }
}
