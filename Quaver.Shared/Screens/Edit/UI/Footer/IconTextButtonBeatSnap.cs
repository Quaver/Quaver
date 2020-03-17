using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Helpers;
using Wobble;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.Footer
{
    public class IconTextButtonBeatSnap : IconTextButton
    {
        public IconTextButtonBeatSnap(EditScreen screen) : base(FontAwesome.Get(FontAwesomeIcon.fa_sun),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Beat Snap",
            (sender, args) => screen?.ActivateRightClickOptions(new BeatSnapRightClickOptions(screen.BeatSnap, screen.AvailableBeatSnaps)))
        {
            var tooltip = new Tooltip("Change the current beat snap divisor.\n" +
                                      "Hotkeys: CTRL + Up/Down/Scroll Wheel", ColorHelper.HexToColor("#808080"));

            Hovered += (sender, args) => screen?.ActivateTooltip(tooltip);
            LeftHover += (sender, args) => screen?.DeactivateTooltip();
        }
    }
}