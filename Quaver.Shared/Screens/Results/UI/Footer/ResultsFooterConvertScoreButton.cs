using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Graphics.Notifications;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Results.UI.Footer
{
    public class ResultsFooterConvertScoreButton : IconTextButton
    {
        public ResultsFooterConvertScoreButton(ResultsScreen screen) : base(FontAwesome.Get(FontAwesomeIcon.fa_refresh_arrow),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Convert Score", (sender, args) =>
            {
                if (screen.IsConvertingScore)
                {
                    NotificationManager.Show(NotificationLevel.Warning, "Please wait! Your score is already being converted!");
                    return;
                }

                screen.ActivateRightClickOptions(new ConvertScoreRightClickOptions(screen));
            })
        {
        }
    }
}