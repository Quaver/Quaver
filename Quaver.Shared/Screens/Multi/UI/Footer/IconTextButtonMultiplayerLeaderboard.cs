using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Screens.Selection.UI;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Multi.UI.Footer
{
    public class IconTextButtonMultiplayerLeaderboard : IconTextButton
    {
        public IconTextButtonMultiplayerLeaderboard(MultiplayerGameScreen screen)
            : base(FontAwesome.Get(FontAwesomeIcon.fa_trophy),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Leaderboard", (sender, args) =>
            {
                if (screen.ActiveLeftPanel.Value == SelectContainerPanel.Leaderboard)
                    screen.ActiveLeftPanel.Value = SelectContainerPanel.MatchSettings;
                else
                    screen.ActiveLeftPanel.Value = SelectContainerPanel.Leaderboard;
            })
        {
        }
    }
}