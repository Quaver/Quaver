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
        
        internal Texture2D UnknownAvatar { get; set; }
        
        internal Texture2D YouAvatar { get; set; }

        internal Texture2D MenuBackground { get; set; }

        internal Texture2D QuaverLogoName { get; set; }

        internal Texture2D SwanLogo { get; set; }

        internal Texture2D MenuSinglePlayer { get; set; }

        internal Texture2D MenuMultiplayer { get; set; }
        
        internal Texture2D MenuCompetitive { get; set; }
        
        internal Texture2D MenuLock { get; set; }

        internal Texture2D MenuNews { get; set; }

        internal Texture2D NotificationError { get; set; }
        internal Texture2D NotificationWarning { get; set; }
        internal Texture2D NotificationInfo { get; set; }
        internal Texture2D NotificationSuccess { get; set; }

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
            UnknownAvatar = ResourceHelper.LoadTexture2DFromPng(QuaverResources.unknown_avatar);
            YouAvatar = ResourceHelper.LoadTexture2DFromPng(QuaverResources.you_avatar);
            MenuBackground = ResourceHelper.LoadTexture2DFromPng(QuaverResources.menu_background);
            QuaverLogoName = ResourceHelper.LoadTexture2DFromPng(QuaverResources.quaver_logo);
            SwanLogo = ResourceHelper.LoadTexture2DFromPng(QuaverResources.swan_logo);
            MenuSinglePlayer = ResourceHelper.LoadTexture2DFromPng(QuaverResources.menu_single_player);
            MenuLock = ResourceHelper.LoadTexture2DFromPng(QuaverResources.menu_lock);
            MenuMultiplayer = ResourceHelper.LoadTexture2DFromPng(QuaverResources.menu_multiplayer);
            MenuCompetitive = ResourceHelper.LoadTexture2DFromPng(QuaverResources.menu_competitive);
            MenuNews = ResourceHelper.LoadTexture2DFromPng(QuaverResources.menu_news);
            NotificationError = ResourceHelper.LoadTexture2DFromPng(QuaverResources.notif_error);
            NotificationInfo = ResourceHelper.LoadTexture2DFromPng(QuaverResources.notif_info);
            NotificationSuccess = ResourceHelper.LoadTexture2DFromPng(QuaverResources.notif_success);
            NotificationWarning = ResourceHelper.LoadTexture2DFromPng(QuaverResources.notif_warning);
        }
    }
}
