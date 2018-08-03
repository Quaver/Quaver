using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Resources;
using Wobble.Assets;
using Wobble.Graphics;

namespace Quaver.Assets
{
    public static class UserInterface
    {
        public static Texture2D DiffSelectMask { get; set; }
        public static Texture2D SetSelectMask { get; set; }
        public static Texture2D BlankBox { get; set; }
        public static Texture2D HollowBox { get; set; }
        public static Texture2D BarCap { get; set; }
        public static Texture2D BarCorner { get; set; }
        public static Texture2D JudgementOverlay { get; set; }
        public static List<Texture2D> TestSpritesheet { get; set; }
        public static Texture2D UnknownAvatar { get; set; }
        public static Texture2D YouAvatar { get; set; }
        public static Texture2D MenuBackground { get; set; }
        public static Texture2D QuaverLogoName { get; set; }
        public static Texture2D SwanLogo { get; set; }
        public static Texture2D MenuSinglePlayer { get; set; }  
        public static Texture2D MenuMultiplayer { get; set; }
        public static Texture2D MenuCompetitive { get; set; }
        public static Texture2D MenuLock { get; set; }
        public static Texture2D MenuNews { get; set; }
        public static Texture2D NotificationError { get; set; }
        public static Texture2D NotificationWarning { get; set; }
        public static Texture2D NotificationInfo { get; set; }
        public static Texture2D NotificationSuccess { get; set; }
        public static Texture2D QuaverLogo { get; set; }

        /// <summary>
        ///     Loads all the ui elements into content
        /// </summary>
        public static void Load()
        {
            DiffSelectMask = AssetLoader.LoadTexture2D(QuaverResources.diff_select_mask, ImageFormat.Png);
            SetSelectMask = AssetLoader.LoadTexture2D(QuaverResources.set_select_mask, ImageFormat.Png);
            BlankBox = AssetLoader.LoadTexture2D(QuaverResources.blank_box, ImageFormat.Png);
            HollowBox = AssetLoader.LoadTexture2D(QuaverResources.hollow_box, ImageFormat.Png);
            BarCap = AssetLoader.LoadTexture2D(QuaverResources.bar_cap, ImageFormat.Png);
            BarCorner = AssetLoader.LoadTexture2D(QuaverResources.bar_corner, ImageFormat.Png);
            JudgementOverlay = AssetLoader.LoadTexture2D(QuaverResources.judgement_overlay, ImageFormat.Png);
            TestSpritesheet = AssetLoader.LoadSpritesheetFromTexture(AssetLoader.LoadTexture2D(QuaverResources.test_spritesheet, ImageFormat.Png), 1, 8);
            UnknownAvatar = AssetLoader.LoadTexture2D(QuaverResources.unknown_avatar, ImageFormat.Png);
            YouAvatar = AssetLoader.LoadTexture2D(QuaverResources.you_avatar, ImageFormat.Png);
            MenuBackground = AssetLoader.LoadTexture2D(QuaverResources.menu_background, ImageFormat.Png);
            QuaverLogoName = AssetLoader.LoadTexture2D(QuaverResources.quaver_logo, ImageFormat.Png);
            SwanLogo = AssetLoader.LoadTexture2D(QuaverResources.swan_logo, ImageFormat.Png);
            MenuSinglePlayer = AssetLoader.LoadTexture2D(QuaverResources.menu_single_player, ImageFormat.Png);
            MenuLock = AssetLoader.LoadTexture2D(QuaverResources.menu_lock, ImageFormat.Png);
            MenuMultiplayer = AssetLoader.LoadTexture2D(QuaverResources.menu_multiplayer, ImageFormat.Png);
            MenuCompetitive = AssetLoader.LoadTexture2D(QuaverResources.menu_competitive, ImageFormat.Png);
            MenuNews = AssetLoader.LoadTexture2D(QuaverResources.menu_news, ImageFormat.Png);
            NotificationError = AssetLoader.LoadTexture2D(QuaverResources.notif_error, ImageFormat.Png);
            NotificationInfo = AssetLoader.LoadTexture2D(QuaverResources.notif_info, ImageFormat.Png);
            NotificationSuccess = AssetLoader.LoadTexture2D(QuaverResources.notif_success, ImageFormat.Png);
            NotificationWarning = AssetLoader.LoadTexture2D(QuaverResources.notif_warning, ImageFormat.Png);
            QuaverLogo = AssetLoader.LoadTexture2D(QuaverResources.quaver_logo_only, ImageFormat.Png);
        }
    }
}
