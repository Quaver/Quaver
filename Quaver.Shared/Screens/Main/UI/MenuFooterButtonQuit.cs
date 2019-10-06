using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Selection.UI.Playlists.Dialogs.Create;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Main.UI
{
    public class MenuFooterButtonQuit : IconTextButton
    {
        public MenuFooterButtonQuit() : base(FontAwesome.Get(FontAwesomeIcon.fa_power_button_off),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Quit Game", (sender, args) =>
            {
                DialogManager.Show(new QuitDialog());
            })
        {
            BaseColor = ColorHelper.HexToColor("#e46060");
        }
    }
}