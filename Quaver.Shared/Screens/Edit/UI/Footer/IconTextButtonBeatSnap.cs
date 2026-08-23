using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Input;
using Wobble;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.Footer
{
    public class IconTextButtonBeatSnap : IconTextButton
    {
        public IconTextButtonBeatSnap(EditScreen screen) : base(FontAwesome.Get(FontAwesomeIcon.fa_sun),
            FontManager.GetWobbleFont(Fonts.InterSemiBold), LocalizationManager.Get("Screen_Editor_BeatSnap"),
            (sender, args) => screen?.ActivateRightClickOptions(new BeatSnapRightClickOptions(screen.BeatSnap, EditScreen.AvailableBeatSnaps)))
        {
            Hovered += (sender, args) => screen?.ActivateTooltip(new Tooltip(
                LocalizationManager.Get("Screen_Editor_BeatSnapTooltip",
                    screen.InputManager.InputConfig.GetOrDefault(EditorKeybindActions.DecreaseSnap).ToDisplayString(),
                    screen.InputManager.InputConfig.GetOrDefault(EditorKeybindActions.IncreaseSnap).ToDisplayString()),
                ColorHelper.HexToColor("#808080")));
            LeftHover += (sender, args) => screen?.DeactivateTooltip();
        }
    }
}
