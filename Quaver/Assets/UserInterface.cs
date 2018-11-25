using System.Data;
using Microsoft.Xna.Framework.Graphics;
using Wobble;
using Wobble.Assets;

namespace Quaver.Assets
{
    public static class UserInterface
    {
        public static Texture2D BlankBox { get; private set; }
        public static Texture2D UnknownAvatar { get; private set; }
        public static Texture2D YouAvatar { get; private set; }
        public static Texture2D MenuBackground { get; private set; }
        public static Texture2D NotificationError { get; private set; }
        public static Texture2D NotificationWarning { get; private set; }
        public static Texture2D NotificationInfo { get; private set; }
        public static Texture2D NotificationSuccess { get; private set; }
        public static Texture2D LeftButtonSquare { get; private set; }
        public static Texture2D RightButtonSquare { get; private set; }
        public static Texture2D RoundedSquare { get; private set; }
        public static Texture2D SelectBorder { get; private set; }
        public static Texture2D SearchBar { get; private set; }
        public static Texture2D SelectSearchBackground { get; private set; }
        public static Texture2D ThumbnailSinglePlayer { get; private set; }
        public static Texture2D ThumbnailCompetitive { get; private set; }
        public static Texture2D ThumbnailCustomGames { get; private set; }
        public static Texture2D ThumbnailEditor { get; private set; }
        public static Texture2D PlaycardBackground { get; private set; }
        public static Texture2D LoadingWheel { get; private set; }
        public static Texture2D StatusRanked { get; private set; }
        public static Texture2D StatusUnranked { get; private set; }
        public static Texture2D StatusNotSubmitted { get; private set; }
        public static Texture2D StatusDanCourse { get; private set; }
        public static Texture2D SelectButtonBackground { get; private set; }
        public static Texture2D HorizontalSelectorLeft { get; private set; }
        public static Texture2D HorizontalSelectorRight { get; private set; }
        public static Texture2D HorizontalSelectorMiddle { get; private set; }

        /// <summary>
        ///     Loads all the ui elements into content
        /// </summary>
        public static void Load()
        {
            BlankBox = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/blank-box.png"));
            UnknownAvatar = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/unknown-avatar.png"));
            YouAvatar = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/you-avatar.png"));
            MenuBackground = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Menu/menu-background.jpg"));
            NotificationError = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Notifications/notif-error.png"));
            NotificationWarning = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Notifications/notif-warning.png"));
            NotificationInfo = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Notifications/notif-info.png"));
            NotificationSuccess = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Notifications/notif-success.png"));
            LeftButtonSquare = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/left-button-square.png"));
            RightButtonSquare = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/right-button-square.png"));
            RoundedSquare = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/rounded-square.png"));
            SelectBorder = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Menu/select-border.png"));
            SelectSearchBackground = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/select-search-background.png"));
            SearchBar = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/search-bar.png"));
            ThumbnailSinglePlayer = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/MainMenu/thumbnail-single-player.jpg"));
            ThumbnailCompetitive = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/MainMenu/thumbnail-competitive.jpg"));
            ThumbnailCustomGames = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/MainMenu/thumbnail-custom-games.jpg"));
            ThumbnailEditor = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/MainMenu/thumbnail-editor.jpg"));
            PlaycardBackground = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Playercard/playercard-bg.png"));
            LoadingWheel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/loading-wheel.png"));
            StatusRanked = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/RankedStatus/status-ranked.png"));
            StatusUnranked = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/RankedStatus/status-unranked.png"));
            StatusNotSubmitted = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/RankedStatus/status-not-submitted.png"));
            StatusDanCourse = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/RankedStatus/status-dancourse.png"));
            SelectButtonBackground = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/SongSelect/select-button-background.png"));
            HorizontalSelectorLeft = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Elements/horizontal-selector-left.png"));
            HorizontalSelectorRight = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Elements/horizontal-selector-right.png"));
            HorizontalSelectorMiddle = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Elements/horizontal-selector-middle.png"));
        }
    }
}
