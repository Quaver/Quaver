using System;
using IniFileParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Config;

namespace Quaver.Shared.Skinning.Menus
{
    public class SkinMenuBorder : SkinMenu
    {
        public Color? BackgroundLineColor { get; private set; }

        public Color? ForegroundLineColor { get; private set; }

        public Color? ButtonTextColor { get; private set; }

        public Color? ButtonTextHoveredColor { get; private set; }

        public Texture2D Background { get; private set; }

        public SkinMenuBorder(SkinStore store, IniData config) : base(store, config)
        {
        }

        protected override void ReadConfig()
        {
            var ini = Config["MenuBorder"];

            var bgLineColor = ini["BackgroundLineColor"];
            ReadIndividualConfig(bgLineColor, () => BackgroundLineColor = ConfigHelper.ReadColor(Color.Transparent, bgLineColor));

            var fgLineColor = ini["ForegroundLineColor"];
            ReadIndividualConfig(fgLineColor, () => ForegroundLineColor = ConfigHelper.ReadColor(Color.Transparent, fgLineColor));

            var btnTextColor = ini["ButtonTextColor"];
            ReadIndividualConfig(btnTextColor, () => ButtonTextColor = ConfigHelper.ReadColor(Color.Transparent, btnTextColor));

            var btnTextHoveredColor = ini["ButtonTextHoveredColor"];
            ReadIndividualConfig(btnTextHoveredColor, () => ButtonTextHoveredColor = ConfigHelper.ReadColor(Color.Transparent, btnTextHoveredColor));
        }

        protected override void LoadElements()
        {
            Background = LoadSkinElement("MenuBorder", "menu-border-background.png");
        }
    }
}