using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Results.UI.Footer
{
    public class ResultsFooterConvertScoreButton : IconTextButton
    {
        public ResultsFooterConvertScoreButton(ResultsScreen screen) : base(FontAwesome.Get(FontAwesomeIcon.fa_refresh_arrow),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Convert Score", (sender, args) =>
            {
                screen.ActivateRightClickOptions(new ConvertScoreRightClickOptions(screen));
            })
        {
        }
    }
}