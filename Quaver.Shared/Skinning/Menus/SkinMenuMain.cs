using IniFileParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Config;

namespace Quaver.Shared.Skinning.Menus
{
    public class SkinMenuMain : SkinMenu
    {
        public Texture2D Background { get; private set; }

        public Texture2D NavigationButton { get; private set; }

        public Texture2D NavigationButtonSelected { get; private set; }

        public Texture2D NavigationButtonHovered { get; private set; }

        public float? NavigationButtonHoveredAlpha { get; private set; }

        public Texture2D TipPanel { get; private set; }

        public Texture2D NewsPanel { get; private set; }

        public Texture2D JukeboxOverlay { get; private set; }

        public Texture2D NoteVisualizer { get; private set; }

        public float? NoteVisualizerOpacity { get; private set; }

        public Color? AudioVisualizerColor { get; private set; }

        public float? AudioVisualizerOpacity { get; private set; }

        public Color? NavigationButtonTextColor { get; private set; }

        public Color? NavigationQuitButtonTextColor { get; private set; }

        public Color? TipTitleColor { get; private set; }

        public Color? TipTextColor { get; private set; }

        public Color? NewsTitleColor { get; private set; }

        public Color? NewsDateColor { get; private set; }

        public Color? NewsTextColor { get; private set; }

        public Color? JukeboxProgressBarColor { get; private set; }

        public SkinMenuMain(SkinStore store, IniData config) : base(store, config)
        {
        }

        protected override void ReadConfig()
        {
            var ini = Config["MainMenu"];

            var navigationButtonHoveredAlpha = ini["NavigationButtonHoveredAlpha"];
            ReadIndividualConfig(navigationButtonHoveredAlpha, () => NavigationButtonHoveredAlpha = ConfigHelper.ReadFloat(0.35f, navigationButtonHoveredAlpha));

            var noteVisualizerOpacity = ini["NoteVisualizerOpacity"];
            ReadIndividualConfig(noteVisualizerOpacity, () => NoteVisualizerOpacity = ConfigHelper.ReadFloat(0, noteVisualizerOpacity));

            var audioVisualizerColor = ini["AudioVisualizerColor"];
            ReadIndividualConfig(audioVisualizerColor, () => AudioVisualizerColor = ConfigHelper.ReadColor(Color.Transparent, audioVisualizerColor));

            var audioVisualizerOpacity = ini["AudioVisualizerOpacity"];
            ReadIndividualConfig(audioVisualizerOpacity, () => AudioVisualizerOpacity = ConfigHelper.ReadFloat(0, audioVisualizerOpacity));

            var navBtnTextColor = ini["NavigationButtonTextColor"];
            ReadIndividualConfig(navBtnTextColor, () => NavigationButtonTextColor = ConfigHelper.ReadColor(Color.Transparent, navBtnTextColor));

            var navQuitBtnTextColor = ini["NavigationQuitButtonTextColor"];
            ReadIndividualConfig(navQuitBtnTextColor, () => NavigationQuitButtonTextColor = ConfigHelper.ReadColor(Color.Transparent, navQuitBtnTextColor));

            var tipTitleColor = ini["TipTitleColor"];
            ReadIndividualConfig(tipTitleColor, () => TipTitleColor = ConfigHelper.ReadColor(Color.Transparent, tipTitleColor));

            var tipTextColor = ini["TipTextColor"];
            ReadIndividualConfig(tipTextColor, () => TipTextColor = ConfigHelper.ReadColor(Color.Transparent, tipTextColor));

            var newsTitleColor = ini["NewsTitleColor"];
            ReadIndividualConfig(newsTitleColor, () => NewsTitleColor = ConfigHelper.ReadColor(Color.Transparent, newsTitleColor));

            var newsDateColor = ini["NewsDateColor"];
            ReadIndividualConfig(newsDateColor, () => NewsDateColor = ConfigHelper.ReadColor(Color.Transparent, newsDateColor));

            var newsTextColor = ini["NewsTextColor"];
            ReadIndividualConfig(newsTextColor, () => NewsTextColor = ConfigHelper.ReadColor(Color.Transparent, newsTextColor));

            var jukeboxProgressBarColor = ini["JukeboxProgressBarColor"];
            ReadIndividualConfig(jukeboxProgressBarColor, () => JukeboxProgressBarColor = ConfigHelper.ReadColor(Color.Transparent, jukeboxProgressBarColor));
        }

        protected override void LoadElements()
        {
            const string folder = "MainMenu";

            Background = LoadSkinElement(folder, "menu-background.png");
            NavigationButton = LoadSkinElement(folder, "navigation-button.png");
            NavigationButtonSelected = LoadSkinElement(folder, "navigation-button-selected.png");
            NavigationButtonHovered = LoadSkinElement(folder, "navigation-button-hovered.png");
            TipPanel = LoadSkinElement(folder, "tip-panel.png");
            NewsPanel = LoadSkinElement(folder, "news-panel.png");
            JukeboxOverlay = LoadSkinElement(folder, "jukebox-overlay.png");
            NoteVisualizer = LoadSkinElement(folder, "note-visualizer.png");
        }
    }
}