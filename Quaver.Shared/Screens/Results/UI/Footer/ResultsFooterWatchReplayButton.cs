using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Results.UI.Footer
{
    public class ResultsFooterWatchReplayButton : IconTextButton
    {
        public ResultsFooterWatchReplayButton(ResultsScreen screen) : base(FontAwesome.Get(FontAwesomeIcon.fa_photo_camera),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Watch Replay", (sender, args) => screen.WatchReplay())
        {
        }
    }
}