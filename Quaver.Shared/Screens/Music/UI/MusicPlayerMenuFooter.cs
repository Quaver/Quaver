using System.Collections.Generic;
using Quaver.Shared.Graphics.Menu;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Graphics.Menu.Border.Components.Buttons;
using Quaver.Shared.Screens.Main.UI;
using Quaver.Shared.Screens.Main.UI.Jukebox;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Music.UI
{
    public class MusicPlayerMenuFooter : MenuBorder
    {
        public MusicPlayerMenuFooter(MusicPlayerScreen screen) : base(MenuBorderType.Footer, new List<Drawable>()
        {
            new IconTextButtonMusicPlayerBack(screen),
            new IconTextButtonOptions(),
            new IconTextButtonWiki()
        }, new List<Drawable>()
        {
            new IconTextButtonTwitter(),
            new IconTextButtonDiscord(),
            new IconTextButtonWebsite()
        })
        {
        }
    }
}