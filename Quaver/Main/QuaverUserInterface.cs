using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Helpers;
using Quaver.Resources;

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
        ///     judgement-overlay
        /// </summary>
        internal Texture2D JudgementOverlay { get; set; }

        internal List<Texture2D> TestSpritesheet { get; set; }

        /// <summary>
        ///     Loads all the ui elements into content
        /// </summary>
        public void LoadElementsAsContent()
        {
            DiffSelectMask = ResourceHelper.LoadTexture2DFromPng(QuaverResources.diff_select_mask);
            SetSelectMask = ResourceHelper.LoadTexture2DFromPng(QuaverResources.set_select_mask);
            BlankBox = ResourceHelper.LoadTexture2DFromPng(QuaverResources.blank_box);
            HollowBox = ResourceHelper.LoadTexture2DFromPng(QuaverResources.hollow_box);
            BarCap = ResourceHelper.LoadTexture2DFromPng(QuaverResources.bar_cap);
            BarCorner = ResourceHelper.LoadTexture2DFromPng(QuaverResources.bar_corner);
            JudgementOverlay = ResourceHelper.LoadTexture2DFromPng(QuaverResources.judgement_overlay);
            TestSpritesheet = GraphicsHelper.LoadSpritesheetFromTexture(ResourceHelper.LoadTexture2DFromPng(QuaverResources.test_spritesheet), 1, 8);
        }

        /// <summary>
        ///     Whenever the settings for window size is changed, call this method to update the window.
        /// </summary>
        /// <param name="newSize"></param>
        public static void UpdateWindow(Point newSize)
        {
            // NOTE: Unfinished
            GameBase.WindowRectangle = new DrawRectangle(0, 0, ConfigManager.WindowWidth.Value, ConfigManager.WindowHeight.Value);
            GameBase.MainRenderTarget = new RenderTarget2D(GameBase.GraphicsDevice, GameBase.GraphicsDevice.Viewport.Width, GameBase.GraphicsDevice.Viewport.Height);
            //Rectangle mainWindow = GraphicsDevice.PresentationParameters.Bounds;

            //Align letterboxed window
        }
    }
}
