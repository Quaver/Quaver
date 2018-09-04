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
        public static Texture2D LeftButtonSquare { get; set; }
        public static Texture2D RightButtonSquare { get; set; }
        public static Texture2D RoundedSquare { get; set; }
        public static Texture2D SelectBorder { get; set; }
        public static Texture2D DiffSelectButton { get; set; }
        public static Texture2D SearchBar { get; set; }
        public static Texture2D SelectSearchBackground { get; set; }
        public static Texture2D SelectInfoBackground { get; set; }
        public static Texture2D RankedStatusFlag { get; set; }
        public static Texture2D MetadataContainer { get; set; }
        public static Texture2D DiffButton { get; set; }
        public static Texture2D DiffButtonInactive { get; set; }
        public static Texture2D ConnectingBackground { get; set; }
        public static Texture2D LoadingWheel { get; set; }
        public static Texture2D QuaverLogoFull { get; set; }
        public static Texture2D QuaverLogoStylish { get; set; }
        public static Texture2D UsernameSelectionBackground { get; set; }
        public static Texture2D UsernameSelectionTextbox { get; set; }
        public static Texture2D UsernameSelectionTextboxOverlay { get; set; }

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
            LeftButtonSquare = AssetLoader.LoadTexture2D(QuaverResources.left_button_square, ImageFormat.Png);
            RightButtonSquare = AssetLoader.LoadTexture2D(QuaverResources.right_button_square, ImageFormat.Png);
            RoundedSquare = AssetLoader.LoadTexture2D(QuaverResources.rounded_square, ImageFormat.Png);
            SelectBorder = AssetLoader.LoadTexture2D(QuaverResources.select_border, ImageFormat.Png);
            DiffSelectButton = AssetLoader.LoadTexture2D(QuaverResources.diff_select_button, ImageFormat.Png);
            SearchBar = AssetLoader.LoadTexture2D(QuaverResources.search_bar, ImageFormat.Png);
            SelectSearchBackground = AssetLoader.LoadTexture2D(QuaverResources.select_search_background, ImageFormat.Png);
            SelectInfoBackground = AssetLoader.LoadTexture2D(QuaverResources.select_info_background, ImageFormat.Png);
            RankedStatusFlag = AssetLoader.LoadTexture2D(QuaverResources.ranked_status_flag, ImageFormat.Png);
            MetadataContainer = AssetLoader.LoadTexture2D(QuaverResources.metadata_container, ImageFormat.Png);
            DiffButton = AssetLoader.LoadTexture2D(QuaverResources.diff_button, ImageFormat.Png);
            DiffButtonInactive = AssetLoader.LoadTexture2D(QuaverResources.diff_button_inactive, ImageFormat.Png);
            ConnectingBackground = AssetLoader.LoadTexture2D(QuaverResources.connecting_background, ImageFormat.Jpeg);
            LoadingWheel = AssetLoader.LoadTexture2D(QuaverResources.loading_wheel, ImageFormat.Png);
            QuaverLogoFull = AssetLoader.LoadTexture2D(QuaverResources.quaver_logo_full, ImageFormat.Png);
            QuaverLogoStylish = AssetLoader.LoadTexture2D(QuaverResources.quaver_logo_stylish, ImageFormat.Png);
            UsernameSelectionBackground = AssetLoader.LoadTexture2D(QuaverResources.username_selection_background, ImageFormat.Jpeg);
            UsernameSelectionTextbox = AssetLoader.LoadTexture2D(QuaverResources.username_selection_textbox, ImageFormat.Png);
            UsernameSelectionTextboxOverlay = AssetLoader.LoadTexture2D(QuaverResources.username_selection_textbox_overlay, ImageFormat.Png);
        }
    }
}
