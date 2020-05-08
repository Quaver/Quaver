using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Results.UI.Footer
{
    public class ResultsFooterFixOffsetButton : IconTextButton
    {
        public ResultsFooterFixOffsetButton(ResultsScreen screen) : base(FontAwesome.Get(FontAwesomeIcon.fa_open_wrench_tool_silhouette),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Fix Offset", (sender, args) => screen.FixLocalOffset())
        {
        }
    }
}