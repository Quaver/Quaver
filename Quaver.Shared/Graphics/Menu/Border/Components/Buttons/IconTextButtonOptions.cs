using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Settings;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Buttons
{
    public class IconTextButtonOptions : IconTextButton
    {
        public IconTextButtonOptions() : base(FontAwesome.Get(FontAwesomeIcon.fa_settings),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Options", (sender, args) =>
            {
                DialogManager.Show(new SettingsDialog());
            })
        {
        }
    }
}