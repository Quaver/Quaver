using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Wobble;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Downloading.UI.Footer.Items
{
    public class DownloadRecommendDifficultyButton : IconTextButton
    {
        public DownloadRecommendDifficultyButton() : base(FontAwesome.Get(FontAwesomeIcon.fa_star),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Recommend Difficulty", (sender, args) =>
            {
                var game = GameBase.Game as QuaverGame;
                var screen = game?.CurrentScreen as DownloadingScreen;
                screen?.ShowRecommendedDifficultyDialog();
            })
        {
        }
    }
}