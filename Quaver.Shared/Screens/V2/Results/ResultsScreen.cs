using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;

namespace Quaver.Shared.Screens.V2.Results
{
    internal sealed class ResultsScreen : PlaceholderScreen
    {
        public override QuaverScreenType Type { get; } = QuaverScreenType.Results;

        public ResultsScreen(ResultsScreenContext context) => View = new ResultsScreenView(this);

        public override UserClientStatus GetClientStatus() =>
            new UserClientStatus(ClientStatus.InMenus, -1, "", 1, "", 0);
    }
}
