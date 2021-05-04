using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Menu.Border.Components.Buttons;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Results;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Results.UI.Footer
{
    public class ResultsScreenFooter : MenuBorder
    {
        public ResultsScreenFooter(ResultsScreen screen) : base(MenuBorderType.Footer, new List<Drawable>
            {
                new ResultsFooterBackButton(screen),
                new IconTextButtonOptions(),
            }, new List<Drawable>())
        {
            if (screen.Processor.Value.Mods.HasFlag(ModIdentifier.Coop))
                return;

            if (OnlineManager.CurrentGame == null)
            {
                if (screen.ScreenType == ResultsScreenType.Gameplay && screen.Gameplay.LoadedReplay == null)
                    RightAlignedItems.Add(new ResultsFooterRetryButton(screen));

                RightAlignedItems.Add(new ResultsFooterWatchReplayButton(screen));
                RightAlignedItems.Add(new ResultsFooterExportReplayButton(screen));
            }

            if (screen.ScreenType == ResultsScreenType.Gameplay && screen.Gameplay.LoadedReplay == null)
                LeftAlignedItems.Add(new ResultsFooterFixOffsetButton(screen));

            if (screen.Replay != null)
                LeftAlignedItems.Add(new ResultsFooterConvertScoreButton(screen));

            AlignRightItems();
            AlignLeftItems();
        }
    }
}