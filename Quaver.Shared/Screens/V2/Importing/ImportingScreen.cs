using Quaver.Server.Client.Objects;
using LegacyMultiplayerScreen = Quaver.Shared.Screens.Multiplayer.MultiplayerScreen;

namespace Quaver.Shared.Screens.V2.Importing
{
    internal sealed class ImportingScreen : PlaceholderScreen
    {
        public override QuaverScreenType Type { get; } = QuaverScreenType.Importing;

        public ImportingScreen(LegacyMultiplayerScreen multiplayerScreen = null, bool fromSelect = false,
            bool fullSync = false, int? selectMapIdAfterImport = null) =>
            View = new ImportingScreenView(this);

        public override UserClientStatus GetClientStatus() => null;
    }
}
