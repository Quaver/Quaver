using System;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Dialogs;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.Footer
{
    public class IconTextButtonAddBookmark : IconTextButton
    {
        public IconTextButtonAddBookmark(EditScreen screen) : base(FontAwesome.Get(FontAwesomeIcon.fa_plus_black_symbol), 
            FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Editor_AddBookmark"),
            (o, e) => DialogManager.Show(new EditorBookmarkDialog(screen.ActionManager, screen.Track, null)))
        {
            var tooltip = new Tooltip(LocalizationManager.Get("Screen_Editor_AddBookmarkTooltip"),
                ColorHelper.HexToColor("#808080"));

            Hovered += (sender, args) => screen?.ActivateTooltip(tooltip);
            LeftHover += (sender, args) => screen?.DeactivateTooltip();
        }
    }
}
