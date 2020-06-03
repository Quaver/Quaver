using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Wobble.Bindables;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Borders.Footer
{
    public class IconTextButtonMapPreview : IconTextButton
    {
        public IconTextButtonMapPreview(Bindable<SelectContainerPanel> activeLeftPanel)
            : base(FontAwesome.Get(FontAwesomeIcon.fa_eye_open), FontManager.GetWobbleFont(Fonts.LatoBlack),
                "View Map", (sender, args) =>
                {
                    if (activeLeftPanel == null)
                        return;

                    if (activeLeftPanel.Value == SelectContainerPanel.MapPreview)
                        activeLeftPanel.Value = SelectContainerPanel.MatchSettings;
                    else
                        activeLeftPanel.Value = SelectContainerPanel.MapPreview;
                })
        {
        }
    }
}