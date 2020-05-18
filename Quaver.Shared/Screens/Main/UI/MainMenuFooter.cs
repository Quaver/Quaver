using System.Collections.Generic;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Menu.Border.Components.Buttons;
using Quaver.Shared.Screens.Main.UI.Jukebox;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Main.UI
{
    public class MainMenuFooter : MenuBorder
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public MainMenuFooter() : base(MenuBorderType.Footer, new List<Drawable>()
        {
            new MenuFooterButtonQuit(),
            new IconTextButtonOptions(),
            new IconTextButtonWiki()
        }, new List<Drawable>()
        {
            new IconTextButtonTwitter(),
            new IconTextButtonDiscord(),
            new IconTextButtonWebsite()
        })
        {
            // ReSharper disable once ObjectCreationAsStatement
            new FooterJukebox()
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
            };
        }
    }
}