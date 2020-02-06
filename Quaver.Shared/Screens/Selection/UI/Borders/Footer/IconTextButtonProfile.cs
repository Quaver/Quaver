using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Wobble.Bindables;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Borders.Footer
{
    public class IconTextButtonProfile : IconTextButton
    {
        public IconTextButtonProfile(Bindable<SelectContainerPanel> activeLeftPanel)
            : base(FontAwesome.Get(FontAwesomeIcon.fa_user_shape), FontManager.GetWobbleFont(Fonts.LatoBlack),
                "Profile", (sender, args) =>
                {
                    if (activeLeftPanel == null)
                        return;

                    if (activeLeftPanel.Value == SelectContainerPanel.UserProfile)
                        activeLeftPanel.Value = SelectContainerPanel.Leaderboard;
                    else
                        activeLeftPanel.Value = SelectContainerPanel.UserProfile;
                })
        {
        }
    }
}