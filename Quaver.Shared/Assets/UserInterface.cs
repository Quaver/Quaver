/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Screens.Editor.UI.Hitsounds;
using Quaver.Shared.Screens.Select.UI.Leaderboard;
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
        public static Texture2D MenuBackgroundRaw { get; private set; }
        public static Texture2D Scrollbar { get; private set; }
        public static Texture2D LobbyCreateGame { get; private set; }
        public static Texture2D TeamBannerRed { get; private set; }
        public static Texture2D TeamBannerBlue { get; private set; }
        public static Texture2D BattleRoyaleGradient { get; private set; }
        public static Texture2D BattleRoyalePanel { get; private set; }
        public static Texture2D WaitingPanel { get; private set; }
        public static Texture2D WinsPanel { get; private set; }
        public static Texture2D ScoreboardBlueMirrored { get; private set; }
        public static Texture2D UserPanelFFA { get; private set; }
        public static Texture2D UserPanelRed { get; private set; }
        public static Texture2D UserPanelBlue { get; private set; }
        public static Texture2D UserPanelReferee{ get; private set; }
        public static Texture2D MapPanel { get; private set; }
        public static Texture2D FeedPanel { get; private set; }
        public static Texture2D MultiplayerSettingaPanel { get; private set; }
        public static Texture2D PlayerOptionsPanel { get; private set; }
        public static Texture2D ResultHeaderPanel { get; private set; }
        public static Texture2D ResultScorePanel { get; private set; }
        public static Texture2D ResultMultiplayerPanel { get; private set; }
        public static Texture2D ResultMultiplayerTeamPanel { get; private set; }
        public static Texture2D ResultRedTeam { get; private set; }
        public static Texture2D ResultBlueTeam { get; private set; }
        public static Texture2D ResultNoTeam { get; private set; }
        public static Texture2D JukeboxPanel { get; private set; }
        public static Texture2D PlayercardBackground { get; private set; }
        public static Texture2D MenuBackgroundNormal { get; private set; }
        public static Texture2D PlayercardCoverDefault { get; private set; }
        public static Texture2D DownloadSearchPanel { get; private set; }
        public static Texture2D DownloadContainer { get; private set; }
        public static Texture2D DownloadItem { get; private set; }
        public static Texture2D DownloadMapsetInfo { get; private set; }
        public static Texture2D SelectedMapset { get; private set; }
        public static Texture2D DeselectedMapset { get; private set; }
        public static Texture2D SelectSearchPanel { get; private set; }
        public static Texture2D LeaderboardScore { get; private set; }
        public static Texture2D LeaderboardPanel { get; private set; }

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
            MenuBackgroundRaw = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Menu/menu-background-raw.jpg"));
            Scrollbar = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Elements/scrollbar.png"));
            LobbyCreateGame = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Multiplayer/create-game.png"));
            TeamBannerRed = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Multiplayer/team-banner-red.png"));
            TeamBannerBlue = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Multiplayer/team-banner-blue.png"));
            BattleRoyaleGradient = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Multiplayer/battle-royale-gradient.png"));
            BattleRoyalePanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Multiplayer/battle-royale-panel.png"));
            WaitingPanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Multiplayer/waiting-panel.png"));
            WinsPanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Multiplayer/wins-panel.png"));
            ScoreboardBlueMirrored = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Multiplayer/scoreboard-blue-mirrored.png"));
            UserPanelFFA = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Multiplayer/user-panel-ffa.png"));
            UserPanelRed = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Multiplayer/user-panel-red.png"));
            UserPanelBlue = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Multiplayer/user-panel-blue.png"));
            UserPanelReferee = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Multiplayer/user-panel-referee.png"));
            MapPanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Multiplayer/map-panel.png"));
            FeedPanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Multiplayer/feed-panel.png"));
            MultiplayerSettingaPanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Multiplayer/settings-panel.png"));
            PlayerOptionsPanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Multiplayer/player-options-panel.png"));
            ResultHeaderPanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Results/result-header-panel.png"));
            ResultScorePanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Results/result-score-panel.png"));
            ResultMultiplayerPanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Results/result-multiplayer-panel.png"));
            ResultMultiplayerTeamPanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Results/result-multiplayer-team-panel.png"));
            ResultRedTeam = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Results/result-red-team.png"));
            ResultBlueTeam = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Results/result-blue-team.png"));
            ResultNoTeam = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Results/result-no-team.png"));
            JukeboxPanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/MainMenu/jukebox-panel.png"));
            PlayercardBackground = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Playercard/playercard-background.png"));
            MenuBackgroundNormal = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Menu/menu-background-normal.jpg"));
            PlayercardCoverDefault = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/Playercard/playercard-cover-default.png"));
            DownloadSearchPanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/download-search.png"));
            DownloadContainer = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/download-container.png"));
            DownloadItem = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/download-item.png"));
            DownloadMapsetInfo = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/mapset-info.png"));
            SelectedMapset = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/SongSelect/selected-mapset.png"));
            DeselectedMapset = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/SongSelect/deselected-mapset.png"));
            SelectSearchPanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/SongSelect/search-panel.png"));
            LeaderboardPanel = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/SongSelect/leaderboard-panel.png"));
            LeaderboardScore = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get("Quaver.Resources/Textures/UI/SongSelect/leaderboard-score.png"));
        }
    }
}
