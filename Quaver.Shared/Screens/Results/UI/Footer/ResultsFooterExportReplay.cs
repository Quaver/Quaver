using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Results.UI.Footer
{
    public class ResultsFooterExportReplayButton : IconTextButton
    {
        public ResultsFooterExportReplayButton(ResultsScreen screen) : base(FontAwesome.Get(FontAwesomeIcon.fa_archive_black_box),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Export Replay", (sender, args) => screen.ExportReplay())
        {
        }
    }
}