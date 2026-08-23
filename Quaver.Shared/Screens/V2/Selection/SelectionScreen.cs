using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;
using Quaver.Shared.Screens.Selection.UI;
using Quaver.Shared.Screens.Selection.UI.Mapsets;

namespace Quaver.Shared.Screens.V2.Selection
{
    internal sealed class SelectionScreen : PlaceholderScreen
    {
        public override QuaverScreenType Type { get; } = QuaverScreenType.Select;

        public SelectionScreen(SelectScrollContainerType? activeScrollContainer = null,
            SelectContainerPanel activeLeftPanel = SelectContainerPanel.Leaderboard) =>
            View = new SelectionScreenView(this);

        public override UserClientStatus GetClientStatus() =>
            new UserClientStatus(ClientStatus.Selecting, -1, "", 0, "", 0);
    }
}
