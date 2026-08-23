using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;

namespace Quaver.Shared.Screens.V2.Theater
{
    internal sealed class TheaterScreen : PlaceholderScreen
    {
        public override QuaverScreenType Type { get; } = QuaverScreenType.Theatre;

        public TheaterScreen() => View = new TheaterScreenView(this);

        public override UserClientStatus GetClientStatus() =>
            new UserClientStatus(ClientStatus.InMenus, -1, "-1", 1, "", 0);
    }
}
