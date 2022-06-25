using Quaver.Shared.Assets;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Music;
using Wobble;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Buttons
{
    public class IconTextButtonMusicPlayer : MenuBorderScreenChangeButton
    {
        public override QuaverScreenType Screen { get; } = QuaverScreenType.Music;

        public IconTextButtonMusicPlayer() : base(FontAwesome.Get(FontAwesomeIcon.fa_music_note_black_symbol),
            FontManager.GetWobbleFont(Fonts.LatoBlack), "Jukebox")
        {
        }

        public override void OnClick()
        {
            var game = (QuaverGame) GameBase.Game;
            game.CurrentScreen.Exit(() => new MusicPlayerScreen());
        }
    }
}