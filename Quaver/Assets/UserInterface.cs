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
        public static Texture2D CloseChannelButton { get; set; }
        public static Texture2D SendMessageButton { get; set; }
        public static Texture2D ThumbnailSinglePlayer { get; set; }
        public static Texture2D ThumbnailCompetitive { get; set; }
        public static Texture2D ThumbnailCustomGames { get; set; }
        public static Texture2D ThumbnailEditor { get; set; }

        /// <summary>
        ///     Loads all the ui elements into content
        /// </summary>
        public static void Load()
        {
            DiffSelectMask = AssetLoader.LoadTexture2D(QuaverResources.diff_select_mask);
            SetSelectMask = AssetLoader.LoadTexture2D(QuaverResources.set_select_mask);
            BlankBox = AssetLoader.LoadTexture2D(QuaverResources.blank_box);
            HollowBox = AssetLoader.LoadTexture2D(QuaverResources.hollow_box);
            BarCap = AssetLoader.LoadTexture2D(QuaverResources.bar_cap);
            BarCorner = AssetLoader.LoadTexture2D(QuaverResources.bar_corner);
            JudgementOverlay = AssetLoader.LoadTexture2D(QuaverResources.judgement_overlay);
            TestSpritesheet = AssetLoader.LoadSpritesheetFromTexture(AssetLoader.LoadTexture2D(QuaverResources.test_spritesheet), 1, 8);
            UnknownAvatar = AssetLoader.LoadTexture2D(QuaverResources.unknown_avatar);
            YouAvatar = AssetLoader.LoadTexture2D(QuaverResources.you_avatar);
            MenuBackground = AssetLoader.LoadTexture2D(QuaverResources.menu_background);
            QuaverLogoName = AssetLoader.LoadTexture2D(QuaverResources.quaver_logo);
            SwanLogo = AssetLoader.LoadTexture2D(QuaverResources.swan_logo);
            MenuSinglePlayer = AssetLoader.LoadTexture2D(QuaverResources.menu_single_player);
            MenuLock = AssetLoader.LoadTexture2D(QuaverResources.menu_lock);
            MenuMultiplayer = AssetLoader.LoadTexture2D(QuaverResources.menu_multiplayer);
            MenuCompetitive = AssetLoader.LoadTexture2D(QuaverResources.menu_competitive);
            MenuNews = AssetLoader.LoadTexture2D(QuaverResources.menu_news);
            NotificationError = AssetLoader.LoadTexture2D(QuaverResources.notif_error);
            NotificationInfo = AssetLoader.LoadTexture2D(QuaverResources.notif_info);
            NotificationSuccess = AssetLoader.LoadTexture2D(QuaverResources.notif_success);
            NotificationWarning = AssetLoader.LoadTexture2D(QuaverResources.notif_warning);
            QuaverLogo = AssetLoader.LoadTexture2D(QuaverResources.quaver_logo_only);
            LeftButtonSquare = AssetLoader.LoadTexture2D(QuaverResources.left_button_square);
            RightButtonSquare = AssetLoader.LoadTexture2D(QuaverResources.right_button_square);
            RoundedSquare = AssetLoader.LoadTexture2D(QuaverResources.rounded_square);
            SelectBorder = AssetLoader.LoadTexture2D(QuaverResources.select_border);
            DiffSelectButton = AssetLoader.LoadTexture2D(QuaverResources.diff_select_button);
            SearchBar = AssetLoader.LoadTexture2D(QuaverResources.search_bar);
            SelectSearchBackground = AssetLoader.LoadTexture2D(QuaverResources.select_search_background);
            SelectInfoBackground = AssetLoader.LoadTexture2D(QuaverResources.select_info_background);
            RankedStatusFlag = AssetLoader.LoadTexture2D(QuaverResources.ranked_status_flag);
            MetadataContainer = AssetLoader.LoadTexture2D(QuaverResources.metadata_container);
            DiffButton = AssetLoader.LoadTexture2D(QuaverResources.diff_button);
            DiffButtonInactive = AssetLoader.LoadTexture2D(QuaverResources.diff_button_inactive);
            ConnectingBackground = AssetLoader.LoadTexture2D(QuaverResources.connecting_background);
            LoadingWheel = AssetLoader.LoadTexture2D(QuaverResources.loading_wheel);
            QuaverLogoFull = AssetLoader.LoadTexture2D(QuaverResources.quaver_logo_full);
            QuaverLogoStylish = AssetLoader.LoadTexture2D(QuaverResources.quaver_logo_stylish);
            UsernameSelectionBackground = AssetLoader.LoadTexture2D(QuaverResources.username_selection_background);
            UsernameSelectionTextbox = AssetLoader.LoadTexture2D(QuaverResources.username_selection_textbox);
            UsernameSelectionTextboxOverlay = AssetLoader.LoadTexture2D(QuaverResources.username_selection_textbox_overlay);
            CloseChannelButton = AssetLoader.LoadTexture2D(QuaverResources.close_channel_button);
            SendMessageButton = AssetLoader.LoadTexture2D(QuaverResources.send_message_button);
            ThumbnailSinglePlayer = AssetLoader.LoadTexture2D(QuaverResources.thumbnail_single_player);
            ThumbnailCompetitive = AssetLoader.LoadTexture2D(QuaverResources.thumbnail_competitive);
            ThumbnailCustomGames = AssetLoader.LoadTexture2D(QuaverResources.thumbnail_custom_games);
            ThumbnailEditor = AssetLoader.LoadTexture2D(QuaverResources.thumbnail_editor);
        }
    }
}
