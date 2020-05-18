using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Borders.Footer
{
    public class IconTextButtonRandom : IconTextButton
    {
        public IconTextButtonRandom(SelectionScreen screen) : base(FontAwesome.Get(FontAwesomeIcon.fa_exchange_arrows),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Random", (sender, args) =>
            {
                screen.SelectRandomMap();
            })
        {
        }
    }
}