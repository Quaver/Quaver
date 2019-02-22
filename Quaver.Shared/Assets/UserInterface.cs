/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Screens.Editor.UI.Hitsounds;
using Wobble;
using Wobble.Assets;

namespace Quaver.Shared.Assets
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
        public static Texture2D NotificationErrorBg { get; private set; }
        public static Texture2D NotificationWarningBg { get; private set; }
        public static Texture2D NotificationInfoBg { get; private set; }
        public static Texture2D NotificationSuccessBg { get; private set; }
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
        public static Texture2D QuaverLogoFull { get; private set; }
        public static Texture2D MenuBackgroundBlurred { get; private set; }
        public static Texture2D QuaverLogoStylish { get; private set; }
        public static Texture2D QuaverLogoShadowed { get; private set; }
        public static Texture2D EditorToolSelect { get; private set; }
        public static Texture2D EditorToolMine { get; private set; }
        public static Texture2D EditorToolLongNote { get; private set; }
        public static Texture2D EditorToolNote { get; private set; }
        public static Texture2D EditorLayerPanel { get; private set; }
        public static Texture2D EditorEditLayerPanel { get; private set; }
        public static Texture2D EditorMetadataPanel { get; private set; }
        public static Texture2D EditorHitsoundsPanel { get; private set; }
        public static Texture2D EditorCompositionToolsPanel { get; private set; }
        public static Texture2D EditorDetailsPanel { get; private set; }
        public static Texture2D EditorHitObjectSelection { get; private set; }
        public static Texture2D EditorEditScrollVelocities { get; private set; }

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
            NotificationErrorBg = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Notifications/notif-error-bg.png"));
            NotificationWarningBg = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Notifications/notif-warning-bg.png"));
            NotificationInfoBg = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Notifications/notif-info-bg.png"));
            NotificationSuccessBg = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Notifications/notif-success-bg.png"));
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
            QuaverLogoFull = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/quaver-logo-full.png"));
            MenuBackgroundBlurred = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Menu/menu-background-blurred.jpg"));
            QuaverLogoStylish = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/quaver-logo-stylish.png"));
            QuaverLogoShadowed = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/quaver-logo-shadowed.png"));
            EditorToolSelect = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Editor/select.png"));
            EditorToolNote = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Editor/note.png"));
            EditorToolLongNote = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Editor/long-note.png"));
            EditorToolMine = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Editor/mine.png"));
            EditorLayerPanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Editor/layer-panel.png"));
            EditorEditLayerPanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Editor/edit-layer-panel.png"));
            EditorMetadataPanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Editor/metadata-panel.png"));
            EditorHitsoundsPanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Editor/hitsounds-panel.png"));
            EditorCompositionToolsPanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Editor/composition-tools-panel.png"));
            EditorDetailsPanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Editor/details-panel.png"));
            EditorHitObjectSelection = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Editor/hitobject-selection.png"));
            EditorEditScrollVelocities = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Editor/edit-scroll-velocities-panel.png"));
        }
    }
}
