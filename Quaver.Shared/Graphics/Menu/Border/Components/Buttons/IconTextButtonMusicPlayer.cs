using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Music;
using Quaver.Shared.Screens.Settings;
using Wobble;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Buttons
{
    public class IconTextButtonMusicPlayer : IconTextButton
    {
        public IconTextButtonMusicPlayer() : base(FontAwesome.Get(FontAwesomeIcon.fa_music_note_black_symbol),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Music Player", (sender, args) =>
            {
                var game = (QuaverGame) GameBase.Game;
                game.CurrentScreen.Exit(() => new MusicPlayerScreen());
            })
        {
        }
    }
}