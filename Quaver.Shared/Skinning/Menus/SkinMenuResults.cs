using IniFileParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Results.UI;

namespace Quaver.Shared.Skinning.Menus
{
    public class SkinMenuResults : SkinMenu
    {
        public Texture2D ResultsAvatarBorder { get; private set; }
        public Texture2D ResultsAvatarMask { get; private set; }
        public Texture2D ResultsBackgroundFilter { get; private set; }
        public Texture2D ResultsTabSelectorBackground { get; private set; }
        public Texture2D ResultsLabelBackground { get; private set; }
        public Texture2D ResultsMultiplayerTeamPanel { get; private set; }
        public Texture2D ResultsScoreContainerPanel { get; private set; }
        public Texture2D ResultsGraphContainerPanel { get; private set; }
        public Texture2D ResultsMultiplayerFFAPanel { get; private set; }
        public Texture2D ResultsBackground { get; private set; }
        public ResultsBackgroundType ResultsBackgroundType { get; private set; }
        public float? ResultsBackgroundFilterAlpha { get; private set; }

        public SkinMenuResults(SkinStore store, IniData config) : base(store, config)
        {
        }

        protected override void ReadConfig()
        {
            var ini = Config["Results"];

            var resultsBackgroundType = ini["ResultsBackgroundType"];
            ReadIndividualConfig(resultsBackgroundType, () => ResultsBackgroundType = ConfigHelper.ReadEnum(ResultsBackgroundType.Header, resultsBackgroundType));

            var resultsBackgroundFilterAlpha = ini["ResultsBackgroundFilterAlpha"];
            ReadIndividualConfig(resultsBackgroundFilterAlpha, () => ResultsBackgroundFilterAlpha = ConfigHelper.ReadFloat(0f, resultsBackgroundFilterAlpha));
        }

        protected override void LoadElements()
        {
            const string folder = "Results";

            ResultsAvatarBorder = LoadSkinElement(folder, "avatar-border.png");
            ResultsAvatarMask = LoadSkinElement(folder, "avatar-mask.png");
            ResultsBackgroundFilter = LoadSkinElement(folder, "background-filter.png");
            ResultsTabSelectorBackground = LoadSkinElement(folder, "tab-selector-background.png");
            ResultsLabelBackground = LoadSkinElement(folder, "label-bg.png");
            ResultsMultiplayerTeamPanel = LoadSkinElement(folder, "multiplayer-team-panel.png");
            ResultsScoreContainerPanel = LoadSkinElement(folder, "score-container-panel.png");
            ResultsGraphContainerPanel = LoadSkinElement(folder, "graph-container-panel.png");
            ResultsMultiplayerFFAPanel = LoadSkinElement(folder, "multiplayer-ffa-panel.png");
            ResultsBackground = LoadSkinElement(folder, "background.png");
        }
    }
}