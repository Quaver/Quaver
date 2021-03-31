using System;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.AutoMods
{
    public class EditorAutoModHeader : Sprite
    {
        private EditorAutoModPanel Panel { get; }

        private Sprite Gear { get; set; }

        private SpriteTextPlus Text { get; set; }

        public EditorAutoModHeader(EditorAutoModPanel panel)
        {
            Panel = panel;

            Image = UserInterface.AutoModPanelHeader;
            Size = new ScalableVector2(Panel.Width, Image.Height);

            CreateGearIcon();
            CreateText();
        }

        private void CreateGearIcon() => Gear = new Sprite
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            X = Height / 2,
            Size = new ScalableVector2(Height / 2, Height / 2),
            Image = UserInterface.AutoModHeaderGear
        };

        private void CreateText()
        {
            Text = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "AutoMod", 22)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Gear.X + Gear.Width + 10
            };
        }
    }
}