using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Borders.Footer
{
    public class IconTextButtonPlay : IconTextButton
    {
        public IconTextButtonPlay(SelectionScreen screen) : base(FontAwesome.Get(FontAwesomeIcon.fa_play_button),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Play", (sender, args) =>
            {
                switch (screen.ActiveScrollContainer.Value)
                {
                    case SelectScrollContainerType.Mapsets:
                        screen.ActiveScrollContainer.Value = SelectScrollContainerType.Maps;
                        break;
                    case SelectScrollContainerType.Maps:
                        screen.ExitToGameplay();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            })
        {
        }
    }
}