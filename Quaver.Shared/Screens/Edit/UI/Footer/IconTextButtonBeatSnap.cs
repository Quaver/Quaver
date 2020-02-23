using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Wobble;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.Footer
{
    public class IconTextButtonBeatSnap : IconTextButton
    {
        public IconTextButtonBeatSnap(EditScreen screen) : base(FontAwesome.Get(FontAwesomeIcon.fa_sun),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Beat Snap",
            (sender, args) => screen?.ActivateRightClickOptions(new BeatSnapRightClickOptions(screen.BeatSnap)))
        {
        }
    }
}