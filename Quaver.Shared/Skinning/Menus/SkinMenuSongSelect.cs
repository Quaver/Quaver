using IniFileParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Config;
using Wobble.Graphics;

namespace Quaver.Shared.Skinning.Menus
{
    public class SkinMenuSongSelect : SkinMenu
    {
        public bool DisplayMapBackground { get; private set; }

        public byte? MapBackgroundBrightness { get; private set; }

        public Color? MapsetPanelSongTitleColor { get; private set; }

        public Color? MapsetPanelSongArtistColor { get; private set; }

        public Color? MapsetPanelCreatorColor { get; private set; }

        public Color? MapsetPanelByColor { get; private set; }

        public ScalableVector2? MapsetPanelBannerSize { get; private set; }

        public float? MapsetPanelHoveringAlpha { get; private set; }

        public Color? LeaderboardScoreColorEven { get; private set; }

        public Color? LeaderboardScoreColorOdd { get; private set; }

        public Color? LeaderboardScoreRankColor { get; private set; }

        public Color? LeaderboardScoreRatingColor { get; private set; }

        public Color? LeaderboardScoreAccuracyColor { get; private set; }

        public Color? LeaderboardScoreUsernameSelfColor { get; private set; }

        public Color? LeaderboardScoreUsernameOtherColor { get; private set; }

        public Color? LeaderboardTitleColor { get; private set; }

        public Color? LeaderboardRankingTitleColor { get; private set; }

        public Color? LeaderboardDropdownColor { get; private set; }

        public Color? LeaderboardStatusTextColor { get; private set; }

        public Color? PersonalBestTitleColor { get; private set; }

        public Color? NoPersonalBestColor { get; private set; }

        public Color? PersonalBestTrophyColor { get; private set; }

        public Color? PersonalBestRankColor { get; private set; }

        #region MAPSET

        public Texture2D MapsetSelected { get; private set; }

        public Texture2D MapsetDeselected { get; private set; }

        public Texture2D MapsetHovered { get; private set; }

#endregion

#region GAME_MODE

        public Texture2D GameMode4K { get; private set; }

        public Texture2D GameMode7K { get; private set; }

        public Texture2D GameMode4K7K { get; private set; }

#endregion

#region  RANKED_STATUS

        public Texture2D StatusNotSubmitted { get; private set; }

        public Texture2D StatusUnranked { get; private set; }

        public Texture2D StatusRanked { get; private set; }

        public Texture2D StatusOsu { get; private set; }

        public Texture2D StatusStepmania { get; private set; }

 #endregion

#region LEADERBOARD

        public Texture2D LeaderboardPanel { get; private set; }

        public Texture2D PersonalBestPanel { get; private set; }

#endregion

        public SkinMenuSongSelect(SkinStore store, IniData config) : base(store, config)
        {
        }

