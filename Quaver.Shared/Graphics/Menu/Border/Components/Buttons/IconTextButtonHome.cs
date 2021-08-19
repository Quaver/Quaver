using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Main;
using Wobble;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Buttons
{
    public class IconTextButtonHome : MenuBorderScreenChangeButton
    {
        public override QuaverScreenType Screen { get; } = QuaverScreenType.Menu;

        public IconTextButtonHome() : base(FontAwesome.Get(FontAwesomeIcon.fa_home), FontManager.GetWobbleFont(Fonts.LatoBlack), "Home")
        {
        }

        public override void OnClick()
        {
            var game = (QuaverGame) GameBase.Game;
            game.CurrentScreen.Exit(() => new MainMenuScreen());
        }
    }
}