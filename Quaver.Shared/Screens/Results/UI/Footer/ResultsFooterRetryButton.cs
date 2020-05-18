using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Results.UI.Footer
{
    public class ResultsFooterRetryButton : IconTextButton
    {
        public ResultsFooterRetryButton(ResultsScreen screen) : base(FontAwesome.Get(FontAwesomeIcon.fa_play_button),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Retry Map", (sender, args) => screen.RetryMap())
        {
        }
    }
}