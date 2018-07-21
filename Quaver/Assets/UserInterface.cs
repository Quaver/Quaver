using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Helpers;
using Quaver.Resources;

namespace Quaver.Assets
{
    /// <summary>
    ///     All UI loaded into the game (Non-Skinnable)
    /// </summary>
    internal static class UserInterface
    {
        internal static Texture2D DiffSelectMask { get; set; }

        internal static Texture2D SetSelectMask { get; set; }

        internal static Texture2D BlankBox { get; set; }

        internal static Texture2D HollowBox { get; set; }

        internal static Texture2D BarCap { get; set; }

        internal static Texture2D BarCorner { get; set; }

        internal static Texture2D JudgementOverlay { get; set; }

        internal static List<Texture2D> TestSpritesheet { get; set; }

        internal static Texture2D UnknownAvatar { get; set; }

        internal static Texture2D YouAvatar { get; set; }

        internal static Texture2D MenuBackground { get; set; }

        internal static Texture2D QuaverLogoName { get; set; }

        internal static Texture2D SwanLogo { get; set; }

        internal static Texture2D MenuSinglePlayer { get; set; }

        internal static Texture2D MenuMultiplayer { get; set; }

        internal static Texture2D MenuCompetitive { get; set; }

        internal static Texture2D MenuLock { get; set; }

        internal static Texture2D MenuNews { get; set; }

        internal static Texture2D NotificationError { get; set; }

        internal static Texture2D NotificationWarning { get; set; }

        internal static Texture2D NotificationInfo { get; set; }

        internal static Texture2D NotificationSuccess { get; set; }

        /// <summary>
        ///     Loads all the ui elements into content
        /// </summary>
        internal static void Load()
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
