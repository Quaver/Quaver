using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Screens.Selection;
using Wobble.Bindables;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Borders.Footer
{
    public class IconTextButtonProfile : IconTextButton
    {
        public IconTextButtonProfile(Bindable<SelectContainerPanel> activeLeftPanel)
            : base(FontAwesome.Get(FontAwesomeIcon.fa_user_shape), FontManager.GetWobbleFont(Fonts.InterBold),
                "Profile", (sender, args) =>
                {
                    if (activeLeftPanel == null)
                        return;

                    if (activeLeftPanel.Value == SelectContainerPanel.UserProfile)
                        activeLeftPanel.Value = SelectContainerPanel.Leaderboard;
                    else
                        activeLeftPanel.Value = SelectContainerPanel.UserProfile;
                }, localizationKey: SelectionLocalization.GetKey("Profile"))
        {
        }
    }
}