        protected override void ReadConfig()
        {
            var ini = Config["SongSelect"];

            var displayMapBackground = ini["DisplayMapBackground"];
            ReadIndividualConfig(displayMapBackground, () => DisplayMapBackground = ConfigHelper.ReadBool(false, displayMapBackground));

            var mapBackgroundBrightness = ini["MapBackgroundBrightness"];
            ReadIndividualConfig(mapBackgroundBrightness, () => MapBackgroundBrightness = ConfigHelper.ReadByte(0, mapBackgroundBrightness));

            var mapsetPanelSongTitleColor = ini["MapsetPanelSongTitleColor"];
            ReadIndividualConfig(mapsetPanelSongTitleColor, () => MapsetPanelSongTitleColor = ConfigHelper.ReadColor(Color.Transparent, mapsetPanelSongTitleColor));

            var mapsetPanelSongArtistColor = ini["MapsetPanelSongArtistColor"];
            ReadIndividualConfig(mapsetPanelSongArtistColor, () => MapsetPanelSongArtistColor = ConfigHelper.ReadColor(Color.Transparent, mapsetPanelSongArtistColor));

            var mapsetPanelCreatorColor = ini["MapsetPanelCreatorColor"];
            ReadIndividualConfig(mapsetPanelCreatorColor, () => MapsetPanelCreatorColor = ConfigHelper.ReadColor(Color.Transparent, mapsetPanelCreatorColor));

            var mapsetPanelByColor = ini["MapsetPanelByColor"];
            ReadIndividualConfig(mapsetPanelByColor, () => MapsetPanelByColor = ConfigHelper.ReadColor(Color.Transparent, mapsetPanelByColor));

            var mapsetPanelBannerSize = ini["MapsetPanelBannerSize"];
            ReadIndividualConfig(mapsetPanelBannerSize, () => MapsetPanelBannerSize = ConfigHelper.ReadVector2(new ScalableVector2(0, 0), mapsetPanelBannerSize));

            var mapsetPanelHoveringAlpha = ini["MapsetPanelHoveringAlpha"];
            ReadIndividualConfig(mapsetPanelHoveringAlpha, () => MapsetPanelHoveringAlpha = ConfigHelper.ReadFloat(0.35f, mapsetPanelHoveringAlpha));

            var leaderboardScoreColorEven = ini["LeaderboardScoreColorEven"];
            ReadIndividualConfig(leaderboardScoreColorEven, () => LeaderboardScoreColorEven = ConfigHelper.ReadColor(Color.Transparent, leaderboardScoreColorEven));

            var leaderboardScoreColorOdd = ini["LeaderboardScoreColorOdd"];
            ReadIndividualConfig(leaderboardScoreColorOdd, () => LeaderboardScoreColorOdd = ConfigHelper.ReadColor(Color.Transparent, leaderboardScoreColorOdd));

            var leaderboardScoreRankColor = ini["LeaderboardScoreRankColor"];
            ReadIndividualConfig(leaderboardScoreRankColor, () => LeaderboardScoreRankColor = ConfigHelper.ReadColor(Color.Transparent, leaderboardScoreRankColor));

            var leaderboardScoreRatingColor = ini["LeaderboardScoreRatingColor"];
            ReadIndividualConfig(leaderboardScoreRatingColor, () => LeaderboardScoreRatingColor = ConfigHelper.ReadColor(Color.Transparent, leaderboardScoreRatingColor));

            var leaderboardScoreAccuracyColor = ini["LeaderboardScoreAccuracyColor"];
            ReadIndividualConfig(leaderboardScoreAccuracyColor, () => LeaderboardScoreAccuracyColor = ConfigHelper.ReadColor(Color.Transparent, leaderboardScoreAccuracyColor));

            var leaderboardScoreUsernameSelfColor = ini["LeaderboardScoreUsernameSelfColor"];
            ReadIndividualConfig(leaderboardScoreUsernameSelfColor, () => LeaderboardScoreUsernameSelfColor = ConfigHelper.ReadColor(Color.Transparent, leaderboardScoreUsernameSelfColor));

            var leaderboardScoreUsernameOtherColor = ini["LeaderboardScoreUsernameOtherColor"];
            ReadIndividualConfig(leaderboardScoreUsernameOtherColor, () => LeaderboardScoreUsernameOtherColor = ConfigHelper.ReadColor(Color.Transparent, leaderboardScoreUsernameOtherColor));

            var leaderboardTitleColor = ini["LeaderboardTitleColor"];
            ReadIndividualConfig(leaderboardTitleColor, () => LeaderboardTitleColor = ConfigHelper.ReadColor(Color.Transparent, leaderboardTitleColor));

            var leaderboardRankingTitleColor = ini["LeaderboardRankingTitleColor"];
            ReadIndividualConfig(leaderboardRankingTitleColor, () => LeaderboardRankingTitleColor = ConfigHelper.ReadColor(Color.Transparent, leaderboardRankingTitleColor));

            var leaderboardDropdownColor = ini["LeaderboardDropdownColor"];
            ReadIndividualConfig(leaderboardDropdownColor, () => LeaderboardDropdownColor = ConfigHelper.ReadColor(Color.Transparent, leaderboardDropdownColor));

            var personalBestTitleColor = ini["PersonalBestTitleColor"];
            ReadIndividualConfig(personalBestTitleColor, () => PersonalBestTitleColor = ConfigHelper.ReadColor(Color.Transparent, personalBestTitleColor));

            var noPersonalBestColor = ini["NoPersonalBestColor"];
            ReadIndividualConfig(noPersonalBestColor, () => NoPersonalBestColor = ConfigHelper.ReadColor(Color.Transparent, noPersonalBestColor));

            var leaderboardStatusTextColor = ini["LeaderboardStatusTextColor"];
            ReadIndividualConfig(leaderboardStatusTextColor, () => LeaderboardStatusTextColor = ConfigHelper.ReadColor(Color.Transparent, leaderboardStatusTextColor));

            var personalBestTrophyColor = ini["PersonalBestTrophyColor"];
            ReadIndividualConfig(personalBestTrophyColor, () => PersonalBestTrophyColor = ConfigHelper.ReadColor(Color.Transparent, personalBestTrophyColor));

            var personalBestRankColor = ini["PersonalBestRankColor"];
            ReadIndividualConfig(personalBestRankColor, () => PersonalBestRankColor = ConfigHelper.ReadColor(Color.Transparent, personalBestRankColor));
        }

        protected override void LoadElements()
        {
            const string folder = "SongSelect";

            MapsetSelected = LoadSkinElement(folder, "mapset-selected.png");
            MapsetDeselected = LoadSkinElement(folder, "mapset-deselected.png");
            MapsetHovered = LoadSkinElement(folder, "mapset-hovered.png");
            GameMode4K = LoadSkinElement(folder, "game-mode-4k.png");
            GameMode7K = LoadSkinElement(folder, "game-mode-7k.png");
            GameMode4K7K = LoadSkinElement(folder, "game-mode-4k7k.png");
            StatusNotSubmitted = LoadSkinElement(folder, "status-notsubmitted.png");
            StatusUnranked = LoadSkinElement(folder, "status-unranked.png");
            StatusRanked = LoadSkinElement(folder, "status-ranked.png");
            StatusOsu = LoadSkinElement(folder, "status-osu.png");
            StatusStepmania = LoadSkinElement(folder, "status-sm.png");
            LeaderboardPanel = LoadSkinElement(folder, "leaderboard-panel.png");
            PersonalBestPanel = LoadSkinElement(folder, "personalbest-panel.png");
        }
    }
}