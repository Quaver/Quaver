using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Dialogs;
using Quaver.Shared.Screens.Selection.UI.Playlists.Dialogs.Create;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.Footer
{
    public class IconTextButtonTestPlay : IconTextButton
    {
        public IconTextButtonTestPlay(EditScreen screen) : base(FontAwesome.Get(FontAwesomeIcon.fa_play_button),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Test Play", (sender, args) => screen.ExitToTestPlay())
        {
            var tooltip = new Tooltip("Save and test play your map.\n" +
                         "Right-clicking will allow you to select modifers to use while test playing.\n" +
                         "Hotkey: F4", ColorHelper.HexToColor("#808080"));

            Hovered += (sender, args) => screen?.ActivateTooltip(tooltip);
            LeftHover += (sender, args) => screen?.DeactivateTooltip();

            RightClicked += (sender, args) => DialogManager.Show(new EditorModifierMenuDialog());
        }
    }
}