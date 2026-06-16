using Quaver.Shared.Assets;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.SkinEditor;
using Wobble;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Buttons
{
    public class IconTextButtonSkins : MenuBorderScreenChangeButton
    {
        public override QuaverScreenType Screen { get; } = QuaverScreenType.SkinEditor;

        public IconTextButtonSkins() : base(FontAwesome.Get(FontAwesomeIcon.fa_pencil),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Skins")
        {
        }

        public override void OnClick()
        {
            var game = (QuaverGame)GameBase.Game;
            game.CurrentScreen.Exit(() => new SkinEditorScreen());
        }
    }
}
